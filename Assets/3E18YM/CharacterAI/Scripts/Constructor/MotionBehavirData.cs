using System;
using System.Collections.Generic;

namespace CharacterAI
{
    [Serializable]
    public class CharacterBehaviorData
    {
        public BehaviorData onStart;
        public BehaviorData onEnd;
        public BehaviorData speech;
        public List<BehaviorData> tag;
        public BehaviorData listen;
        public BehaviorData idle;
        public BehaviorData thinking;
        public BehaviorData wait;

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
        public string content;
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
}