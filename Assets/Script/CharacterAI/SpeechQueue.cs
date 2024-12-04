using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using Zenject;
[Serializable]
public class SpeechQueue
{
    private ITextToSpeech textToSpeech;
    [InjectOptional] private Status_Manage status_Manage;
    #region SentenceSetting"
    [Header("SentenceSetting")][SerializeField] private int sentenceLength = 50;
    [SerializeField] private HashSet<char> specialCharacters = new HashSet<char> { '。', '.', ',', '，', '?', '？', '!', '！', ':', '：' };
    #endregion
    public List<SpeechCommand> tempCommands = new List<SpeechCommand>();
    Dictionary<CommandType, Status> keyValuePairs = new Dictionary<CommandType, Status>()
    {
        { CommandType.speech, Status.RichText },
        { CommandType.image, Status.ImageURL }
    };
    bool isSpeech;
    private object lockObject = new object();
    private bool pause;
    private bool newLine = true;

    public SpeechQueue(ITextToSpeech textToSpeech)
    {
        this.textToSpeech = textToSpeech;
    }

    public async Task SendToSpeechQueue(SpeechCommand speechCommand)
    {

        status_Manage?.EnableStatus(Status.Speech);
        lock (lockObject) // 加入鎖定以確保隊列更新安全
        {
            InputCommandHandler(speechCommand);
        }
        if (isSpeech == false)
        {
            await ExecuteQueue();
        }


    }
    public async Task ExecuteQueue()
    {
        lock (lockObject) // 確認在修改isSpeech標志前，沒有其他執行線程
        {
            isSpeech = true;
        }

        while (tempCommands.Count != 0)
        {

            await UniTask.WaitUntil(() => pause == false);
            status_Manage?.ExecuteStatusEvent(Status.Speech, tempCommands[0].content);
            status_Manage?.ExecuteStatusEvent(keyValuePairs[tempCommands[0].commandType], tempCommands[0].xml);

            // if (GameManager.dataStorage.globalSettingData.transition)
            //     status_Manage?.ExecuteStatusEvent(Status.Transition, tempCommands[0].transition);


            await textToSpeech.Speak(
                     tempCommands[0].content,
                     tempCommands[0].tTSData.rate,
                     tempCommands[0].tTSData.pitch,
                     tempCommands[0].tTSData.volume,
                     tempCommands[0].tTSData.language,
                     tempCommands[0].tTSData.voiceID
                 );

            if (!pause)
            {
                lock (lockObject) // 在移除命令前加鎖
                {
                    tempCommands.RemoveAt(0);
                }
            }


        }

        lock (lockObject) // 更新狀態之前加鎖
        {
            isSpeech = false;
            status_Manage?.DisableStatus(Status.Speech);
        }

    }
    public async void JumpToNext()
    {
        await textToSpeech.ForceStop();
    }
    public void Pause(bool active = true)
    {

        pause = active;
        if (active)
        {
            textToSpeech.ForceStop();
        }

    }
    public void ForceStopAndResetQueue()
    {
        textToSpeech.ForceStop();
        tempCommands.Clear();
        isSpeech = false;
        status_Manage?.DisableStatus(Status.Speech);

    }
    #region TextMethod
    public void InputCommandHandler(SpeechCommand speechCommand)
    {
        // 將句子依照包含、以及不包含<br>分組

        var text = speechCommand.content;
        var contents = text.GetAllTags();
        var transitions = speechCommand.transition.GetAllTags();


        string pattern = $"([{Regex.Escape(string.Concat(specialCharacters))}])";

        for (int i = 0; i < contents.Count; i++)
        {
            SpeechCommand command = new SpeechCommand();
            switch (contents[i].Tag)
            {
                // xml tag methods
                case "br":
                    command = new SpeechCommand(
                   speechCommand,
                    contents[i].Content,
                     "",
                     xml: contents[i].Content
                     );
                    tempCommands.Add(command);


                    if (i < transitions.Count && transitions[i].Content != null)
                    {
                        tempCommands.LastOrDefault().transition += transitions[i].Content;

                    };
                    break;
                case "img":
                    newLine = true;
                    command = new SpeechCommand(
                       speechCommand,
                        contents[i].Content,
                         "",
                         xml: contents[i].Attributes["src"],
                         commandType: CommandType.image
                         );
                    tempCommands.Add(command);

                    if (i < transitions.Count && transitions[i].Content != null)
                    {
                        tempCommands.LastOrDefault().transition += transitions[i].Content;
                    }
                    break;
                case "recommendPrompt":
                    status_Manage?.ExecuteStatusEvent(Status.RandomPrompt, contents[i].Content);
                    break;
                default:
                    string[] sentenceArray = Regex.Split(contents[i].Content, pattern);
                    List<string> xmlArray = new List<string>();
                    string[] transitionArray = null;

                    if (contents[i].XmlText != null)
                    {

                        if (!string.IsNullOrWhiteSpace(contents[i].Tag))
                        {
                            // 為每個句子添加標籤包裹
                            foreach (var sentence in sentenceArray)
                            {
                                xmlArray.Add($"<{contents[i].Tag}>{sentence}</{contents[i].Tag}>");
                            }
                        }
                        else
                        {
                            // 如果沒有標籤，則直接添加
                            xmlArray.AddRange(sentenceArray);
                        }

                    }

                    if (i < transitions.Count && transitions[i].Content != null)
                    {
                        transitionArray = Regex.Split(transitions[i].Content, pattern);
                    }

                    // 處理經過符號拆分後的句子長度
                    for (int s = 0; s < sentenceArray.Length; s++)
                    {

                        if (!string.IsNullOrEmpty(sentenceArray[s]))
                        {
                            //如果tempCommands != 0 加入前一組，否則新增commands

                            if (tempCommands.Count != 0 && !newLine)
                            {
                                bool IsSingleWord = sentenceArray[s].Length == 1;//如果SentenceArray 只有一個單字加入前一組，否則新增commands
                                bool IsOverLength = tempCommands.LastOrDefault().content.Length + sentenceArray[s].Length > sentenceLength;//如果前一組的長度+SentenceArray 長度小於預設長度(SentenceLength) 加入前一組，否則新增commands

                                if (IsSingleWord || !IsOverLength)
                                {
                                    var lastCommand = tempCommands.LastOrDefault();
                                    lastCommand.content += " " + sentenceArray[s];
                                    //lastCommand.xml += xmlArray[s] ?? null;
                                }
                                else
                                {
                                    command = new SpeechCommand(
                                        speechCommand,
                                        Content: sentenceArray[s],
                                        Transition: "",
                                        xml: ""
                                    );

                                    tempCommands.Add(command);

                                }
                            }
                            else
                            {
                                newLine = false;
                                command = new SpeechCommand(
                                    speechCommand,
                                    Content: sentenceArray[s],
                                    Transition: "",
                                    xml: ""
                                );

                                tempCommands.Add(command);

                            }

                            //將翻譯加入最後一個commands
                            if (transitionArray != null && s < transitionArray.Length)
                            {
                                tempCommands.LastOrDefault().transition += transitionArray[s];
                            }

                            //將Xml加入最後一個commands
                            if (xmlArray != null && s < xmlArray.Count)
                            {
                                tempCommands.LastOrDefault().xml += xmlArray[s];
                            }



                        }

                    }
                    break;

            }

        }


    }

    #endregion
}

