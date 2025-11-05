using UnityEngine;
using System.Collections.Generic;
using System;

namespace Smarteye.RestAPI
{
    [CreateAssetMenu]
    public class TargetAPIConfig : ScriptableObject
    {
        public string baseUrl;
        public List<endpointTarget> endpoints;

        [Serializable]
        public struct endpointTarget
        {
            public string title;
            public string targetEndpoint;
        }

        [Header("Authorization")]
        public bool isUsingBasicAuth;
        public string username;
        public string password;

        [Space(10f)]
        public bool isUsingBearerAuth;
        public string jwtToken;


        public string GetEndpoint(string _title)
        {
            var target = endpoints.Find((x) => x.title == _title);
            return target.targetEndpoint;
        }
    }
}