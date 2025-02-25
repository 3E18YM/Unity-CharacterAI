
using System.Collections.Generic;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt.Messages;
using M2MqttUnity;
using System;
using System.Threading.Tasks;
using Zenject;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Linq;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace CharacterAI.Modules.WebduinoAI
{
    public class mqttReceiver : M2MqttUnityClient, IAIServices, IInitializable
{

    [InjectOptional] StatusManage StatusManage;
    [Header("MQTT topics")]
    public string messagePublish = "";

    [Tooltip("Set this to true to perform a testing cycle automatically on startup")]
    public bool autoTest = false;

    //using C# Property GET/SET and event listener to reduce Update overhead in the controlled objects
    private string m_msg;

    [Header("MQTT Config")] 
    public MqttConfig mqttConfig;
    public string receiveTopic;
    // message to publish
    public string publishTopic; // topic to publish
    public string assistantID;
    private string GuID;
    
    public string msg
    {
        get
        {
            return m_msg;
        }

        set
        {
            if (m_msg == value) return;
            m_msg = value;
            if (OnMessageArrived != null)
            {
                OnMessageArrived(m_msg);
            }
        }
    }

    public event OnMessageArrivedDelegate OnMessageArrived;
    public delegate void OnMessageArrivedDelegate(string newMsg);

    private bool m_isConnected;

    public bool isConnected
    {
        get
        {
            return m_isConnected;
        }
        set
        {
            if (m_isConnected == value) return;
            m_isConnected = value;
            if (OnConnectionSucceeded != null)
            {
                OnConnectionSucceeded(isConnected);
            }
        }
    }
    public event OnConnectionSucceededDelegate OnConnectionSucceeded;
    public event Action<string> OnMessage;

    public delegate void OnConnectionSucceededDelegate(bool isConnected);

    // a list to store the messages
    private List<string> eventMessages = new List<string>();
    private CancellationTokenSource cancellationTokenSource;

    private float RESPONSE_TIMEOUT_SECONDS = 2f;
  
    
    public void Initialize()
    {
        base.Awake();
        base.Start();
        GuID = Guid.NewGuid().ToString();
        brokerAddress = mqttConfig.brokerAddress;
        brokerPort = mqttConfig.brokerPort;
        receiveTopic = mqttConfig.receiveTopic + GuID;
        publishTopic = mqttConfig.publishTopic + GuID;
        assistantID = mqttConfig.assistantID;
        isEncrypted = mqttConfig.isEncrypted;
        connectionDelay = mqttConfig.connectionDelay;
        timeoutOnConnection = mqttConfig.timeoutOnConnection;
        autoConnect = mqttConfig.autoConnect;
        mqttUserName = mqttConfig.mqttUserName;
        mqttPassword = mqttConfig.mqttPassword;
        RESPONSE_TIMEOUT_SECONDS = mqttConfig.RESPONSE_TIMEOUT_SECONDS;
    }
    public async Task<string> SendMessages(string message)
    {
        StatusManage?.EnableStatus(Status.SendToAI);
        cancellationTokenSource = new CancellationTokenSource();
        await Connect();
        SendToMqtt(message);
        await Task.Yield();

        int previousCount = eventMessages.Count;
        Stopwatch stopwatch = Stopwatch.StartNew();

        while (!cancellationTokenSource.Token.IsCancellationRequested)
        {
            await Task.Yield();
            ProcessMqttEvents();

            int currentCount = eventMessages.Count;
            if (currentCount == 0)
            {
                continue;
            }
            else if (currentCount != previousCount)
            {
                previousCount = currentCount;
                stopwatch.Restart();
                Debug.Log($"{eventMessages.Last()}");
            }
            else if (stopwatch.Elapsed.TotalSeconds > RESPONSE_TIMEOUT_SECONDS)
            {

                break;
            }


        }
        Debug.Log($"等待超時{stopwatch.Elapsed.TotalSeconds}");
        StatusManage?.DisableStatus(Status.SendToAI);
        string _msg = string.Join("", eventMessages);
        eventMessages.Clear();
        return _msg;
    }

    public void ForceStop()
    {
        StatusManage?.DisableStatus(Status.SendToAI);
        cancellationTokenSource?.Cancel();
    }
    public void SetAssistant(string assistantID)
    {

        this.assistantID = assistantID;
        //Debug.Log($"assistantID：{assistantID}ReceiveTopic: {receiveTopic}PublishTopic: {publishTopic}");

    }
    public void Publish()
    {

        client.Publish(publishTopic, System.Text.Encoding.UTF8.GetBytes(""), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);

    }
    public void SendToMqtt(string Message)
    {

        if (!string.IsNullOrEmpty(Message))
        {

            Debug.Log($"{publishTopic}：{assistantID}：{Message}");
            client.Publish(publishTopic, System.Text.Encoding.UTF8.GetBytes(assistantID + ": " + Message), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);

        }
        SubscribeTopics();

    }
    public void SetEncrypted(bool isEncrypted)
    {
        this.isEncrypted = isEncrypted;
    }

    protected override void OnConnecting()
    {
        base.OnConnecting();
    }

    protected override void OnConnected()
    {
        base.OnConnected();
        isConnected = true;

        if (autoTest)
        {
            Publish();
        }
    }

    protected override void OnConnectionFailed(string errorMessage)
    {
        Debug.Log("CONNECTION FAILED! " + errorMessage);
    }

    protected override void OnDisconnected()
    {
        Debug.Log("Disconnected.");
        isConnected = false;
    }

    protected override void OnConnectionLost()
    {
        Debug.Log("CONNECTION LOST!");
    }

    protected override void SubscribeTopics()
    {
        client.Subscribe(new string[] { receiveTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
        //Debug.Log("subscribe Topic:" + receiveTopic);

    }

    protected override void UnsubscribeTopics()
    {
        client.Unsubscribe(new string[] { receiveTopic });
    }



    protected override void DecodeMessage(string topic, byte[] message)
    {
        msg = System.Text.Encoding.UTF8.GetString(message);

        if (autoTest)
        {
            autoTest = false;
            Disconnect();
        }

        StatusManage.ExecuteStatusEvent(Status.AIReply, msg);
        OnMessage?.Invoke(msg);
        //Debug.Log($"{receiveTopic}: {msg}");
        StoreMessage(msg);




    }
    private void StoreMessage(string eventMsg)
    {
        // if (eventMessages.Count > 10)
        // {
        //     eventMessages.RemoveAt(0);
        // }
        eventMessages.Add(eventMsg);
    }

    private void OnDestroy()
    {
        Disconnect();
    }

    private void OnValidate()
    {
        if (autoTest)
        {
            autoConnect = true;
        }
    }

    protected override void Update()
    {
    }

}

}
