using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class APIHandler
{
    public string accessToken;
    public string refreshToken;
    public event Action<UnityWebRequest> OnSuccess;
    public event Action<UnityWebRequest> OnFailed;
    private CancellationTokenSource cts = new CancellationTokenSource();

    public List<string> ParseListString(string jsonString)
    {
        var jsonResponse = JObject.Parse(jsonString);
        var result = jsonResponse["data"]?.ToObject<List<string>>();
        return result;
    }
    public void ForceStop()
    {
        cts.Cancel();
        cts.Dispose();
        cts = null;
    }

    public async Task<string> GetAsync(string apiUrl, CancellationToken? cancellationToken = null)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
        {
            // 設置請求的內容類型
            SetRequest(request);

            // 發送請求並等待回應
            try
            {
                await request.SendWebRequest().WithCancellation(cancellationToken ?? cts.Token); // 使用自己的 token
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            return requestHandler(request);
        }
    }

    public async Task<AudioClip> GetAudioAsync(string apiUrl, CancellationToken? cancellationToken = null)
    {
        using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(apiUrl, AudioType.WAV))
        {
            // 設置請求的內容類型
            SetRequest(request);

            // 發送請求並等待回應
            try
            {
                await request.SendWebRequest().WithCancellation(cancellationToken ?? cts.Token); // 使用自己的 token
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            // 處理請求結果
            if (request.result == UnityWebRequest.Result.Success)
            {
                var clip = DownloadHandlerAudioClip.GetContent(request);
                return clip;
            }
            else
            {
                requestHandler(request); // 處理錯誤
                return null;
            }
        }
    }

    public async Task<string> PostAsync(string apiUrl, string json = null, CancellationToken? cancellationToken = null)
    {
        using (UnityWebRequest request = new UnityWebRequest(apiUrl, UnityWebRequest.kHttpVerbPOST))
        {
            // 設置請求的內容類型
            byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes(json ?? string.Empty);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            SetRequest(request);
            // 發送請求並等待回應
            try
            {
                await request.SendWebRequest().WithCancellation(cancellationToken ?? cts.Token); // 使用自己的 token
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            return requestHandler(request);
        }
    }

    public async Task<AudioSource> PostAudioAsync(string apiUrl, string json, AudioSource audioSource, CancellationToken? cancellationToken = null)
    {
        using (UnityWebRequest request = new UnityWebRequest(apiUrl, UnityWebRequest.kHttpVerbPOST))
        {
            // 設置請求的內容類型
            byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes(json ?? string.Empty);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            SetRequest(request);

            // 發送請求並等待回應
            try
            {
                await request.SendWebRequest().WithCancellation(cancellationToken ?? cts.Token); // 使用自己的 token
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            // 處理請求結果
            if (request.result == UnityWebRequest.Result.Success)
            {
                var clip = DownloadHandlerAudioClip.GetContent(request);
                audioSource.clip = clip;
                audioSource.Play(); // 播放音頻
                return audioSource;
            }
            else
            {
                requestHandler(request); // 處理錯誤
                return null;
            }
        }
    }

    public async Task<string> PutAsync(string apiUrl, string json = null, CancellationToken? cancellationToken = null)
    {
        using (UnityWebRequest request = new UnityWebRequest(apiUrl, UnityWebRequest.kHttpVerbPUT))
        {
            // 設置請求的內容類型
            byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes(json ?? string.Empty);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            SetRequest(request);
            // 發送請求並等待回應
            try
            {
                await request.SendWebRequest().WithCancellation(cancellationToken ?? cts.Token); // 使用自己的 token
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            return requestHandler(request);
        }
    }

    public async Task<string> DeleteAsync(string apiUrl, CancellationToken? cancellationToken = null)
    {
        using (UnityWebRequest request = new UnityWebRequest(apiUrl, UnityWebRequest.kHttpVerbDELETE))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            SetRequest(request);

            // 發送請求並等待回應
            try
            {
                await request.SendWebRequest().WithCancellation(cancellationToken ?? cts.Token); // 使用自己的 token
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            return requestHandler(request);
        }
    }
    public void SetRequest(UnityWebRequest request)
    {
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Accept", "application/json, text/plain, */*");
        if (string.IsNullOrWhiteSpace(accessToken) && string.IsNullOrWhiteSpace(refreshToken))
        {
            request.SetRequestHeader("cookie", $"oauth_access_token={accessToken};oauth_refresh_token={refreshToken}");
        }
    }

    public string requestHandler(UnityWebRequest request)
    {
        // 處理回應
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            OnFailed?.Invoke(request);
            // 401 錯誤處理
            if (request.responseCode == 401)
            {
                throw new UnauthorizedAccessException("Unauthorized access");
            }

            // 其他錯誤處理
            throw new Exception($"Error: {request.error}, HTTP Status Code: {request.responseCode}");
        }

        // 如果成功，返回結果
        OnSuccess?.Invoke(request);
        var result = request.downloadHandler.text;
        Debug.Log("Response: " + result);
        return result;
    }
}