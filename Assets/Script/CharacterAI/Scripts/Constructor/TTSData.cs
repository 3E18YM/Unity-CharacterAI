using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

[System.Serializable]
public class TTSData
{
    // Start is called before the first frame update
    public float volume;
    public float pitch;
    public float rate;
    public string language;
    public string voiceID;
    public Gender gender;
    public TTSData(float Volume = 1, float Pitch = 1, float Rate = 1, string Language = "en-US", string VoiceID = "AnaNeural", Gender Gender = Gender.Male)
    {
        volume = Volume;
        pitch = Pitch;
        rate = Rate;
        language = Language;
        voiceID = VoiceID;
        gender = Gender;
    }
    public TTSData(TTSData tTSData)
    {
        volume = tTSData.volume;
        pitch = tTSData.pitch;
        rate = tTSData.rate;
        language = tTSData.language;
        voiceID = tTSData.voiceID;
        gender = tTSData.gender;
    }

}


[Serializable]
public class SpeechCommand
{
    public TTSData tTSData;
    public string content;
    public string transition;
    public string xml;
    public CommandType commandType;

    public SpeechCommand(string Content = null, string Transition = null, TTSData TTSData = null, string xml = null, CommandType commandType = CommandType.Null)
    {
        this.content = Content;
        this.transition = Transition;
        this.tTSData = TTSData ?? new TTSData();
        this.xml = xml;
        this.commandType = commandType == CommandType.Null ? CommandType.speech : commandType;
    }
    public SpeechCommand(SpeechCommand speechCommand, string Content = null, string Transition = null, TTSData TTSData = null, string xml = null, CommandType commandType = CommandType.Null)
    {
        this.content = Content ?? speechCommand.content;
        this.transition = Transition ?? speechCommand.transition;
        this.tTSData = TTSData ?? speechCommand.tTSData;
        this.xml = xml ?? speechCommand.xml;
        this.commandType = commandType == CommandType.Null ? speechCommand.commandType : commandType;



    }


}

public enum CommandType
{
    Null,
    speech,
    image,

}

