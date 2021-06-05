using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using MiniJSON;

public class UDPManager : MonoBehaviour
{
    private static UDPManager instance = null;
    public static UDPManager Instance
    {
        get
        {
            if (instance)
            {
                return instance;
            }
            else
            {
                instance = GameObject.FindObjectOfType<UDPManager>();
                if (!instance)
                {
                    var newObj = new GameObject();
                    newObj.name = "UDPManager";
                    newObj.AddComponent<UDPManager>();
                    Instantiate(newObj);
                    instance = newObj.GetComponent<UDPManager>();
                }
                return instance;
            }
        }
    }

    [SerializeField]
    string host = "localhost";
    [SerializeField]
    int port = 33333;

    private UdpClient client;
    private Thread thread;

    public delegate void MessageReceived(JsonNode jsonNode, string jsonStr);
    public event MessageReceived messageReceived;

    private void Awake()
    {
        if (this != Instance)
        {
            Destroy(this.gameObject);
            return;
        }

        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        client = new UdpClient();
        client.Connect(host, port);

        thread = new Thread(new ThreadStart(ThreadMethod));
        thread.Start();
    }

    void Update()
    {

    }

    private void OnApplicationFocus(bool focus)
    {
        if (client == null)
            return;

        if(focus)
        {
            client.Connect(host, port);
        }
    }

    void OnApplicationQuit()
    {
        client.Close();
        thread.Abort();

        if (this == Instance) instance = null;
    }

    private void ThreadMethod()
    {
        while (true)
        {
            if (!client.Client.Connected)
                client.Connect(host, port);

            IPEndPoint remoteEP = null;
            byte[] data = client.Receive(ref remoteEP);
            string text = Encoding.UTF8.GetString(data);

            if (messageReceived != null)
            {
                JsonNode jsonNode = JsonNode.Parse(text);
                messageReceived(jsonNode, text);
            }

            Debug.Log("GET:" + text);
        }
    }

    public void SendJson(string jsonStr)
    {
        if(!client.Client.Connected)
            client.Connect(host, port);

        byte[] dgram = Encoding.UTF8.GetBytes(jsonStr);
        client.Send(dgram, dgram.Length);

        Debug.Log("SEND:" + jsonStr);
    }
}