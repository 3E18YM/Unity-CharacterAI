using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Zenject;
using Sirenix.Serialization;

namespace CharacterAI
{
    [Serializable]
    public class ListenHandler
    {
        [InjectOptional] public ISpeechToText speechToText;
        [InjectOptional] public DiContainer DiContainer;
        [InjectOptional] public StatusManage StatusManage;
        public Microphone_Handler MicrophoneHandler = new Microphone_Handler();

        private bool pause = false;
        [FormerlySerializedAs("UnityMic")] public bool unityMic = false;
        public UnityEvent<bool> onPause;
        private CancellationTokenSource cts;
        private string message = null;
        private string language = "zh-TW";
        public ListenHandler(DiContainer diContainer = null)
        {
            if (diContainer == null) return;

            this.DiContainer = diContainer;
            StatusManage = this.DiContainer.Resolve<StatusManage>();
            diContainer.Inject(MicrophoneHandler);
        }
        public async Task<string> Listen(float autoStop = -1, bool first = true)
        {

            StatusManage?.EnableStatus(Status.Listen);
            if (first == true)
            {
                message = "";
                cts = new CancellationTokenSource();
            }
            string tempString = "";
            if (unityMic)
            {
                AudioClip audioClip = await MicrophoneHandler.StartRecording();
                tempString = await speechToText.Listen(audioClip);
                message += tempString;
                StatusManage?.ExecuteStatusEvent(Status.AzureSTT, message);

            }
            else
            {
                tempString = await speechToText.ListenStreaming(autoStop, language);
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
            speechToText.ForceStop();
            MicrophoneHandler.StopRecording();
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
            speechToText.ForceStop();
            MicrophoneHandler.StopRecording();

        }
        public void Restart()
        {
            if (pause == false)
            {
                pause = true;
                speechToText.ForceStop();
                MicrophoneHandler.StopRecording();
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
