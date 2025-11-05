using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;

namespace Smarteye.RestAPI
{
    public class RestAPI : MonoBehaviour
    {
        public TargetAPIConfig targetAPIConfig;

        #region Public Methods

        public void PostAction(Dictionary<string, object> _data, Action<JObject> success, Action<JObject> err, string _endpointTitle, string uniqValue = "")
        {
            string jsonData = JsonConvert.SerializeObject(_data);
            string fullUri = targetAPIConfig.baseUrl + targetAPIConfig.GetEndpoint(_endpointTitle) + uniqValue;
            StartCoroutine(Post(fullUri, jsonData, success, err));
        }

        public void PatchAction(string _endpointTitle, string patchData, Action<JObject> success, Action<JObject> err)
        {
            string fullUri = targetAPIConfig.baseUrl + targetAPIConfig.GetEndpoint(_endpointTitle) + patchData;
            StartCoroutine(Patch(fullUri, success, err));
        }

        public void PatchActionWithBody(Dictionary<string, object> _data, string _endpointTitle, string patchData, Action<JObject> success, Action<JObject> err)
        {
            string jsonData = JsonConvert.SerializeObject(_data);
            string fullUri = targetAPIConfig.baseUrl + targetAPIConfig.GetEndpoint(_endpointTitle) + patchData;
            StartCoroutine(PatchWithBody(fullUri, jsonData, success, err));
        }

        public void GetWithAuthorization(string _endpointTitle, Action<JObject> success, Action<JObject> err)
        {
            string uri = targetAPIConfig.baseUrl + targetAPIConfig.GetEndpoint(_endpointTitle);
            StartCoroutine(GetWithAuthCoroutine(uri, targetAPIConfig.username, targetAPIConfig.password, success, err));
        }

        public void GetWithAuthorization(string _endpointTitle, string id, Action<JObject> success, Action<JObject> err)
        {
            string uri = targetAPIConfig.baseUrl + targetAPIConfig.GetEndpoint(_endpointTitle) + id;
            StartCoroutine(GetWithAuthCoroutine(uri, targetAPIConfig.username, targetAPIConfig.password, success, err));
        }

        public void GetActionByID(string _endpointTitle, string id, Action<JObject> success, Action<JObject> err)
        {
            string uri = targetAPIConfig.baseUrl + targetAPIConfig.GetEndpoint(_endpointTitle) + id;
            StartCoroutine(Get(uri, success, err));
        }

        public void PostWithHeaderAndBody(string _endpointTitle, bool _isUsingToken, string _body, Action<JObject> _success, Action<JObject> _err)
        {
            string token = null;
            string uri = targetAPIConfig.baseUrl + targetAPIConfig.GetEndpoint(_endpointTitle);

            Dictionary<string, string> header = new Dictionary<string, string>();
            if (_isUsingToken)
            {
                token = targetAPIConfig.jwtToken;
                header["Authorization"] = $"Bearer {token}";
            }

            StartCoroutine(Post(uri, header, _body, _success, _err));
        }

        #endregion

        #region Coroutines

        private IEnumerator Get(string uri, Action<JObject> success, Action<JObject> err)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(uri))
            {
                yield return request.SendWebRequest();
                HandleResponse(request, success, err);
            }
        }

        private IEnumerator GetWithAuthCoroutine(string uri, string username, string password, Action<JObject> success, Action<JObject> err)
        {
            string encoded = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));

            using (UnityWebRequest request = UnityWebRequest.Get(uri))
            {
                request.SetRequestHeader("Accept", "application/json");
                request.SetRequestHeader("Authorization", $"Basic {encoded}");

                yield return request.SendWebRequest();
                HandleResponse(request, success, err);
            }
        }

        private IEnumerator Post(string uri, string jsonBody, Action<JObject> success, Action<JObject> err)
        {
            using (UnityWebRequest request = new UnityWebRequest(uri, "POST"))
            {
                byte[] rowData = Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(rowData);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();
                HandleResponse(request, success, err);
            }
        }

        private IEnumerator Post(string uri, Dictionary<string, string> headers, string jsonBody, Action<JObject> success, Action<JObject> err)
        {
            // kalau caller nggak ngirim header, bikin baru
            if (headers == null)
                headers = new Dictionary<string, string>();

            // kalau belum ada content-type, set default
            if (!headers.ContainsKey("Content-Type"))
                headers["Content-Type"] = "application/json";

            using (UnityWebRequest request = new UnityWebRequest(uri, "POST"))
            {
                byte[] rowData = Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(rowData);
                request.downloadHandler = new DownloadHandlerBuffer();

                // set semua header yang sudah disiapin
                foreach (var h in headers)
                    request.SetRequestHeader(h.Key, h.Value); ;

                yield return request.SendWebRequest();
                HandleResponse(request, success, err);
            }
        }

        private IEnumerator Patch(string uri, Action<JObject> success, Action<JObject> err)
        {
            using (UnityWebRequest request = new UnityWebRequest(uri, "PATCH"))
            {
                request.downloadHandler = new DownloadHandlerBuffer();
                yield return request.SendWebRequest();
                HandleResponse(request, success, err);
            }
        }

        private IEnumerator PatchWithBody(string uri, string jsonBody, Action<JObject> success, Action<JObject> err)
        {
            using (UnityWebRequest request = new UnityWebRequest(uri, "PATCH"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();
                HandleResponse(request, success, err);
            }
        }

        #endregion

        #region Helper

        private void HandleResponse(UnityWebRequest request, Action<JObject> success, Action<JObject> err)
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
#if UNITY_EDITOR
                Debug.Log($"[RESTAPI SUCCESS] URL: {request.url} | Status: {request.responseCode}");
#endif

                string text = request.downloadHandler.text;
                try
                {
                    var token = JToken.Parse(text);

                    if (token is JObject obj)
                    {
                        success?.Invoke(obj);
                    }
                    else
                    {
                        JObject wrapped = new JObject { ["data"] = token };
                        success?.Invoke(wrapped);
                    }
                }
                catch (JsonReaderException e)
                {
#if UNITY_EDITOR
                    Debug.LogError($"JSON Parse Error: {e.Message}");
#endif
                    err?.Invoke(new JObject { ["error"] = "Invalid JSON format" });
                }
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogError($"HTTP Error: {request.responseCode} - {request.error}");
#endif
                try
                {
                    JObject errorObj = JObject.Parse(request.downloadHandler.text);
                    err?.Invoke(errorObj);
                }
                catch
                {
#if UNITY_EDITOR
                    err?.Invoke(new JObject { ["error"] = "Unknown error occurred" });
#endif
                }
            }
        }

        #endregion
    }
}
