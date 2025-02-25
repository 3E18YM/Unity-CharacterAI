using UnityEngine;

[CreateAssetMenu(menuName = "Ai_Companion/mqttConfig")]
public class MqttConfig : ScriptableObject
{
    [Header("MQTT broker configuration")]
    [Tooltip("IP address or URL of the host running the broker")]
    public string brokerAddress = "mqtt.webduino.io";
    [Tooltip("Port where the broker accepts connections")]
    public int brokerPort = 1883;
    [Tooltip("Use encrypted connection")]
    public string receiveTopic = "kn@chat_completion/guest_";
    public string publishTopic = "kn@chat_prompt/guest_";
    public bool isEncrypted = false;
    [Header("Connection parameters")]
    [Tooltip("Connection to the broker is delayed by the the given milliseconds")]
    public int connectionDelay = 500;
    [Tooltip("Connection timeout in milliseconds")]
    public int timeoutOnConnection = 30000;
    [Tooltip("Connect on startup")]
    public bool autoConnect = false;
    [Tooltip("UserName for the MQTT broker. Keep blank if no user name is required.")]
    public string mqttUserName = "webduino";
    [Tooltip("Password for the MQTT broker. Keep blank if no password is required.")]
    public string mqttPassword = "webduino";
    // topic to publish
    public string assistantID;
    [Tooltip("等待超時，結束SendMessage 任務")]
    public float RESPONSE_TIMEOUT_SECONDS = 2f;
}