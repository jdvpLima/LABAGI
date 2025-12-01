using Assets.Scripts.CreateDeck;
using Assets.Scripts.Model;
using Assets.Scripts.Workshop;
using Newtonsoft.Json;
using System;
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
        // Cartas runtime (para o workshop)
        public async Task<List<WorkshopCardDTO>> GetRuntimeCardsAsync()
        {
            using (var req = UnityWebRequest.Get($"{BaseUrl}/runtime"))
            {
                var op = req.SendWebRequest();
                while (!op.isDone)
                    await Task.Yield();

                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("GET /Cards/runtime failed: " + req.error);
                    return new List<WorkshopCardDTO>();
                }

                var json = req.downloadHandler.text;

                try
                {
                    var list = JsonConvert.DeserializeObject<List<WorkshopCardDTO>>(json);
                    return list ?? new List<WorkshopCardDTO>();
                }
                catch (System.Exception e)
                {
                    Debug.LogError("[CardService] Erro a parsear JSON de runtime (Newtonsoft): " + e + "\nJSON = " + json);
                    return new List<WorkshopCardDTO>();
                }
            }
        }

        // Cartas de workshop do utilizador
        public async Task<List<WorkshopCardDTO>> GetUserWorkshopCardsAsync(long userId)
        {
            using (var req = UnityWebRequest.Get($"{BaseUrl}/workshop/{userId}"))
            {
                req.SetRequestHeader("X-User", userId.ToString());

                var op = req.SendWebRequest();
                while (!op.isDone)
                    await Task.Yield();

                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("GET /Cards/workshop failed: " + req.error);
                    return new List<WorkshopCardDTO>();
                }

                var json = req.downloadHandler.text;

                try
                {
                    var list = JsonUtility.FromJson<List<WorkshopCardDTO>>(json);
                    return list ?? new List<WorkshopCardDTO>();
                }
                catch (System.Exception e)
                {
                    Debug.LogError("[CardService] Erro a parsear JSON de workshop (Newtonsoft): " + e + "\nJSON = " + json);
                    return new List<WorkshopCardDTO>();
                }
            }
        }


        // Upsert de uma carta de workshop
        public WorkshopCardDTO UpsertWorkshopCard(long userId, WorkshopCardDTO dto)
        {
            string jsonBody = JsonUtility.ToJson(dto);
            var bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);

            using (var req = new UnityWebRequest($"{BaseUrl}/workshop", "POST"))
            {
                req.uploadHandler = new UploadHandlerRaw(bodyRaw);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                req.SetRequestHeader("X-User", userId.ToString());

                var op = req.SendWebRequest();
               
                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("POST /Cards/workshop failed: " + req.error);
                    return null;
                }

                string json = req.downloadHandler.text;
                // aqui assumo que o backend devolve um DTO único, não uma lista
                return JsonUtility.FromJson<WorkshopCardDTO>(json);
            }
        }
    }
}