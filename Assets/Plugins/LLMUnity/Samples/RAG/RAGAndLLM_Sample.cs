using UnityEngine.UI;
using LLMUnity;
using UnityEngine;

namespace LLMUnitySamples
{
    public class RAGAndLLMSample : RAGSample
    {
        public LLMCharacter llmCharacter;
        public Toggle ParaphraseWithLLM;
        [TextArea(20,20)]public string Template;
        protected async override void onInputFieldSubmit(string message)
        {
            playerText.interactable = false;
            AIText.text = "...";
            (string[] similarPhrases, float[] distances) = await rag.Search(message, optionCount);
            string information = string.Join("\n", similarPhrases);
            
            if (!ParaphraseWithLLM.isOn)
            {
                AIText.text = information;
                AIReplyComplete();
            }
            else
            {
                string query = string.Format(Template,playerText.text,information);
                Debug.Log(query);
                _ = llmCharacter.Chat(query, SetAIText, AIReplyComplete);
            }
        }

        public void CancelRequests()
        {
            llmCharacter.CancelRequests();
            AIReplyComplete();
        }

        protected override void CheckLLMs(bool debug)
        {
            base.CheckLLMs(debug);
            CheckLLM(llmCharacter, debug);
        }
    }
}
