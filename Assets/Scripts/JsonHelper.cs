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
            Wrapper<T> w = JsonUtility.FromJson<Wrapper<T>>(wrapped);
            return w.Items;
        }
    }
}