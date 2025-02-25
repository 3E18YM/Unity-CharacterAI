using UnityEngine;
using Zenject;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine.Events;
using System;

namespace CharacterAI
{
    [Serializable]

    public class ListenHandler
    {
        #region Settings

        [TabGroup("Settings")] public float SilenceTime = 2;
        [TabGroup("Settings")] public bool UnityMic = false;
        [TabGroup("Settings")] public UnityEvent<bool> onPause;

        #endregion

        #region Dependencies

        [Inject] DiContainer diContainer;
        [Inject] public ISpeechToText STT;
        [Inject] StatusManage StatusManage;
        public Microphone_Handler microphone_Handler = new Microphone_Handler();

        #endregion

        #region State

        private bool pause = false;
        private string message = null;
        private CancellationTokenSource cts;

        #endregion



        public void Initial()
        {
            diContainer.Inject(microphone_Handler);
        }

        public async Task<string> Listen(float? autoStop = null, bool first = true)
        {
            StatusManage?.EnableStatus(Status.Listen);
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
                StatusManage?.ExecuteStatusEvent(Status.AzureSTT, message);

            }
            else
            {
                tempString = await STT.ListenStreaming(autoStop ?? SilenceTime);
                message += tempString;
            }

            StatusManage.ExecuteStatusEvent(Status.Listen, message);

            Debug.Log(message);

            if (pause)
            {

                await UniTask.WaitUntil(() => pause == false);

                if (!cts.IsCancellationRequested)
                {
                    await Listen(autoStop, false);
                }

            }

            StatusManage?.DisableStatus(Status.Listen);
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
            StatusManage?.DisableStatus(Status.Listen);

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

}