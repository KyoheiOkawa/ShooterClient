using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System;

public class PlayerNetworkController : MonoBehaviour
{
    public delegate void HitBulletDelegate(BulletType.Type bulletType, int fireFrame);

    [SerializeField]
    Player player;

    [SerializeField]
    Player rival;

    [SerializeField]
    MobileInput mobileInput;

    [SerializeField]
    GameObject messagePanelPrefab;

    GameObject msgPanel;

    [SerializeField]
    int frameTolerance = 3;

    [SerializeField]
    float timeOutSec = 5.0f;

    float timeOutCount = 0.0f;

    List<RequesJsons.InputObj> stockedInputObj = new List<RequesJsons.InputObj>();

    UDPManager udpManager;

    RequesJsons.UserObj playerInfo;
    RequesJsons.UserObj rivalInfo;

    Queue<RequesJsons.InputObj> playerInputQueue = new Queue<RequesJsons.InputObj>();
    Queue<RequesJsons.InputObj> rivalInputQueue = new Queue<RequesJsons.InputObj>();

    List<RequesJsons.HitBulletRequest> playerHitInfoList = new List<RequesJsons.HitBulletRequest>();
    List<RequesJsons.HitBulletRequest> rivalHitInfoList = new List<RequesJsons.HitBulletRequest>();

    RequesJsons.InputObj nowFrameInputInfo = new RequesJsons.InputObj();

    //まだ受け取っていない次の相手の入力情報のフレーム
    int nextRivalInputFrame = 0;
    //現在ゲーム開始してから何フレーム経過したか
    int nowFrameCount = 0;

    int rivalRequireFrame = 0;

    void Start()
    {
        udpManager = UDPManager.Instance;
        udpManager.messageReceived += OnMessageReceived;

        mobileInput.StickAxisEvent += OnStickAxis;
        mobileInput.FireEvent += OnFire;

        CreateUserInfoObject();

        nowFrameInputInfo.posX = player.transform.position.x;
        nowFrameInputInfo.posY = player.transform.position.y;
        nowFrameInputInfo.rotZ = player.transform.rotation.eulerAngles.z;

        player.hitBulletEvent += AddAndSendHitBulletInfo;
        rival.hitBulletEvent += AddAndSendHitBulletInfo;

    }

    void Update()
    {
        if (msgPanel)
            return;

        if (playerInputQueue.Count > frameTolerance &&
            rivalInputQueue.Count <= 0)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
    }

    void LateUpdate()
    {
        UpdateSync();
    }

    void UpdateSync()
    {
        if (msgPanel)
            return;

        nowFrameInputInfo.posX = player.transform.position.x;
        nowFrameInputInfo.posY = player.transform.position.y;
        nowFrameInputInfo.rotZ = player.transform.rotation.eulerAngles.z;

        if (Time.timeScale <= 0)
        {
            if(nowFrameCount < rivalRequireFrame)
            {
                QueueOwnInput();
                SendInput();
                nowFrameCount++;
            }

            timeOutCount += Time.unscaledDeltaTime;
            if (timeOutCount > timeOutSec)
            {
                var canvas = GameObject.Find("Canvas");
                msgPanel = Instantiate(messagePanelPrefab, canvas.transform);
                msgPanel.GetComponent<MessagePanel>().Message = "通信エラーが発生しました\nタイトル画面に戻ります";
                msgPanel.GetComponent<MessagePanel>().TapAction = () =>
                {
                    SceneManager.LoadScene("Menu");
                };
            }

            return;
        }

        if (playerInputQueue.Count > 0)
        {
            if (rivalInputQueue.Count > 0)
            {
                RequesJsons.InputObj myInputObj = playerInputQueue.Dequeue();
                player.Move(new Vector2(myInputObj.axisX, myInputObj.axisY));
                if (myInputObj.isFire)
                    player.Fire(new Vector2(myInputObj.fireDirX,myInputObj.fireDirY),myInputObj.frame);

                RequesJsons.InputObj rivalInputObj = rivalInputQueue.Dequeue();
                rival.GetComponent<Player>().FixPositionRot(new Vector2(-rivalInputObj.posX, -rivalInputObj.posY), rivalInputObj.rotZ+180.0f);
                rival.Move(new Vector2(-rivalInputObj.axisX, -rivalInputObj.axisY));
                if (rivalInputObj.isFire)
                    rival.Fire(new Vector2(-rivalInputObj.fireDirX,-rivalInputObj.fireDirY),rivalInputObj.frame);
            }
        }

        if (playerInputQueue.Count < frameTolerance)
        {
            QueueOwnInput();
            SendInput();
            nowFrameCount++;

            timeOutCount = 0.0f;
        }
        else
        {
            SendInput();

            timeOutCount += Time.unscaledDeltaTime;
            if (timeOutCount > timeOutSec)
            {
                var canvas = GameObject.Find("Canvas");
                msgPanel = Instantiate(messagePanelPrefab, canvas.transform);
                msgPanel.GetComponent<MessagePanel>().Message = "通信エラーが発生しました\nタイトル画面に戻ります";
                msgPanel.GetComponent<MessagePanel>().TapAction = () =>
                {
                    SceneManager.LoadScene("Menu");
                };
            }
        }
    }

    void OnDestroy()
    {
        udpManager.messageReceived -= OnMessageReceived;

        mobileInput.StickAxisEvent -= OnStickAxis;
        mobileInput.FireEvent -= OnFire;

        player.hitBulletEvent -= AddAndSendHitBulletInfo;
        rival.hitBulletEvent -= AddAndSendHitBulletInfo;
    }

    void OnMessageReceived(JsonNode jsonNode, string jsonStr)
    {
        string type = jsonNode["type"].Get<string>();
        if (type == "rival-input")
        {
            RequesJsons.RivalInputReturn inputReturn = JsonUtility.FromJson<RequesJsons.RivalInputReturn>(jsonStr);
            rivalRequireFrame = inputReturn.requireNextFrame;
            foreach(JsonNode inputStr in jsonNode["inputObjects"])
            {
                RequesJsons.InputObj inputObj = JsonUtility.FromJson<RequesJsons.InputObj>(inputStr.Get<string>());
                if(nextRivalInputFrame==inputObj.frame)
                {
                    rivalInputQueue.Enqueue(inputObj);
                    nextRivalInputFrame++;
                }
            }
        }
        else if(type == "hit-bullet")
        {
            RequesJsons.HitBulletRequest hitBullet = JsonUtility.FromJson<RequesJsons.HitBulletRequest>(jsonStr);
            hitBullet.FromJson();

            rivalHitInfoList.Add(hitBullet);
            CheckHitBulletList();
        }
    }

    void OnStickAxis(Vector2 axis)
    {
        nowFrameInputInfo.axisX = axis.x;
        nowFrameInputInfo.axisY = axis.y;
    }

    void OnFire()
    {
        nowFrameInputInfo.isFire = true;
        nowFrameInputInfo.fireDirX = player.transform.up.x;
        nowFrameInputInfo.fireDirY = player.transform.up.y;
    }

    void CreateUserInfoObject()
    {
        playerInfo = new RequesJsons.UserObj();
        playerInfo.name = PlayerPrefs.GetString("MyName");
        playerInfo.id = PlayerPrefs.GetString("ID");

        rivalInfo = new RequesJsons.UserObj();
        rivalInfo.name = PlayerPrefs.GetString("RivalName");
        rivalInfo.id = PlayerPrefs.GetString("RivalID");
        rivalInfo.port = PlayerPrefs.GetString("RivalPort");
        rivalInfo.address = PlayerPrefs.GetString("RivalAddress");
    }

    void QueueOwnInput()
    {
        nowFrameInputInfo.frame = nowFrameCount;
        playerInputQueue.Enqueue(nowFrameInputInfo.Clone());
        stockedInputObj.Add(nowFrameInputInfo.Clone());

        List<RequesJsons.InputObj> removeItems = new List<RequesJsons.InputObj>();
        foreach(var inputObj in stockedInputObj)
        {
            if (inputObj.frame < rivalRequireFrame)
                removeItems.Add(inputObj);
        }
        foreach(var removeItem in removeItems)
        {
            stockedInputObj.Remove(removeItem);
        }

        nowFrameInputInfo.Reset();
    }

    void SendInput()
    {
        RequesJsons.PlayInputRequest sendData = new RequesJsons.PlayInputRequest();
        sendData.ownObj = playerInfo;
        sendData.rivalObj = rivalInfo;
        sendData.requireNextFrame = nextRivalInputFrame;
        foreach (var inputInfo in stockedInputObj)
        {
            sendData.inputObjectsObj.Add(inputInfo);
        }

        udpManager.SendJson(sendData.ToJson());
    }

    void AddAndSendHitBulletInfo(BulletType.Type type,int fireFrame)
    {
        Debug.Log("HIT:" + type.ToString());

        RequesJsons.HitBulletRequest hitBulletRequest = new RequesJsons.HitBulletRequest();
        hitBulletRequest.ownObj = playerInfo;
        hitBulletRequest.rivalObj = rivalInfo;
        hitBulletRequest.fireFrame = fireFrame;

        hitBulletRequest.bulletType = type.ToString();
        playerHitInfoList.Add(hitBulletRequest.Clone());

        switch (type)
        {
            //自分の弾が相手に当たった
            case BulletType.Type.RED:
                {
                    hitBulletRequest.bulletType = BulletType.Type.BLUE.ToString();
                    udpManager.SendJson(hitBulletRequest.Clone().ToJson());
                    break;
                }
            //相手の弾が自分に当たった
            case BulletType.Type.BLUE:
                {
                    hitBulletRequest.bulletType = BulletType.Type.RED.ToString();
                    udpManager.SendJson(hitBulletRequest.Clone().ToJson());
                    break;
                }
        }

        CheckHitBulletList();
    }

    void CheckHitBulletList()
    {
        List<RequesJsons.HitBulletRequest> sameList = new List<RequesJsons.HitBulletRequest>();
        List<RequesJsons.HitBulletRequest> deleteList = new List<RequesJsons.HitBulletRequest>();

        foreach(var playerHit in playerHitInfoList)
        {
            int frame = playerHit.fireFrame;
            BulletType.Type type = playerHit.GetBulletType();
            foreach(var rivalHit in rivalHitInfoList)
            {
                if(frame == rivalHit.fireFrame &&
                    type == rivalHit.GetBulletType())
                {
                    sameList.Add(playerHit);
                    deleteList.Add(rivalHit);
                }
            }
        }

        foreach (var sameObj in sameList)
        {
            BulletType.Type type = sameObj.GetBulletType();
            switch(type)
            {
                case BulletType.Type.RED:
                    {
                        rival.Damage();
                        break;
                    }
                case BulletType.Type.BLUE:
                    {
                        player.Damage();
                        break;
                    }
            }
            playerHitInfoList.Remove(sameObj);
        }

        foreach(var deleteObj in deleteList)
        {
            rivalHitInfoList.Remove(deleteObj);
        }
    }
}
