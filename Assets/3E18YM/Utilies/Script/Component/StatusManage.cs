using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine.Events;

public class StatusManage
{
    private readonly HashSet<Status> activeStatuses = new HashSet<Status>();
    private readonly Dictionary<Status, StatusEntry> statusEntries = new Dictionary<Status, StatusEntry>();

    public StatusManage()
    {
        foreach (Status status in Enum.GetValues(typeof(Status)))
        {
            statusEntries[status] = new StatusEntry();
        }
    }

    #region SenderMethods

    public void EnableStatus(Status status, object value = null)
    {
        if (activeStatuses.Add(status))
        {
            statusEntries[status].OnStartMessage = value;
            statusEntries[status].OnStartEvent.Invoke(value);
        };

    }
    public void EnableStatus(Status[] status, object value = null)
    {
        foreach (var s in status)
        {
            EnableStatus(s, value);
        }
    }

    public void DisableStatus(Status[] status, object value = null)
    {
        foreach (var s in status)
        {
            DisableStatus(s, value);
        }
    }

    public void ExecuteStatusEvent(Status status, object value = null)
    {
        statusEntries[status].Message = value;
        statusEntries[status].StatusEvent.Invoke(value);
    }

    public void DisableStatus(Status status, object value = null)
    {
        if (activeStatuses.Remove(status))
        {
            statusEntries[status].OnEndMessage = value;
            statusEntries[status].OnEndEvent.Invoke(value);
        };

    }

    public void DisableAll()
    {
        foreach (var status in new List<Status>(activeStatuses))
        {
            DisableStatus(status);
        }
    }

    #endregion

    #region ReceiverMethods

    public IReadOnlyCollection<Status> GetActiveStatuses()
    {
        return activeStatuses;
    }

    public bool CheckStatus(Status status)
    {
        return activeStatuses.Contains(status);
    }

    public void AddOnStartListener<T>(Status status, UnityAction<T> listener)
    {
        statusEntries[status].OnStartEvent.AddListener(value =>
        {
            if (value is T typeValue)
            {
                listener.Invoke(typeValue);
            }
            else
            {
                listener.Invoke(default);
            }

        });
    }

    public void AddOnEndListener<T>(Status status, UnityAction<T> listener)
    {
        statusEntries[status].OnEndEvent.AddListener(value =>
        {
            if (value is T typeValue)
            {
                listener.Invoke(typeValue);
            }
            else
            {
                listener.Invoke(default);
            }
        });
    }

    public void AddStatusListener<T>(Status status, UnityAction<T> listener)
    {
        statusEntries[status].StatusEvent.AddListener(value =>
        {
            if (value is T typeValue)
            {
                listener.Invoke(typeValue);
            }
            else
            {
                listener.Invoke(default);
            }
        });
    }

    #endregion

    // 泛型获取消息的方法，进行类型安全的转换
    public T GetLastMessage<T>(Status status)
    {
        object message = statusEntries[status].Message;

        if (message is T typedMessage)
        {
            return typedMessage;
        }
        else
        {
            throw new InvalidCastException($"无法将消息转换为类型 {typeof(T)}。");
        }
    }

    public T GetLastOnStartMessage<T>(Status status)
    {
        object message = statusEntries[status].OnStartMessage;
        if (message is T typedMessage)
        {
            return typedMessage;
        }
        else
        {
            throw new InvalidCastException($"无法将启动消息转换为类型 {typeof(T)}。");
        }
    }

    public T GetLastOnEndMessage<T>(Status status)
    {
        object message = statusEntries[status].OnEndMessage;
        if (message is T typedMessage)
        {
            return typedMessage;
        }
        else
        {
            throw new InvalidCastException($"无法将结束消息转换为类型 {typeof(T)}。");
        }
    }

    private class StatusEntry
    {
        public UnityEvent<object> OnStartEvent { get; } = new UnityEvent<object>();
        public UnityEvent<object> OnEndEvent { get; } = new UnityEvent<object>();
        public UnityEvent<object> StatusEvent { get; } = new UnityEvent<object>();

        public object Message { get; set; }
        public object OnStartMessage { get; set; }
        public object OnEndMessage { get; set; }
    }
}
public enum Status
{
    Error,
    OnStart,
    OnEnd,
    Idle,
    Wait,
    Typing,
    Speech,
    SpeechAudioSource,
    SpeechBookMark,
    Transition,
    Recording,
    SendToAI,
    GenerateText,
    PronunciationAnalysis,
    AzureSTT,
    Listen,
    RichText,
    ImageURL,
    AIReply,
    RandomPrompt,

}

