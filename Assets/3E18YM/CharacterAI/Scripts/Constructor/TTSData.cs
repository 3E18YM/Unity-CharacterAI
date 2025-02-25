using System;
using UnityEngine.Analytics;
using Microsoft.CognitiveServices.Speech.PronunciationAssessment;

namespace CharacterAI
{
    [System.Serializable]
    public class TtsData
    {
        // Start is called before the first frame update
        public float volume;
        public float pitch;
        public float rate;
        public string language;
        public string voiceID;
        public Gender gender;
        public TtsData(float volume = 1, float pitch = 1, float rate = 1, string language = "en-US", string voiceID = "AnaNeural", Gender gender = Gender.Male)
        {
            this.volume = volume;
            this.pitch = pitch;
            this.rate = rate;
            this.language = language;
            this.voiceID = voiceID;
            this.gender = gender;
        }
        public TtsData(TtsData TtsData)
        {
            volume = TtsData.volume;
            pitch = TtsData.pitch;
            rate = TtsData.rate;
            language = TtsData.language;
            voiceID = TtsData.voiceID;
            gender = TtsData.gender;
        }

    }
    [Serializable]
    public class PronunciationData
    {
        public string languageCode;
        public GradingSystem GradingSystem;
        public Granularity Granularity;
        public bool enableMiscue;

        public PronunciationData(string languageCode = default, GradingSystem gradingSystem = default, Granularity granularity = default, bool enableMiscue = true)
        {
            this.languageCode = languageCode;
            GradingSystem = gradingSystem;
            Granularity = granularity;
            this.enableMiscue = enableMiscue;
        }
    }

    [Serializable]
    public class SpeechCommand
    {
        public TtsData TtsData;
        public string content;
        public string transition;
        public string xml;
        public CommandType commandType;

        public SpeechCommand(string content = null, string transition = null, TtsData TtsData = null, string xml = null, CommandType commandType = CommandType.Null)
        {
            this.content = content;
            this.transition = transition;
            this.TtsData = TtsData ?? new TtsData();
            this.xml = xml;
            this.commandType = commandType == CommandType.Null ? CommandType.Speech : commandType;
        }
        public SpeechCommand(SpeechCommand speechCommand, string content = null, string transition = null, TtsData TtsData = null, string xml = null, CommandType commandType = CommandType.Null)
        {
            this.content = content ?? speechCommand.content;
            this.transition = transition ?? speechCommand.transition;
            this.TtsData = TtsData ?? speechCommand.TtsData;
            this.xml = xml ?? speechCommand.xml;
            this.commandType = commandType == CommandType.Null ? speechCommand.commandType : commandType;



        }


    }

    public enum CommandType
    {
        Null,
        Speech,
        Image,

    }
}