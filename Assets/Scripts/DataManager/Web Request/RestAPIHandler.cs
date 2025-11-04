using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System;
using Newtonsoft.Json;

namespace Smarteye.RestAPI
{
    public abstract class RestAPIHandler : MonoBehaviour
    {
        public RestAPI restAPI;
        public abstract void OnSuccessResult(JObject result);
        public abstract void OnProtocolErr(JObject result);

        // Utility untuk format error agar lebih rapi
        protected string GetFormattedError(JObject errorObj)
        {
            try
            {
                return JsonConvert.SerializeObject(errorObj, Formatting.Indented);
            }
            catch
            {
                return errorObj.ToString();
            }
        }

        // versi minimal: persis seperti baris yang kamu tulis, tapi generic
        protected List<T> ParseList<T>(JToken token)
        {
            // kalau token null, kembalikan list kosong biar aman
            if (token == null || token.Type == JTokenType.Null)
                return new List<T>();

            return JsonConvert.DeserializeObject<List<T>>(token.ToString());
        }
    }

    [Serializable]
    public class RowDataObject
    {
        public Dictionary<string, object> baseData;

        public RowDataObject(Dictionary<string, object> _baseData)
        {
            baseData = _baseData;
        }
    }
}