using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json; // Install Newtonsoft.Json (via Package Manager or Unity's com.unity.nuget.newtonsoft-json)

namespace Assets.Scripts
{
    [Serializable]
    public class DeckCardDTO
    {
        public long cardId;
        public int qty;
    }

    [Serializable]
    public class DeckDTO
    {
        public long id;
        public string name;
        public List<DeckCardDTO> cards;
    }

    public class DeckService : MonoBehaviour
    {
        public static DeckService Instance { get; private set; }

        // Change baseUrl to your production/qa base URL
        private string baseUrl = "https://lagabi-group2-backend.onrender.com";

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Fetches the full deck list (with cards) for the currently logged-in user.
        /// </summary>
        public void FetchUserDecks(Action<List<DeckDTO>> onSuccess, Action<string> onError = null)
        {
            if (!AuthContext.IsLoggedIn)
            {
                onError?.Invoke("User not logged in");
                return;
            }

            var url = $"{baseUrl}/api/decks/UserDeckList?userId={AuthContext.UserId}";
            StartCoroutine(GetUserDecksCoroutine(url, onSuccess, onError));
        }

        private IEnumerator GetUserDecksCoroutine(string url, Action<List<DeckDTO>> onSuccess, Action<string> onError)
        {
            using (var req = UnityWebRequest.Get(url))
            {
                // Required headers: Authorization and X-User
                req.SetRequestHeader("Authorization", "Bearer " + AuthContext.SessionToken);
                req.SetRequestHeader("X-User", AuthContext.UserId.ToString());
                req.SetRequestHeader("Accept", "application/json");

                yield return req.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
                bool isNetworkError = req.result == UnityWebRequest.Result.ConnectionError;
                bool isHttpError = req.result == UnityWebRequest.Result.ProtocolError;
#else
                bool isNetworkError = req.isNetworkError;
                bool isHttpError = req.isHttpError;
#endif

                if (isNetworkError)
                {
                    onError?.Invoke("Network error: " + req.error);
                    yield break;
                }

                if (isHttpError)
                {
                    if (req.responseCode == 401 || req.responseCode == 403)
                    {
                        onError?.Invoke("Unauthorized. Session token might be expired (401).");
                    }
                    else if (req.responseCode == 204)
                    {
                        onSuccess?.Invoke(new List<DeckDTO>()); // no content, return empty
                    }
                    else
                    {
                        onError?.Invoke($"HTTP Error {req.responseCode}: {req.downloadHandler.text}");
                    }
                    yield break;
                }

                // Success: parse JSON
                try
                {
                    var json = req.downloadHandler.text;
                    var decks = JsonConvert.DeserializeObject<List<DeckDTO>>(json);
                    onSuccess?.Invoke(decks);
                }
                catch (Exception ex)
                {
                    onError?.Invoke("JSON parse error: " + ex.Message);
                }
            }
        }
    }
}
