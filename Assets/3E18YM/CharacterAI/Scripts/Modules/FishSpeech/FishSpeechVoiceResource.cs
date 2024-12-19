using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace CharacterAI.Modules.FishSpeech
{
    [CreateAssetMenu(fileName = "FishSpeechVoiceConfig", menuName = "CharacterAI/FishSpeechVoiceConfig")]
    public class FishSpeechVoiceResource : ScriptableObject
    {
        public List<FishSpeechVoiceConfig> voiceResource;

        public FishSpeechVoiceConfig GetConfig(string id)
        {

            return voiceResource.FirstOrDefault(x => x.id == id);

        }
    }
}