using System.Collections.Generic;
using UnityEngine;

namespace CharacterAI
{
    [CreateAssetMenu(menuName = "CharacterAI/CharacterList")]
    public class CharacterLoadingList : ScriptableObject
    {
        [Header("DeafultData")]
        public List<ModelData> modelDataList;
        [Header("UserData")]
        public List<CharacterData> characterDataList;

    }
}
