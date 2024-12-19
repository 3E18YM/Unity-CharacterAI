using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Diagnostics;
using System.Linq;
using Debug = UnityEngine.Debug;
using UnityEngine.UI;
using LLMUnity;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Sirenix.OdinInspector;

namespace LLMUnitySamples
{
    public class RAGSample : MonoBehaviour
    {   
        public  int optionCount =1;
        public RAG rag;
        public InputField playerText;
        public Text AIText;
        public TextAsset inputText;
        List<string> phrases;
        string ragPath = "RAGSample.zip";
        

        async void Start()
        {
            CheckLLMs(false);
            playerText.interactable = false;
            await rag.Load(ragPath);
            playerText.onSubmit.AddListener(onInputFieldSubmit);
            AIReplyComplete();
        }
        [Button]
        public void LoadPhrases()
        {
            //phrases = RAGUtils.ReadGutenbergFile(HamletText.text)["HAMLET"];
            phrases = GetParagraphs(inputText.text);
            //phrases.ForEach(x=>Debug.Log(x));
        }
        [Button]
        public async Task CreateEmbeddings()
        {
#if UNITY_EDITOR
            LoadPhrases();
            // build the embeddings
            playerText.text += $"Creating Embeddings (only once)...\n";
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            int pIndex = 0;
            foreach (string phrase in phrases)
            {   
                Debug.Log(System.Math.Round( (float)pIndex/(float)phrases.Count, 2));
                await rag.Add(phrase);
                pIndex += 1;
            }
            stopwatch.Stop();
            Debug.Log($"embedded {rag.Count()} phrases in {stopwatch.Elapsed.TotalMilliseconds / 1000f} secs");
            // store the embeddings
            rag.Save(ragPath);
#else
                // if in play mode throw an error
                throw new System.Exception("The embeddings could not be found!");
#endif
        }

        protected async virtual void onInputFieldSubmit(string message)
        {
            //playerText.interactable = false;
            if(string.IsNullOrEmpty(message)) return;
            AIText.text = "...";
            (string[] similarPhrases, float[] distances) = await rag.Search(message, optionCount);
            AIText.text = string.Join("\n", similarPhrases);
            
            
        }

        public void SetAIText(string text)
        {
            AIText.text = text;
        }

        public void AIReplyComplete()
        {
            playerText.interactable = true;
            playerText.Select();
            playerText.text = "";
        }

        public void ExitGame()
        {
            Debug.Log("Exit button clicked");
            Application.Quit();
        }

        protected void CheckLLM(LLMCaller llmCaller, bool debug)
        {
            if (!llmCaller.remote && llmCaller.llm != null && llmCaller.llm.model == "")
            {
                string error = $"Please select a llm model in the {llmCaller.llm.gameObject.name} GameObject!";
                if (debug) Debug.LogWarning(error);
                else throw new System.Exception(error);
            }
        }

        protected virtual void CheckLLMs(bool debug)
        {
            CheckLLM(rag.search.llmEmbedder, debug);
        }

        bool onValidateWarning = true;
        

        void OnValidate()
        {
            if (onValidateWarning)
            {
                CheckLLMs(true);
                onValidateWarning = false;
            }
        }
        public static List<string> GetParagraphs(string input)
        {
            return input
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
                .Select(line => line.Trim())
                .Aggregate(new List<string>(), (acc, line) =>
                {
                    if (string.IsNullOrEmpty(line))
                    {
                        // 空行產生新的段落容器
                        acc.Add(string.Empty);
                    }
                    else
                    {
                        // 將此行文字加入最後一個段落中
                        if (acc.Count == 0) acc.Add(string.Empty);
                        acc[acc.Count - 1] = 
                            (string.IsNullOrWhiteSpace(acc[acc.Count - 1]) 
                                ? line 
                                : acc[acc.Count - 1] + " " + line);
                    }
                    return acc;
                })
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToList();
        }
    }
}
