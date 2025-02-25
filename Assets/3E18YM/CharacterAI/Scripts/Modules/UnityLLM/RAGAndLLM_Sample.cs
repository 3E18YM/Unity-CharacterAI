using System.Threading.Tasks;
using UnityEngine.UI;
using LLMUnity;
using UnityEngine;
using System;

namespace CharacterAI.Modules.UnityLLM
{
    public class AIServices_UnityLLM : IAIServices
    {
        public LLMCharacter llmCharacter;
        public AIServices_RAG rag;
        [TextArea(20, 20)] public string Template;

        public async Task<string> SendMessages(string message)
        {
            var information = await rag.Search(message);
            string query = string.Format(Template, message, information);
            Debug.Log(query);
            var result = await llmCharacter.Chat(query);
            return result;
        }

        public void ForceStop()
        {
            llmCharacter.CancelRequests();
        }

        public void SetAssistant(string assistantID)
        {
            // 實作內容
        }

        public event Action<string> OnMessage;
    }
}
