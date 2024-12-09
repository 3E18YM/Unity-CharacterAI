using System;
using UnityEngine.Analytics;

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
        public TtsData(TtsData tTsData)
        {
            volume = tTsData.volume;
            pitch = tTsData.pitch;
            rate = tTsData.rate;
            language = tTsData.language;
            voiceID = tTsData.voiceID;
            gender = tTsData.gender;
        }

    }


    [Serializable]
    public class SpeechCommand
    {
        public TtsData tTsData;
        public string content;
        public string transition;
        public string xml;
        public CommandType commandType;

        public SpeechCommand(string content = null, string transition = null, TtsData ttsData = null, string xml = null, CommandType commandType = CommandType.Null)
        {
            this.content = content;
            this.transition = transition;
            this.tTsData = ttsData ?? new TtsData();
            this.xml = xml;
            this.commandType = commandType == CommandType.Null ? CommandType.Speech : commandType;
        }
        public SpeechCommand(SpeechCommand speechCommand, string content = null, string transition = null, TtsData ttsData = null, string xml = null, CommandType commandType = CommandType.Null)
        {
            this.content = content ?? speechCommand.content;
            this.transition = transition ?? speechCommand.transition;
            this.tTsData = ttsData ?? speechCommand.tTsData;
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