
using System;
using System.Collections.Generic;
using UnityEngine;


    [Serializable]
    public class CharacterBehaviorData
    {
        public BehaviorData OnStart;
        public BehaviorData OnEnd;
        public BehaviorData Speech;
        public List<BehaviorData> Tag;
        public BehaviorData Listen;
        public BehaviorData Idle;
        public BehaviorData Thinking;
        public BehaviorData Wait;

    }
    [Serializable]
    public class BehaviorData
    {
        public string condition;
        public List<ActionData> actionList;

    }

    [Serializable]
    public class ActionData
    {
        public ActionType actionType;
        public string condition;
        public string Content;
        public int layer;
        public bool loop;
        public float duration;
        public int time;
        public bool wait;

    }
    [Serializable]
    public enum ActionType
    {
        RandomMotion,
        Animation,
        Emotion,
        AiResponse,
        TextResponse,
        MoveMotion,

    }

