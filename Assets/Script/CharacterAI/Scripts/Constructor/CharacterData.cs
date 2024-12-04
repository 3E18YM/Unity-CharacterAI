using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;


    [Serializable]
    public class CharacterData
    {
        public string name;
        public string id; 
        public int modelID;
        public string assistantID;
        public AssistantType assistantType;
        [FormerlySerializedAs("tTsData")] public TTSData tTSData;
        public CharacterBehaviorData behaviorData;


        public CharacterData(string name = null, string id = null, string uuid = null, int modelID = 0, string assistantID = null, AssistantType assistantType = AssistantType.Null, TTSData ttsData = null, CharacterBehaviorData behaviorData = null, bool isPublic = false)
        {
            this.name = name;
            this.id = id;
            this.modelID = modelID;
            this.assistantID = assistantID;
            this.assistantType = assistantType;
            this.tTSData = ttsData ?? new TTSData();
            this.behaviorData = behaviorData ?? new CharacterBehaviorData();
        }

        public CharacterData(CharacterData characterData)
        {
            this.name = characterData.name;
            this.id = characterData.id;
            this.modelID = characterData.modelID;
            this.assistantType = characterData.assistantType;
            this.tTSData = new TTSData(characterData.tTSData);
            this.behaviorData = characterData.behaviorData;
        }

        public CharacterData(ModelData modelData)
        {
            name = "";
            id = Guid.NewGuid().ToString();
            modelID = modelData.ID;
            assistantID = null;
            tTSData = modelData.tTSData;
            behaviorData = modelData.motionBehaviorDatas;

        }
    }

    [System.Serializable]
    public class ModelData
    {
        public string name;
        public int ID;
        public ModelType modelType;
        public Sprite sprite;
        public GameObject prefab;
        public string slogan;
        public TTSData tTSData;
        public CharacterBehaviorData motionBehaviorDatas;

        public ModelData(string Name, int ID, ModelType modelType, Sprite Sprite, GameObject Prefab, string slogan, CharacterBehaviorData motionBehaviorDatas = null)
        {
            this.name = Name;
            this.ID = ID;
            this.modelType = modelType;
            this.sprite = Sprite;
            this.prefab = Prefab;
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
