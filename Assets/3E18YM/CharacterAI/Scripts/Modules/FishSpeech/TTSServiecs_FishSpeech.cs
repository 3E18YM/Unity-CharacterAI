using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace CharacterAI.Modules.FishSpeech
{   
    [ShowOdinSerializedPropertiesInInspector]
    public class TTSServiecs_FishSpeech : ITextToSpeech
    {
        [InjectOptional] private StatusManage statusManage;
        [InjectOptional] private AudioSource audioSource;
        private APIHandler apiHandler = new APIHandler();
        [SerializeField] string localPort = "9880";
        [SerializeField] private FishSpeechVoiceResource VoiceResource;
        CancellationTokenSource cts;

        [Button]
        public async Task Speak(string text, double rate = 1, double pitch = 1, double volume = 1, string language = null, string voiceID = null)
        {
            cts = cts ?? new CancellationTokenSource();
            statusManage?.EnableStatus(Status.SpeechAudioSource);
            var voiceConfig = VoiceResource.GetConfig(voiceID);
            var referenceSource = voiceConfig.referenceSource;
            var refenceText = voiceConfig.referenceText;

            var apiUrl = $"http://localhost:{localPort}/?text={text}&ref_audio=./参考音频/{referenceSource}.wav&ref_text={refenceText}";
            Debug.Log(apiUrl);
            audioSource.clip = await apiHandler.GetAudioAsync(apiUrl, cts.Token);
            audioSource.Play();
            statusManage?.DisableStatus(Status.SpeechAudioSource);

        }
        [Button]
        public async Task ForceStop()
        {
            cts.Cancel();
            cts.Dispose();
            cts = null;
            audioSource?.Stop();
            statusManage?.DisableStatus(Status.SpeechAudioSource);
        }
    }
}