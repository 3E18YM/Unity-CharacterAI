using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.PronunciationAssessment;
using UnityEngine;
using Zenject;
using Cysharp.Threading.Tasks;

namespace CharacterAI.Modules.AzureSpeech
{
    public class AzureSTT_Services : ISpeechToText
    {

        #region ReferencesRegion
        [InjectOptional] private StatusManage StatusManage;
        public AzureConfig azureConfig;
        private AzureSpeechConfig azureSetting=>azureConfig.azureSpeechConfig;
        private SpeechConfig config => azureSetting.GetSpeechConfig();
        private SpeechRecognizer recognizer;
        private CancellationTokenSource cts;

        #endregion

        #region Output
        public string messaged = "";
        public string messaging = "";
        private bool isSpeaking;
        public string currentDetectLanguage
        {
            get { return _currentDetectLanguage; }
            set { _currentDetectLanguage = value; }
        }
        private string _currentDetectLanguage = "zh-TW";

        #endregion

       
        #region initial
        public void ForceStop()
        {
            StatusManage?.DisableStatus(Status.AzureSTT);
            StatusManage?.DisableStatus(Status.Recording);
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose(); // 释放资源
                cts = null; // 重置cts，以便重新启动
            }

        }
        #endregion
        #region STT

        public async Task<string> ListenStreaming(float autoStop = -1, string defaultLanguage = null)
        {

            StatusManage?.EnableStatus(Status.Recording);
            if (cts != null)
            {
                return null;
            }

            cts = new CancellationTokenSource();
            var taskCompletionSource = new TaskCompletionSource<string>();

            if (recognizer == null)
            {
                recognizer = new SpeechRecognizer(config, defaultLanguage ?? azureSetting.sTTLanguage);

            }

            messaged = null;
            recognizer.Recognizing += (s, e) =>
            {
                messaging = messaged + e.Result.Text;
                _ = Task.Run(() => StatusManage?.ExecuteStatusEvent(Status.AzureSTT, messaging));
#if UNITY_EDITOR
                Debug.Log(messaging);
#endif
            };
            recognizer.Recognized += (s, e) =>
            {
                messaged += e.Result.Text;
                messaging = messaged;
                StatusManage?.ExecuteStatusEvent(Status.AzureSTT, messaging);
                AutoStop(autoStop);
            };

            recognizer.Canceled += (s, e) =>
            {

                Debug.Log($"CANCELED: Reason={e.Reason}");
                if (e.Reason == CancellationReason.Error)
                {
                    Debug.Log($"ErrorCode={e.ErrorCode}");
                    Debug.Log($"ErrorDetails={e.ErrorDetails}");
                }


            };
            cts.Token.Register(async () =>
            {
                await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
                recognizer = null;
                taskCompletionSource.TrySetResult(messaged);

            });

            await recognizer.StartContinuousRecognitionAsync();


            return await taskCompletionSource.Task;


        }
        public async void AutoStop(float autoStop)
        {
            string result = new string(messaging);
            await UniTask.WaitForSeconds(autoStop);
            if (result == messaging)
            {
                ForceStop();
            }
        }
        public async Task<string> Listen(AudioClip clip = null, string defaultLanguage = null)
        {
            // 获取 AudioClip 的样本数据
            int sampleCount = clip.samples * clip.channels;
            float[] samples = new float[sampleCount];
            clip.GetData(samples, 0);

            // 将浮点样本数据转换为 16 位 PCM 格式的字节数组
            byte[] pcmData = new byte[sampleCount * 2]; // 每个样本2个字节
            int rescaleFactor = 32767; // 将浮点数 [-1.0,1.0] 转换为 Int16

            for (int i = 0; i < samples.Length; i++)
            {
                short intData = (short)(samples[i] * rescaleFactor);
                byte[] byteArr = BitConverter.GetBytes(intData);
                pcmData[i * 2] = byteArr[0];
                pcmData[i * 2 + 1] = byteArr[1];
            }

            var autoDetectSourceLanguageConfig = AutoDetectSourceLanguageConfig.FromLanguages(new string[] { "en-US", "zh-TW", "ja-JP" });
            // 指定音频流格式
            var audioFormat = AudioStreamFormat.GetWaveFormatPCM((uint)clip.frequency, 16, (byte)clip.channels);
            using var audioInput = AudioInputStream.CreatePushStream(audioFormat);
            using var audioConfig = AudioConfig.FromStreamInput(audioInput);
            using var recognizer = new SpeechRecognizer(config, autoDetectSourceLanguageConfig, audioConfig);

            // 将 PCM 数据写入音频输入流
            audioInput.Write(pcmData);
            audioInput.Close();

            // 执行语音识别
            var result = await recognizer.RecognizeOnceAsync();
            if (result.Reason == ResultReason.RecognizedSpeech)
            {
                // 取得辨識的語言資訊
                var detectedLanguage = AutoDetectSourceLanguageResult.FromResult(result);
                _currentDetectLanguage = detectedLanguage.Language;

            }
            return result.Text;

        }
        #endregion


    }
}


