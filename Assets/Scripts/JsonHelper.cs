using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public static class JsonHelper
    {
        [Serializable]
        private class Wrapper<T> { public List<T> Items; }

        public static List<T> FromJsonList<T>(string json)
        {
            string wrapped = "{\"Items\":" + json + "}";
            var w = JsonUtility.FromJson<Wrapper<T>>(wrapped);
            return w.Items;
        }
        public static string ToJsonArray<T>(List<T> list)
        {
            var w = new Wrapper<T> { Items = list };
            var json = JsonUtility.ToJson(w);

            // JsonUtility gera algo do género: {"Items":[ ... ]}
            int start = json.IndexOf('[');
            int end = json.LastIndexOf(']');

            if (start >= 0 && end > start)
                return json.Substring(start, end - start + 1);

            return "[]";
        }
    }
}