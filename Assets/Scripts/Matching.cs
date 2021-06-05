using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

public class Matching : MonoBehaviour
{
    UDPManager udpManager;

    [SerializeField]
    Text playerNameText;

    [SerializeField]
    InputField playerInputField;

    [SerializeField]
    GameObject messagePanelPrefab;

    [SerializeField]
    GameObject loadingPanelPrefab;

    bool isNowMatching = false;

    [SerializeField]
    float matchingTimeOut = 6.0f;

    float matchingTimeOutCount = 0.0f;

    GameObject loadingPanel;

    GameObject canvas;

    Queue<Action> mainThreadQueue = new Queue<Action>();

    bool isPlayerInfoOK = false;
    bool isMatchingOK = false;

    void Start()
    {
        string playerName = playerNameText.text;
        if (PlayerPrefs.GetString("MyName") != "")
        {
            playerName = PlayerPrefs.GetString("MyName");
            playerInputField.text = playerName;
        }
        PlayerPrefs.DeleteAll();
        PlayerPrefs.SetString("MyName", playerName);

        udpManager = UDPManager.Instance;
        udpManager.messageReceived += OnReceiveMessage;

        canvas = GameObject.Find("Canvas");
    }

    void Update()
    {
        while(mainThreadQueue.Count > 0)
        {
            Action action = mainThreadQueue.Dequeue();
            action();
        }

        if(isNowMatching)
        {
            matchingTimeOutCount += Time.deltaTime;
            if(matchingTimeOutCount > matchingTimeOut)
            {
                matchingTimeOutCount = 0.0f;
                isNowMatching = false;
                EnqueueMessagePanel("通信エラーです");
            }
        }
    }

    private void OnDestroy()
    {
        if (udpManager == null)
            return;

        udpManager.messageReceived -= OnReceiveMessage;
    }

    void OnReceiveMessage(JsonNode jsonNode,string jsonStr)
    {
        string type = jsonNode["type"].Get<string>();

        if (type == "playerInfo")
        {
            Action action = () =>
            {
                PlayerPrefs.SetString("MyName", jsonNode["name"].Get<String>());
                PlayerPrefs.SetString("ID", jsonNode["id"].Get<String>());
                isPlayerInfoOK = true;
                CheckMatching();
            };
            mainThreadQueue.Enqueue(action);
        }
        else if(type == "success-match")
        {
            Action action = () =>
            {
                PlayerPrefs.SetString("RivalName", jsonNode["rival"]["name"].Get<string>());
                PlayerPrefs.SetString("RivalID", jsonNode["rival"]["id"].Get<string>());
                PlayerPrefs.SetString("RivalPort", jsonNode["rival"]["port"].Get<string>());
                PlayerPrefs.SetString("RivalAddress", jsonNode["rival"]["address"].Get<string>());
                isMatchingOK = true;
                CheckMatching();
            };
            mainThreadQueue.Enqueue(action);
        }
        else if(type == "not-match")
        {
            matchingTimeOutCount = 0.0f;
            isNowMatching = false;
            EnqueueMessagePanel("マッチングしませんでした");
        }
        else if(type == "error")
        {
            matchingTimeOutCount = 0.0f;
            isNowMatching = false;
            EnqueueMessagePanel("ERROR!!\n" + jsonNode["msg"].Get<String>());
        }
    }

    public void StartMatching()
    {
        if (playerNameText.text == "")
        {
            var messagePanel = Instantiate(messagePanelPrefab, canvas.transform);
            messagePanel.GetComponent<MessagePanel>().Message = "名前を入力してください";
            return;
        }

        var data = new RequesJsons.MatchRequest();
        data.name = playerNameText.text;

        udpManager.SendJson(JsonUtility.ToJson(data));

        loadingPanel = Instantiate(loadingPanelPrefab, canvas.transform);

        isNowMatching = true;
    }

    private void EnqueueMessagePanel(string msg)
    {
        Action action = () =>
        {
            var messagePanel = Instantiate(messagePanelPrefab, canvas.transform);
            messagePanel.GetComponent<MessagePanel>().Message = msg;
            if (loadingPanel)
                Destroy(loadingPanel);
        };
        mainThreadQueue.Enqueue(action);
    }

    private void CheckMatching()
    {
        if(isMatchingOK && isPlayerInfoOK)
        {
            //string msg = "マッチング成功\n";
            //msg += "相手プレイヤー:" + PlayerPrefs.GetString("RivalName");
            //EnqueueMessagePanel(msg);
            SceneManager.LoadScene("TestStage");
        }
    }
}
