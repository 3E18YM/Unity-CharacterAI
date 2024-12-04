using UnityEngine;
using Zenject;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine.Events;
using System;
[Serializable]
public class ListenHandler
{
    [Inject] DiContainer diContainer;
    [Inject] public ISpeechToText STT;
    [Inject] Status_Manage status_Manage;
    public Microphone_Handler microphone_Handler = new Microphone_Handler();
    private bool pause = false;
    public bool UnityMic = false;
    public UnityEvent<bool> onPause;
    private CancellationTokenSource cts;
    private string message = null;
    private string language = "zh-TW";

    public void initial()
    {
        diContainer.Inject(microphone_Handler);
    }
    public async Task<string> Listen(float autoStop = -1, bool first = true)
    {

        status_Manage?.EnableStatus(Status.Listen);
        if (first == true)
        {
            message = "";
            cts = new CancellationTokenSource();
        }


        string tempString = "";
        if (UnityMic)
        {
            AudioClip audioClip = await microphone_Handler.StartRecording();
            tempString = await STT.Listen(audioClip);
            message += tempString;
            status_Manage?.ExecuteStatusEvent(Status.AzureSTT, message);

        }
        else
        {
            tempString = await STT.ListenStreaming(autoStop, language);
            message += tempString;
        }

        status_Manage.ExecuteStatusEvent(Status.Listen, message);

        Debug.Log(message);

        if (pause)
        {

            await UniTask.WaitUntil(() => pause == false);

            if (!cts.IsCancellationRequested)
            {
                await Listen(autoStop, false);
            }

        }

        status_Manage?.DisableStatus(Status.Listen);
        return message;

    }

    public void Pause()
    {
        pause = true;
        STT.ForceStop();
        microphone_Handler.StopRecording();
        onPause?.Invoke(true);


    }
    public void UnPause()
    {
        pause = false;
        onPause?.Invoke(false);

    }
    public void ForceStop()
    {
        if (pause == true)
        {
            pause = false;
            cts.Cancel();
        }
        STT.ForceStop();
        microphone_Handler.StopRecording();

    }
    public void Restart()
    {
        if (pause == false)
        {
            pause = true;
            STT.ForceStop();
            microphone_Handler.StopRecording();
            message = null;
            pause = false;
        }
        else
        {
            message = null;
        }

    }

}
