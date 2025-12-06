using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json; // IMPORT THIS

namespace Assets.Scripts.CreateDeck
{
    public class DeckService
    {
        private const string BaseUrl = "https://lagabi-group2-backend.onrender.com/api/Decks";

        public async Task<List<DecksDto>> GetDecksAsync(long userId)
        {
            // Debug check to ensure we aren't sending "0" if we shouldn't
            if (userId == 0) Debug.LogWarning("DeckService: Requesting decks for UserId 0. Is this intended?");

            string url = $"{BaseUrl}/UserDeckList?userId={userId}";
            
            using (UnityWebRequest req = UnityWebRequest.Get(url))
            {
                req.SetRequestHeader("X-User", userId.ToString());

                var operation = req.SendWebRequest();

                while (!operation.isDone)
                    await Task.Yield();

                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"GET /decks failed: {req.error} | Response: {req.downloadHandler.text}");
                    return null;
                }

                string json = req.downloadHandler.text;
                Debug.Log($"Fetched Decks JSON: {json}"); // Helpful for debugging

                try
                {
                    // Use Newtonsoft to handle [Array] responses correctly
                    return JsonConvert.DeserializeObject<List<DecksDto>>(json);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"JSON Parse Error: {ex.Message}");
                    return new List<DecksDto>();
                }
            }
        }

        //---------------------------------------------------------------------
        // POST /api/decks  (envia um DeckDto)
        //---------------------------------------------------------------------
        public async Task<bool> PostDeckAsync(long userId,DecksDto deck)
        {
            string json = JsonUtility.ToJson(deck);

            using (UnityWebRequest req = new UnityWebRequest(BaseUrl + "?userId=" + userId , "POST"))
            {
                byte[] body = Encoding.UTF8.GetBytes(json);
                req.uploadHandler = new UploadHandlerRaw(body);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                req.SetRequestHeader("X-User", userId.ToString());

                var operation = req.SendWebRequest();

                while (!operation.isDone)
                    await Task.Yield();

                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("POST /decks failed: " + req.error);
                    return false;
                }

                return true;
            }
        }

 
    }





}