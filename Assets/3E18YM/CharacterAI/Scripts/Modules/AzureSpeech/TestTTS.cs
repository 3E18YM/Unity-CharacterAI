using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.PronunciationAssessment;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;
namespace CharacterAI.Modules.AzureSpeech
{
    public class TestTTS: ITextToSpeech
    {   
        [InjectOptional] private StatusManage StatusManage;
        [InjectOptional] public AudioSource audioSource = null;      
        private AzureSpeechConfig azureSetting;
        private SpeechConfig config;
        private PronunciationAssessmentConfig pronunciationAssessmentConfig;
        private SpeechSynthesizer synthesizer;
        
        public TestTTS(AzureSpeechConfig azureSetting)
        {
            this.azureSetting = azureSetting;
            config = azureSetting.GetSpeechConfig();
            //pronunciationAssessmentConfig = azureSetting.GetPronunciationAssessmentConfig();
        }
        public Task Speak(string text, double rate = 1, double pitch = 1, double volume = 1, string language = null, string voiceID = null)
        {
            throw new System.NotImplementedException();
        }

        public Task ForceStop()
        {
            throw new System.NotImplementedException();
        }
    }
}