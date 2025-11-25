using Assets.Scripts.CreateDeck;
using Assets.Scripts.Model;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.Service
{
    public class CardService
    {

        private const string BaseUrl = "https://lagabi-group2-backend.onrender.com/api/Cards";

        
        public async Task<List<CardDto>> GetPlayerCardCollectionAsync(long userId)
        {
            using (UnityWebRequest req = UnityWebRequest.Get(BaseUrl + "/collection/" + userId.ToString()))
            {
                req.SetRequestHeader("X-User", userId.ToString());

                var operation = req.SendWebRequest();

                while (!operation.isDone)
                    await Task.Yield();

                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("GET /Card/collection failed: " + req.error);
                    return null;
                }

                string json = req.downloadHandler.text;
                return JsonHelper.FromJsonList<CardDto>(json);
            }
        }
    }
}