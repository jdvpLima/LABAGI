using Assets.Scripts.Model;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.Workshop
{
    public class CardWorkshopApiClient : MonoBehaviour
    {
        [SerializeField] private string baseApiUrl = "https://lagabi-group2-backend.onrender.com";
        [SerializeField] private string sessionToken;

        private void ApplyAuth(UnityWebRequest req)
        {
            if (!string.IsNullOrEmpty(sessionToken))
                req.SetRequestHeader("Authorization", "Bearer " + sessionToken);
        }
        public IEnumerator PostWorkshopCard(CardDto card, string status, Action<CardDto> onSuccess, Action<string> onError)
        {
            card.status = status;

            var url = $"{baseApiUrl}/api/Cards/workshop?userId={AuthBootstrapper.CurrentUserId}";
            var json = JsonUtility.ToJson(card);
            var body = Encoding.UTF8.GetBytes(json);

            using var req = new UnityWebRequest(url, "POST");
            req.uploadHandler = new UploadHandlerRaw(body);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            ApplyAuth(req);

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke(req.error);
                yield break;
            }

            // se o endpoint devolver a carta atualizada (com id), podes ler:
            var responseJson = req.downloadHandler.text;
            var saved = JsonUtility.FromJson<CardDto>(responseJson);
            onSuccess?.Invoke(saved);
        }
        public IEnumerator GetWorkshopCard(long? cardId, Action<CardDto> onSuccess, Action<string> onError)
        {
            var url = $"{baseApiUrl}/api/Cards/{cardId}";

            using (var req = UnityWebRequest.Get(url))
            {
                ApplyAuth(req);

                yield return req.SendWebRequest();

                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("GET /Cards/{id} falhou: " + req.error);
                    onError?.Invoke(req.error);
                    yield break;
                }

                var json = req.downloadHandler.text; // esperado: { ... }

                CardDto card;
                try
                {
                    card = JsonUtility.FromJson<CardDto>(json);
                }
                catch (Exception e)
                {
                    Debug.LogError("[CardWorkshopApiClient] Erro a parsear JSON em GetWorkshopCard: " + e);
                    onError?.Invoke(e.Message);
                    yield break;
                }

                onSuccess?.Invoke(card);
            }
        }


        public IEnumerator GetUserCollection(Action<List<CardDto>> onSuccess, Action<string> onError)
        {
            var url = $"{baseApiUrl}/api/Cards/collection/{AuthBootstrapper.CurrentUserId}";

            using (var req = UnityWebRequest.Get(url))
            {
                ApplyAuth(req);

                yield return req.SendWebRequest();

                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("GET /Cards/collection falhou: " + req.error);
                    onError?.Invoke(req.error);
                    yield break;
                }

                var json = req.downloadHandler.text; // esperado: [ { ... }, ... ]

                List<CardDto> cards;
                try
                {
                    cards = JsonHelper.FromJsonList<CardDto>(json);
                }
                catch (Exception e)
                {
                    Debug.LogError("[CardWorkshopApiClient] Erro a parsear JSON em GetUserCollection: " + e);
                    onError?.Invoke(e.Message);
                    yield break;
                }

                onSuccess?.Invoke(cards ?? new List<CardDto>());
            }
        }

        public async Task<List<CardDto>> GetRuntimeCards(Action<List<CardDto>> onSuccess, Action<string> onError)
        {

            using (var req = UnityWebRequest.Get(baseApiUrl + "/api/Cards/runtime"))
            {
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
