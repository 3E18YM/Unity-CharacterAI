using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CharacterAI;
using UnityEngine;
using UnityEngine.Windows.Speech;

namespace CharacterAI.Modules.Windows
{
    public class TTSservices_WindowSpeech : ISpeechToText
    {
        private DictationRecognizer dictationRecognizer;
        private TaskCompletionSource<string> tcs;
        private float stopTimer;
        private bool isListening;
        private CancellationTokenSource cts;
        public string currentDetectLanguage { get; set; }

        public async Task<string> Listen(AudioClip clip = null, string defaultLanguage = null)
        {
            Debug.LogWarning("Windows Speech API does not support recognition from AudioClip");
            return string.Empty;
        }

        public async Task<string> ListenStreaming(float autoStop = -1, string defaultLanguage = null)
        {
            try
            {
                if (isListening)
                {
                    Debug.LogWarning("Already listening");
                    return string.Empty;
                }

                cts?.Cancel();
                cts = new CancellationTokenSource();

                tcs = new TaskCompletionSource<string>();
                stopTimer = autoStop;
                isListening = true;

                if (dictationRecognizer == null)
                {
                    dictationRecognizer = new DictationRecognizer();
                    dictationRecognizer.DictationResult += OnDictationResult;
                    dictationRecognizer.DictationError += OnDictationError;
                    dictationRecognizer.DictationHypothesis += OnDictationHypothesis;
                }

                dictationRecognizer.Start();

                // if (autoStop > 0)
                // {
                //     _ = Task.Delay((int)(autoStop * 1000), cts.Token)
                //         .ContinueWith(_ => ForceStop(),
                //             TaskContinuationOptions.OnlyOnRanToCompletion);
                // }

                string result = await WaitForTaskWithCancellation(tcs.Task, cts.Token);

                return result;
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Listening cancelled");
                return string.Empty;
            }
            finally
            {
                if (isListening)
                {
                    ForceStop();
                }
            }
        }

        public void ForceStop()
        {
            try
            {
                if (dictationRecognizer != null && isListening)
                {
                    dictationRecognizer.Stop();
                    isListening = false;

                    if (!tcs.Task.IsCompleted)
                    {
                        tcs.TrySetResult(string.Empty);
                    }
                }

                cts?.Cancel();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error in ForceStop: {ex.Message}");
            }
        }

        private void OnDictationResult(string text, ConfidenceLevel confidence)
        {
            try
            {
                isListening = false;
                dictationRecognizer.Stop();
                Debug.Log(text);
                tcs?.TrySetResult(text);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error in OnDictationResult: {ex.Message}");
                tcs?.TrySetResult(string.Empty);
            }
        }
        private void OnDictationHypothesis(string text)
        {
            Debug.Log("Dictation hypothesis: " + text);

        }

        private void OnDictationError(string error, int hresult)
        {
            try
            {
                isListening = false;
                dictationRecognizer.Stop();
                Debug.LogError($"Dictation error: {error}");
                tcs?.TrySetResult(string.Empty);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error in OnDictationError: {ex.Message}");
            }
        }

        private void OnDestroy()
        {
            try
            {
                cts?.Cancel();
                cts?.Dispose();
                cts = null;

                if (dictationRecognizer != null)
                {
                    dictationRecognizer.DictationResult -= OnDictationResult;
                    dictationRecognizer.DictationError -= OnDictationError;
                    dictationRecognizer.Dispose();
                    dictationRecognizer = null;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error in OnDestroy: {ex.Message}");
            }
        }

        async Task<string> WaitForTaskWithCancellation(Task<string> task, CancellationToken cancellationToken)
        {
            var cancellationTask = Task.Delay(Timeout.Infinite, cancellationToken);
            var completedTask = await Task.WhenAny(task, cancellationTask).ConfigureAwait(false);

            if (completedTask == cancellationTask)
            {
                throw new OperationCanceledException(cancellationToken);
            }

            return await task.ConfigureAwait(false);
        }
    }
}
public static class StringExtensions{

    
     #region AzureSSML
    /// <summary>
    /// 轉換成Azure 語音服務SSML 語言
    /// </summary>
    /// <param name="text">語音內容</param>
    /// <param name="rate">語音速度,0.1~2.0</param>
    /// <param name="pitch">語音音調,0.1~2.0</param>
    /// <param name="volume">語音音量,0.1~2.0</param>
    /// <param name="language">語言,預設en-US</param>
    /// <param name="voiceID">語音ID,預設AnaNeural</param>
    /// <returns></returns>
    public static string ToSSML(this string text, double rate = 1, double pitch = 1, double volume = 1, string language = null, string voiceID = null)
    {


        language = language ?? "en-US";
        voiceID = voiceID ?? "AnaNeural";

        string rateStr = rate.DoubleToStringPercent();
        string pitchStr = pitch.DoubleToStringPercent();
        string volumeStr = volume.DoubleToStringPercent();
        // 将特殊字符转义
        text = EscapeSpecialCharacters(text);

        var ssml = $"<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='{language}'>" +
               $"<voice name='{language}-{voiceID}'>" +
               $"<prosody rate='{rateStr}' pitch='{pitchStr}' volume='{volumeStr}'>" +
               $"{text}" +
               "</prosody></voice></speak>";
        //Debug.Log(ssml);
        return ssml;

    }
    private static readonly Dictionary<string, string> replacements = new Dictionary<string, string>
{
    { "&", "&amp;" },
    { "<", "&lt;" },
    { ">", "&gt;" },
    { "\"", "&quot;" },
    { "'", "&apos;" }
};

    private static string EscapeSpecialCharacters(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        // 使用預定義的字典進行替換
        foreach (var pair in replacements)
        {
            text = text.Replace(pair.Key, pair.Value);
        }

        // 使用正則表達式移除所有emoji
        text = Regex.Replace(text, @"\p{Cs}", "");

        return text;
    }

    public static string ToSSML(this string text, TtsData data)
    {
        var ssml = ToSSML(

            text,
            data.rate,
            data.pitch,
            data.volume,
            data.language,
            data.voiceID

        );

        return ssml;

    }

    public static string ToSSML(this SpeechCommand speechCommand)
    {

        var ssml = ToSSML(
            speechCommand.content,
            speechCommand.TtsData
        );

        return ssml;

    }
    #endregion
}