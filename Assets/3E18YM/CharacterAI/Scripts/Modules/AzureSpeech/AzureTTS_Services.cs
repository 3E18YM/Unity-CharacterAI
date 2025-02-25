using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.PronunciationAssessment;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace CharacterAI.Modules.AzureSpeech
{   
    public class AzureTTS_Services : ITextToSpeech
    {
        [InjectOptional] private StatusManage StatusManage;
        [InjectOptional] public AudioSource audioSource = null;
        public AzureConfig azureConfig;
        private AzureSpeechConfig azureSetting=>azureConfig.azureSpeechConfig;
        private SpeechConfig config => azureSetting.GetSpeechConfig();
        private SpeechSynthesizer synthesizer;
        List<(string Name, double Offset)> bookmarks = new List<(string Name, double Offset)>();

        [Button]
        public async Task Speak(string text, double rate = 1, double pitch = 1, double volume = 1, string language = null, string voiceID = null)
        {
            if (String.IsNullOrWhiteSpace(text))
                return;
            language = language ?? azureSetting.TtsData.language;
            voiceID = voiceID ?? azureSetting.TtsData.voiceID;

            string ssml = text.ToSSML(rate, pitch, volume, language, voiceID);
            if (audioSource != null)
            {

                await StartFromAudioSource(audioSource, ssml);
            }
            else
            {

                await StartFromSynthesizer(ssml);
            }



        }

        public async Task StartFromSynthesizer(string ssml)
        {
            synthesizer = new SpeechSynthesizer(config);
            var result = await synthesizer.SpeakSsmlAsync(ssml);
            if (result.Reason == ResultReason.Canceled)
            {
                var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
                Debug.LogError($"CANCELED: Reason={cancellation.Reason}");

                if (cancellation.Reason == CancellationReason.Error)
                {
                    Debug.LogError($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                    Debug.LogError($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                }
            }
        }
        
        public async Task StartFromAudioSource(AudioSource audioSource, string ssml)
        {

            synthesizer = new SpeechSynthesizer(config, null);
            synthesizer.BookmarkReached += (s, e) =>
            {
                double offsetMilliseconds = e.AudioOffset / 10000.0;
                bookmarks.Add((e.Text, offsetMilliseconds));
                Debug.Log($"Bookmark reached: {e.Text}, Audio offset: {offsetMilliseconds}ms");
            };

            var result = await synthesizer.SpeakSsmlAsync(ssml);




            var audioDataStream = AudioDataStream.FromResult(result);
            var isFirstAudioChunk = true;
            var startTime = DateTime.Now;
            if (result.AudioData.Length == 0)
                return;

            var audioClip = AudioClip.Create(
                "Speech",
                result.AudioData.Length / 2, // Can speak 10mins audio as maximum
                1,
                16000,
                false,
                (float[] audioChunk) =>
                {
                    var chunkSize = audioChunk.Length;
                    var audioChunkBytes = new byte[chunkSize * 2];
                    var readBytes = audioDataStream.ReadData(audioChunkBytes);
                    if (isFirstAudioChunk && readBytes > 0)
                    {
                        var endTime = DateTime.Now;
                        var latency = endTime.Subtract(startTime).TotalMilliseconds;
                        isFirstAudioChunk = false;
                    }

                    for (int i = 0; i < chunkSize; ++i)
                    {
                        if (i < readBytes / 2)
                        {
                            audioChunk[i] = (short)(audioChunkBytes[i * 2 + 1] << 8 | audioChunkBytes[i * 2]) / 32768.0F;
                        }
                        else
                        {
                            audioChunk[i] = 0.0f;
                        }
                    }
                });

            audioSource.clip = audioClip;
            StatusManage.EnableStatus(Status.SpeechAudioSource);
            audioSource.Play();
            await AudioSourceBookMarkAlign(audioSource);
            await UniTask.WaitUntil(() => audioSource.isPlaying == false);
            StatusManage.DisableStatus(Status.SpeechAudioSource);
            audioSource.Stop();


        }

        private async Task AudioSourceBookMarkAlign(AudioSource audioSource)
        {
            int currentBookmarkIndex = 0;

            while (audioSource.isPlaying && currentBookmarkIndex < bookmarks.Count)
            {
                // 獲取當前播放時間（毫秒）
                double currentTime = audioSource.time * 1000.0;

                // 檢查是否到了下一個標記的時間
                if (currentTime >= bookmarks[currentBookmarkIndex].Offset)
                {
                    Debug.Log($"Triggered bookmark: {bookmarks[currentBookmarkIndex].Name} at {bookmarks[currentBookmarkIndex].Offset}ms");
                    StatusManage?.ExecuteStatusEvent(Status.SpeechBookMark, bookmarks[currentBookmarkIndex].Name);

                    currentBookmarkIndex++;
                }

                await UniTask.Yield(); // 使用 UniTask 讓出當前執行，等待下一幀
            }


        }
        
        public async Task ForceStop()
        {
            if (synthesizer != null)
                await synthesizer.StopSpeakingAsync();
            audioSource?.Stop();

        }

        public async Task<List<VoiceInfo>> VoiceIDList(string language)
        {

            synthesizer = new SpeechSynthesizer(config);
            var result = await synthesizer.GetVoicesAsync();
            if (result.Reason == ResultReason.VoicesListRetrieved)
            {
                List<VoiceInfo> voiceList = result.Voices.Where(x => x.Locale == language).ToList();
                return voiceList;

            }
            return null;

        }


    }
}



