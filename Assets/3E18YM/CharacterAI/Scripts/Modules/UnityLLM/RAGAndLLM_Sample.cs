using System.Threading.Tasks;
using UnityEngine.UI;
using LLMUnity;
using UnityEngine;

namespace CharacterAI.Modules.UnityLLM
{
    public class AIServices_UnityLLM : IAIServices
    {
        public LLMCharacter llmCharacter;
        public AIServices_RAG rag;
        [TextArea(20,20)]public string Template;
       
        public override async Task<string> SendMessages(string message)
        {
            var information = await rag.Search(message);
            string query = string.Format(Template,message,information);
            Debug.Log(query);
            var result = await llmCharacter.Chat(query);
            return result;
        }

        public override void ForceStop()
        {   
            llmCharacter.CancelRequests();
        }
    }
}
