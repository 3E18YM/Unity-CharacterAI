using System.Threading.Tasks;
using LLMUnity;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CharacterAI.Modules.UnityLLM
{
    public class AIServices_UnityLLM : IaiServices
    {
        public LLMCharacter llmCharacter;
        [Button]
        public override async Task<string> SendMessages(string message)
        {
            var reply = await llmCharacter.Chat(message);
            Debug.Log(reply);
            return reply;
        }
        public override void ForceStop()
        {
            llmCharacter.CancelRequests();
        }
    }
}