using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace CharacterAI
{   [Serializable]
    public class SpeechQueue
    {
        [InjectOptional] private ITextToSpeech textToSpeech;
        [InjectOptional] private StatusManage StatusManage;

        #region SentenceSetting

        [Header("SentenceSetting")] [SerializeField]
        public int sentenceLength = 30;

        [SerializeField] public string chunkDelimiters;
        private HashSet<char> chunkDelimiterSet;

        #endregion

        public List<SpeechCommand> tempCommands;
        private Dictionary<CommandType, Status> keyValuePairs;

        bool isSpeech;
        private object lockObject;
        private bool pause;
        private bool newLine = true;
        private Status[] status;

        public SpeechQueue(ITextToSpeech textToSpeech)
        {
            this.textToSpeech = textToSpeech;
            Initial();
        }

        public void Initial()
        {
            lockObject = new object();
            keyValuePairs = new Dictionary<CommandType, Status>()
            {
                { CommandType.Speech, Status.RichText },
                { CommandType.Image, Status.ImageURL }
            };
            tempCommands = new List<SpeechCommand>();
            status = new Status[] { Status.Speech, Status.RichText, Status.ImageURL };

        }

        public async Task SendToSpeechQueue(SpeechCommand speechCommand)
        {

            InputCommandHandler(speechCommand);

            if (isSpeech == false)
            {
                await ExecuteQueue();
            }

        }

        public async Task ExecuteQueue()
        {

            isSpeech = true;
            StatusManage?.EnableStatus(status);


            while (tempCommands.Count != 0)
            {

                await UniTask.WaitUntil(() => pause == false);
                StatusManage?.ExecuteStatusEvent(Status.Speech, tempCommands[0].content);
                StatusManage?.ExecuteStatusEvent(keyValuePairs[tempCommands[0].commandType], tempCommands[0].xml);

                // if (GameManager.dataStorage.globalSettingData.transition)
                //     StatusManage?.ExecuteStatusEvent(Status.Transition, tempCommands[0].transition);


                await textToSpeech.Speak(
                    tempCommands[0].content,
                    tempCommands[0].TtsData.rate,
                    tempCommands[0].TtsData.pitch,
                    tempCommands[0].TtsData.volume,
                    tempCommands[0].TtsData.language,
                    tempCommands[0].TtsData.voiceID
                );

                if (!pause)
                {

                    tempCommands.RemoveAt(0);

                }

            }



            isSpeech = false;
            StatusManage?.DisableStatus(status);


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

        public async Task ForceStopAndResetQueue()
        {
            await textToSpeech.ForceStop();
            tempCommands.Clear();
            isSpeech = false;
            StatusManage?.DisableStatus(Status.Speech);
            StatusManage?.DisableStatus(Status.RichText);

        }

        private void OnValidate()
        {
            chunkDelimiterSet = GetSpecialCharactersSet();
        }

        #region TextMethod

        /// <summary>
        /// 處理語音指令的主要方法
        /// 將輸入的文本根據標籤進行解析和處理，並轉換成語音指令序列
        /// </summary>
        /// <param name="speechCommand">要處理的語音指令</param>
        public void InputCommandHandler(SpeechCommand speechCommand)
        {
            var text = speechCommand.content.ToRichText();
            var contents = text.GetAllTags();
            var transitions = speechCommand.transition.GetAllTags();
            chunkDelimiterSet = chunkDelimiterSet ?? GetSpecialCharactersSet();
            string pattern = $"([{Regex.Escape(string.Concat(chunkDelimiterSet))}])";

            for (int i = 0; i < contents.Count; i++)
            {
                var content = contents[i];
                var transition = i < transitions.Count ? transitions[i].Content : null;

                switch (content.Tag)
                {
                    case "br":
                        HandleBreakTag(speechCommand, content, transition);
                        break;
                    case "img":
                        HandleImageTag(speechCommand, content, transition);
                        break;
                    case "recommendPrompt":
                        StatusManage?.ExecuteStatusEvent(Status.RandomPrompt, content.Content);
                        break;
                    default:
                        HandleDefaultContent(speechCommand, content, transition, pattern);
                        break;
                }
            }
        }

        /// <summary>
        /// 處理換行標籤 <br>
        /// 創建一個新的語音指令並添加到指令隊列中
        /// </summary>
        private void HandleBreakTag(SpeechCommand speechCommand, XmlElementData content, string transition)
        {
            var command = new SpeechCommand(
                speechCommand,
                content: content.Content,
                transition: "",
                xml: content.Content
            );
            tempCommands.Add(command);

            if (transition != null)
            {
                tempCommands.LastOrDefault().transition += transition;
            }
        }

        /// <summary>
        /// 處理圖片標籤 <img>
        /// 創建一個新的圖片類型指令並添加到指令隊列中
        /// </summary>
        private void HandleImageTag(SpeechCommand speechCommand, XmlElementData content, string transition)
        {
            newLine = true;
            var command = new SpeechCommand(
                speechCommand,
                content: content.Content,
                transition: "",
                xml: content.Attributes["src"],
                commandType: CommandType.Image
            );
            tempCommands.Add(command);

            if (transition != null)
            {
                tempCommands.LastOrDefault().transition += transition;
            }
        }

        /// <summary>
        /// 處理一般文本內容
        /// 將文本按照標點符號分割並進行處理
        /// </summary>
        private void HandleDefaultContent(SpeechCommand speechCommand, XmlElementData content, string transition, string pattern)
        {
            string[] sentenceArray = Regex.Split(content.Content, pattern);
            var xmlArray = PrepareXmlArray(content, sentenceArray);
            string[] transitionArray = transition != null ? Regex.Split(transition, pattern) : null;

            ProcessSentences(speechCommand, sentenceArray, xmlArray, transitionArray);
        }

        /// <summary>
        /// 準備XML數組
        /// 為每個句子添加適當的XML標籤包裝
        /// </summary>
        private List<string> PrepareXmlArray(XmlElementData content, string[] sentenceArray)
        {
            if (content.XmlText == null) return null;

            var xmlArray = new List<string>();
            if (!string.IsNullOrWhiteSpace(content.Tag))
            {
                foreach (var sentence in sentenceArray)
                {
                    xmlArray.Add($"<{content.Tag}>{sentence}</{content.Tag}>");
                }
            }
            else
            {
                xmlArray.AddRange(sentenceArray);
            }
            return xmlArray;
        }

        /// <summary>
        /// 處理分割後的句子序列
        /// 遍歷所有句子並進行相應的處理
        /// </summary>
        private void ProcessSentences(SpeechCommand speechCommand, string[] sentenceArray, List<string> xmlArray, string[] transitionArray)
        {
            for (int s = 0; s < sentenceArray.Length; s++)
            {
                if (string.IsNullOrEmpty(sentenceArray[s])) continue;

                ProcessSingleSentence(speechCommand, sentenceArray[s]);
                AppendTransitionAndXml(s, transitionArray, xmlArray);
            }
        }

        /// <summary>
        /// 處理單個句子
        /// 根據句子長度和當前狀態決定是添加到現有指令還是創建新指令
        /// </summary>
        private void ProcessSingleSentence(SpeechCommand speechCommand, string sentence)
        {
            if (tempCommands.Count != 0 && !newLine)
            {
                bool isSingleWord = sentence.Length == 1;
                bool isOverLength = tempCommands.LastOrDefault().content.Length + sentence.Length > sentenceLength;

                if (isSingleWord || !isOverLength)
                {
                    tempCommands.LastOrDefault().content += " " + sentence;
                }
                else
                {
                    AddNewCommand(speechCommand, sentence);
                }
            }
            else
            {
                newLine = false;
                AddNewCommand(speechCommand, sentence);
            }
        }

        /// <summary>
        /// 添加新的語音指令到隊列
        /// 創建一個新的語音指令實例並添加到臨時指令列表中
        /// </summary>
        private void AddNewCommand(SpeechCommand speechCommand, string content)
        {
            var command = new SpeechCommand(
                speechCommand,
                content: content,
                transition: "",
                xml: ""
            );
            tempCommands.Add(command);
        }

        /// <summary>
        /// 為最後一個指令添加翻譯和XML內容
        /// 將對應的翻譯文本和XML標記添加到最近添加的指令中
        /// </summary>
        private void AppendTransitionAndXml(int index, string[] transitionArray, List<string> xmlArray)
        {
            var lastCommand = tempCommands.LastOrDefault();
            if (lastCommand == null) return;

            if (transitionArray != null && index < transitionArray.Length)
            {
                lastCommand.transition += transitionArray[index];
            }

            if (xmlArray != null && index < xmlArray.Count)
            {
                lastCommand.xml += xmlArray[index];
            }
        }

        /// <summary>
        /// 驗證並更新特殊字符集合
        /// </summary>
        private HashSet<char> GetSpecialCharactersSet()
        {
            if (string.IsNullOrEmpty(chunkDelimiters))
            {
                chunkDelimiters = "。.,，?？!！:：";
            }

            // 將字串轉換為字符集合，自動去除重複字符
            var set = new HashSet<char>(chunkDelimiters.ToCharArray());
            return set;

        }



        #endregion
    }
}

