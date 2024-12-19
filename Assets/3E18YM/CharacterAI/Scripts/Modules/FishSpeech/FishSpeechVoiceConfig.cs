namespace CharacterAI.Modules.FishSpeech
{
    [System.Serializable]
    public class FishSpeechVoiceConfig
    {       
        public string name;
        public string id;
        public string referenceSource;
        public string referenceText;
        
        public FishSpeechVoiceConfig(string name = null, string id = null, string referenceSource = null, string referenceText = null)
        {
            this.name = name;
            this.id = id;
            this.referenceSource = referenceSource;
            this.referenceText = referenceText;
        }
    }
}