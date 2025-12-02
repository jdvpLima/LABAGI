using Assets.Scripts.Model;
using Assets.Scripts.Workshop;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
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
                    Debug.LogError("[CardService] Error while parsing JSON: " + e + "\nJSON = " + json);
                    return new List<WorkshopCardDTO>();
                }
            }
        }
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
                    var list = JsonConvert.DeserializeObject<List<WorkshopCardDTO>>(json);
                    return list ?? new List<WorkshopCardDTO>();
                }
                catch (System.Exception e)
                {
                    Debug.LogError("[CardService] Error while parsing JSON: " + e + "\nJSON = " + json);
                    return new List<WorkshopCardDTO>();
                }
            }
        }

        public async Task<WorkshopCardDTO> UpsertWorkshopCardAsync(long userId, WorkshopCardDTO dto)
        {
            string jsonBody = JsonUtility.ToJson(dto);
            var bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

            using (var req = new UnityWebRequest($"{BaseUrl}/workshop/{userId}", "POST"))
            {
                req.uploadHandler = new UploadHandlerRaw(bodyRaw);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");

                req.SetRequestHeader("X-User", userId.ToString());

                var op = req.SendWebRequest();
                while (!op.isDone)
                    await Task.Yield();

                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("POST /Cards/workshop failed: " + req.error);
                    return null;
                }

                string json = req.downloadHandler.text;

                var saved = JsonUtility.FromJson<WorkshopCardDTO>(json);
                return saved;
            }
        }
        public async Task<bool> GrantCardToInventoryAsync(long userId, long cardId, short quantity = 4)
        {
            var payload = new[]
            {
                new InventoryGrantPayload { cardId = cardId, quantity = quantity }
            };

            // Precisamos de um array na raiz: [ { cardId, quantity } ]
            var json = JsonConvert.SerializeObject(payload);

            using (var req = new UnityWebRequest($"{BaseUrl}/inventory/{userId}", "POST"))
            {
                var bodyRaw = Encoding.UTF8.GetBytes(json);
                req.uploadHandler = new UploadHandlerRaw(bodyRaw);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");

                var op = req.SendWebRequest();
                while (!op.isDone)
                    await Task.Yield();

                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("POST /Cards/inventory failed: " + req.error +
                                   "\nRequest JSON = " + json +
                                   "\nResponse = " + req.downloadHandler.text);
                    return false;
                }

                return true;
            }
        }

        public async Task DeleteCardAsync(long userId, long cardId)
        {

            using (var req = new UnityWebRequest($"{BaseUrl}/{cardId}", "DELETE"))
            {
                req.SetRequestHeader("Content-Type", "application/json");

                req.SetRequestHeader("X-User", userId.ToString());

                var op = req.SendWebRequest();
                while (!op.isDone)
                    await Task.Yield();

                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"DELETE /Cards/{cardId} failed: " + req.error);
                }
            }
        }
    }
}