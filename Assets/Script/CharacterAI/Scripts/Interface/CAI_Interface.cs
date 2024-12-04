using System.Threading.Tasks;
using UnityEngine;

public interface IAIServices
{
    public Task<string> SendMessages(string message);
    public void SetAssistant(string assistantID);
    public void ForceStop();
}
public interface ISpeechToText
{
    public Task<string> Listen(AudioClip _clip = null, string defaultLanguage = null);
    public Task<string> ListenStreaming(float autoStop = -1, string defaultLanguage = null);
    public void ForceStop();
    public string currentDetectLanguage { get; set; }

}
public interface ITextToSpeech
{
    public AudioSource _audioSource { get; set; }
    public Task Speak(string text, double rate = 1, double pitch = 1, double volume = 1, string language = null, string voiceID = null);
    public Task ForceStop();


}
public interface IData
{

}
public interface IMessage
{

    public Task Add(string text, float speed = 0.01f);
    public void DeleteLast();
    public void DeleteAll();



}



