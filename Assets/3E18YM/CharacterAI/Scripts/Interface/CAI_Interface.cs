using System;
using System.Threading.Tasks;
using UnityEngine;
namespace CharacterAI
{
    public interface IAIServices
    {
        public Task<string> SendMessages(string message);
        public void ForceStop();
        public void SetAssistant(string assistantID);
        public event Action<string> OnMessage;
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
        public Task Speak(string text, double rate = 1, double pitch = 1, double volume = 1, string language = null, string voiceID = null);
        public Task ForceStop();

    }
  
}



