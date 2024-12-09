using System;
using System.Threading.Tasks;
using UnityEngine;

namespace CharacterAI
{
    [Serializable]
    public abstract class IaiServices
    {
        public abstract Task<string> SendMessages(string message);
        public virtual void SetAssistant(string assistantID){}
        public abstract void ForceStop();
    }
    [Serializable]
    public abstract class SpeechToText
    {
        public abstract Task<string> Listen(AudioClip clip = null, string defaultLanguage = null);
        public abstract Task<string> ListenStreaming(float autoStop = -1, string defaultLanguage = null);
        public abstract void ForceStop();
        public  string currentDetectLanguage = "en-US";
    
    }
    [Serializable]
    public abstract class TextToSpeech
    {
        public AudioSource audioSource;
        public abstract Task Speak(string text, double rate = 1, double pitch = 1, double volume = 1, string language = null, string voiceID = null);
        public abstract Task ForceStop();
    
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
}