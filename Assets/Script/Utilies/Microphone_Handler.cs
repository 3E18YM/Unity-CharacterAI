using UnityEngine;
using System.Threading.Tasks;
using Zenject;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;

public class Microphone_Handler
{
    // 宣告兩個 AudioClip 變數來儲存音效檔案


    #region ReferenceRegion 
    public AudioClip audioClip;
    [Inject] private Status_Manage status_Manage;

    #endregion

    #region private values
    private string microphone;
    private float silenceThreshold = 1f;
    private float silenceTime = 2.0f;
    private float volumeBoost;
    private int SAMPLE_BUFFER_LENGTH = 128;
    private int MinFreq = 0;
    private int MaxFreq = 16000;
    public bool isRecording;
    CancellationTokenSource cts = null;
    #endregion

    public Microphone_Handler(float mic_SilenceThreshold = 1.0f, float mic_SilenceTime = 2.0f, float mic_Boost = 1.0f, AudioClip audioClip = null)
    {

        this.audioClip = audioClip;
        silenceThreshold = mic_SilenceThreshold;
        silenceTime = mic_SilenceTime;
        volumeBoost = mic_Boost;


    }
    public async Task<AudioClip> StartRecording(bool autoSilence = true)
    {

        cts = new CancellationTokenSource();

        if (Microphone.devices.Length > 0)
        {
            microphone = Microphone.devices[0].ToString();
            Microphone.GetDeviceCaps(microphone, out MinFreq, out MaxFreq);
            Debug.Log("Selected Microphone: " + microphone + MinFreq + MaxFreq);
        }
        else
        {
            Debug.LogError("No microphone found!");
        }

        float silenceTimer = 0;
        if (microphone != null)
        {
            isRecording = true;
            audioClip = Microphone.Start(microphone, true, 60, 16000);
            status_Manage.EnableStatus(Status.Recording);
            Debug.Log("Recording Started..." + microphone);

        }
        while (silenceTimer <= silenceTime && !cts.Token.IsCancellationRequested)
        {

            await UniTask.WaitForSeconds(0.1f);
            if (autoSilence)
            {

                if (LevelMax() < silenceThreshold)
                {
                    silenceTimer += 0.1f;

                }
                else
                {
                    silenceTimer = 0;
                }

            }

        }

        isRecording = false;
        StopRecording();
        return audioClip;

    }

    [Button]
    public void StopRecording()
    {

        if (isRecording)
        {
            Microphone.End(microphone);
            isRecording = false;
            cts.Cancel();
            status_Manage?.DisableStatus(Status.Recording);
            Debug.Log("Recording Stopped.");


        }

    }

    public float LevelMax()
    {
        float levelMax = 0;
        float[] waveData = new float[SAMPLE_BUFFER_LENGTH];
        int micPosition = Microphone.GetPosition(null) - (SAMPLE_BUFFER_LENGTH + 1); // null means the first microphone
        if (micPosition >= 0)
        {
            audioClip.GetData(waveData, micPosition);
            // Getting a peak on the last 128 samples
            for (int i = 0; i < SAMPLE_BUFFER_LENGTH; i++)
            {
                float wavePeak = waveData[i];
                if (levelMax < wavePeak)
                {
                    levelMax = wavePeak;
                }
            }
            levelMax = Mathf.Clamp(levelMax * (10 + volumeBoost), 0, 1);
            return levelMax;
        }
        return 0;

    }
    AudioClip TrimSilence(AudioClip clip, float threshold)
    {
        var samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        int startSample = 0;
        int endSample = samples.Length;

        // 尋找開始的非靜音位置
        for (int i = 0; i < samples.Length; i++)
        {
            if (Mathf.Abs(samples[i]) > threshold)
            {
                startSample = i;
                break;
            }
        }

        // 尋找結束的非靜音位置
        for (int i = samples.Length - 1; i >= 0; i--)
        {
            if (Mathf.Abs(samples[i]) > threshold)
            {
                endSample = i;
                break;
            }
        }

        int length = endSample - startSample;
        var trimmedSamples = new float[length];
        System.Array.Copy(samples, startSample, trimmedSamples, 0, length);

        // 創建一個新的 AudioClip
        AudioClip trimmedClip = AudioClip.Create("TrimmedClip", length, clip.channels, clip.frequency, false);
        trimmedClip.SetData(trimmedSamples, 0);

        return trimmedClip;
    }




}
