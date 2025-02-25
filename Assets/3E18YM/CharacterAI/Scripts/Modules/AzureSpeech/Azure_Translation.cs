using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace CharacterAI.Modules.AzureSpeech
{
    public class Azure_Translation : MonoBehaviour
    {
        // Start is called before the first frame update

        public async Task<string> GetAzureTranslate(string textToTranslate, string Language)
        {

            var tcs = new TaskCompletionSource<string>();
            StartCoroutine(TranslateText(textToTranslate, Language, (result) => tcs.SetResult(result)));
            return await tcs.Task;


        }

        IEnumerator TranslateText(string textToTranslate, string Language, Action<string> _callback)
        {
            string route = $"/translate?api-version=3.0&from=en&to={Language}";
            object[] body = new object[] { new { Text = textToTranslate } };
            string requestBody = JsonConvert.SerializeObject(body);

            using (UnityWebRequest request = new UnityWebRequest("https://api.cognitive.microsofttranslator.com/" + route, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(requestBody);
                request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Ocp-Apim-Subscription-Key", "105d70afeb0d4a30a1413c1a40271ebe");
                request.SetRequestHeader("Ocp-Apim-Subscription-Region", "eastasia");

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(request.error);
                }
                else
                {
                    string json = request.downloadHandler.text;
                    List<TranslationResponse> translationList = JsonConvert.DeserializeObject<List<TranslationResponse>>(json);

                    foreach (var translationResponse in translationList)
                    {
                        foreach (var translation in translationResponse.translations)
                        {
                            Debug.Log($"Text: {translation.text}, To: {translation.to}");
                            _callback(translation.text);
                        }
                    }

                }
            }


        }


        [Serializable]
        public class TranslationItem
        {
            public string text;
            public string to;
        }

        // 定义与JSON数组中的对象匹配的类
        public class TranslationResponse
        {
            public List<TranslationItem> translations;
        }
    }
}
