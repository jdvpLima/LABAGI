using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.CreateDeck
{
   
    public class DeckService
    {
        private const string BaseUrl = "https://lagabi-group2-backend.onrender.com/api/decks";

        //---------------------------------------------------------------------
        // GET /api/decks  (com header do user)
        //---------------------------------------------------------------------
        public async Task<List<DecksDto>> GetDecksAsync(long userId)
        {
            using (UnityWebRequest req = UnityWebRequest.Get(BaseUrl))
            {
                req.SetRequestHeader("X-User", userId.ToString());

                var operation = req.SendWebRequest();

                while (!operation.isDone)
                    await Task.Yield();

                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("GET /decks failed: " + req.error);
                    return null;
                }

                string json = req.downloadHandler.text;
                return JsonHelper.FromJsonList<DecksDto>(json);
            }
        }

        //---------------------------------------------------------------------
        // POST /api/decks  (envia um DeckDto)
        //---------------------------------------------------------------------
        public async Task<bool> PostDeckAsync(long userId,DecksDto deck)
        {
            string json = JsonUtility.ToJson(deck);

            using (UnityWebRequest req = new UnityWebRequest(BaseUrl, "POST"))
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