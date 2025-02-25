using System;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.PronunciationAssessment;
using UnityEngine;

namespace CharacterAI.Modules.AzureSpeech
{
    [CreateAssetMenu(menuName = "Ai_Companion/AzureConfig")]
    public class AzureConfig : ScriptableObject
    {
        public AzureAIConfig azureAIConfig;
        public AzureSpeechConfig azureSpeechConfig;
    }
    [Serializable]
    public class AzureSpeechConfig
    {
        [SerializeField] private string subscriptionKey;
        [SerializeField] private string serviceRegion;


        public TtsData TtsData;
        public string sTTLanguage;
        public PronunciationData pronunciationData;

        public AzureSpeechConfig(string subscriptionKey = null, string serviceRegion = null, TtsData TtsData = null, string sTTLanguage = "en-US", PronunciationData pronunciationData = null)
        {
            this.subscriptionKey = subscriptionKey;
            this.serviceRegion = serviceRegion;
            this.TtsData = TtsData ?? new TtsData();
            this.sTTLanguage = sTTLanguage;
            this.pronunciationData = pronunciationData ?? new PronunciationData();
        }


        public SpeechConfig GetSpeechConfig()
        {
            var speechconfig = SpeechConfig.FromSubscription(this.subscriptionKey, serviceRegion);
            return speechconfig;
        }

        public PronunciationAssessmentConfig GetPronunciationAssessmentConfig()
        {
            var pronunciationAssessmentConfig = new PronunciationAssessmentConfig(
                referenceText: "",
                gradingSystem: pronunciationData.GradingSystem,
                granularity: pronunciationData.Granularity,
                enableMiscue: pronunciationData.enableMiscue);
            return pronunciationAssessmentConfig;
        }
    }

    [Serializable]
    public class AzureAIConfig
    {
        [SerializeField] private string endPoint;
        [SerializeField] private string KEY;
        public AzureAIConfig(string endPoint, string key)
        {
            this.endPoint = endPoint;
            KEY = key;

        }
       
    }
}