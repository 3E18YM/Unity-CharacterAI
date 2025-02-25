using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace CharacterAI
{
    [Serializable]
    public class CharacterData
    {
        public string name;
        public string id; 
        public int modelID;
        public string assistantID;
        public AssistantType assistantType;
        [FormerlySerializedAs("TtsData")] public TtsData TtsData;
        public CharacterBehaviorData behaviorData;


        public CharacterData(string name = null, string id = null, string uuid = null, int modelID = 0, string assistantID = null, AssistantType assistantType = AssistantType.Null, TtsData TtsData = null, CharacterBehaviorData behaviorData = null, bool isPublic = false)
        {
            this.name = name;
            this.id = id;
            this.modelID = modelID;
            this.assistantID = assistantID;
            this.assistantType = assistantType;
            this.TtsData = TtsData ?? new TtsData();
            this.behaviorData = behaviorData ?? new CharacterBehaviorData();
        }

        public CharacterData(CharacterData characterData)
        {
            this.name = characterData.name;
            this.id = characterData.id;
            this.modelID = characterData.modelID;
            this.assistantType = characterData.assistantType;
            this.TtsData = new TtsData(characterData.TtsData);
            this.behaviorData = characterData.behaviorData;
        }

        public CharacterData(ModelData modelData)
        {
            name = "";
            id = Guid.NewGuid().ToString();
            modelID = modelData.id;
            assistantID = null;
            TtsData = modelData.TtsData;
            behaviorData = modelData.motionBehaviorDatas;

        }
    }

    [System.Serializable]
    public class ModelData
    {
        public string name;
        public int id;
        public ModelType modelType;
        public Sprite sprite;
        public GameObject prefab;
        public string slogan;
        public TtsData TtsData;
        public CharacterBehaviorData motionBehaviorDatas;

        public ModelData(string name, int id, ModelType modelType, Sprite sprite, GameObject prefab, string slogan, CharacterBehaviorData motionBehaviorDatas = null)
        {
            this.name = name;
            this.id = id;
            this.modelType = modelType;
            this.sprite = sprite;
            this.prefab = prefab;
            this.slogan = slogan;
            this.motionBehaviorDatas = motionBehaviorDatas ?? new CharacterBehaviorData();
        }
    }


    [System.Serializable]
    public enum AssistantType
    {
        Null,
        //AzureOpenAI,
        //OpenAI,
        //Webduino,
    }

    public enum ModelType
    {
        Null,
        //Webduino,
        //Custom,
    }
}