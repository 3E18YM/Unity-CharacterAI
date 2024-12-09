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
        [FormerlySerializedAs("tTsData")] public TtsData tTsData;
        public CharacterBehaviorData behaviorData;


        public CharacterData(string name = null, string id = null, string uuid = null, int modelID = 0, string assistantID = null, AssistantType assistantType = AssistantType.Null, TtsData ttsData = null, CharacterBehaviorData behaviorData = null, bool isPublic = false)
        {
            this.name = name;
            this.id = id;
            this.modelID = modelID;
            this.assistantID = assistantID;
            this.assistantType = assistantType;
            this.tTsData = ttsData ?? new TtsData();
            this.behaviorData = behaviorData ?? new CharacterBehaviorData();
        }

        public CharacterData(CharacterData characterData)
        {
            this.name = characterData.name;
            this.id = characterData.id;
            this.modelID = characterData.modelID;
            this.assistantType = characterData.assistantType;
            this.tTsData = new TtsData(characterData.tTsData);
            this.behaviorData = characterData.behaviorData;
        }

        public CharacterData(ModelData modelData)
        {
            name = "";
            id = Guid.NewGuid().ToString();
            modelID = modelData.id;
            assistantID = null;
            tTsData = modelData.tTsData;
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
        public TtsData tTsData;
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