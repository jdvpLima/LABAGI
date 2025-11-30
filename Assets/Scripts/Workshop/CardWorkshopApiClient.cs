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
        [SerializeField] private string baseApiUrl = "https://o_teu_backend.com";
        [SerializeField] private string sessionToken; // podes injetar do teu SessionManager

        private void ApplyAuth(UnityWebRequest req)
        {
            if (!string.IsNullOrEmpty(sessionToken))
                req.SetRequestHeader("Authorization", "Bearer " + sessionToken);
        }
        public IEnumerator PostWorkshopCard(
        WorkshopCardDTO card,
        string status,                          // "draft" ou "active"
        Action<WorkshopCardDTO> onSuccess,
        Action<string> onError)
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
            var saved = JsonUtility.FromJson<WorkshopCardDTO>(responseJson);
            onSuccess?.Invoke(saved);
        }
        public IEnumerator GetWorkshopCard(long? cardId, Action<WorkshopCardDTO> onSuccess, Action<string> onError)
        {
            // cardId e userId ambos em query
            var url = $"{baseApiUrl}/api/Cards/workshop?cardId={cardId}&userId={AuthBootstrapper.CurrentUserId}";

            using var req = UnityWebRequest.Get(url);
            ApplyAuth(req);

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke(req.error);
                yield break;
            }

            var json = req.downloadHandler.text;
            var dto = JsonUtility.FromJson<WorkshopCardDTO>(json);
            onSuccess?.Invoke(dto);
        }

        public IEnumerator GetUserCollection(Action<List<WorkshopCardDTO>> onSuccess, Action<string> onError)
        {
            var url = $"{baseApiUrl}/api/Cards/collection?userId={AuthBootstrapper.CurrentUserId}";

            using var req = UnityWebRequest.Get(url);
            ApplyAuth(req);

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke(req.error);
                yield break;
            }

            var json = req.downloadHandler.text;
            var cards = JsonHelper.FromJsonList<WorkshopCardDTO>(json);
            onSuccess?.Invoke(cards);
        }
        public IEnumerator GetRuntimeCards(Action<List<WorkshopCardDTO>> onSuccess, Action<string> onError)
        {
            var url = $"{baseApiUrl}/api/Cards/runtime";

            using var req = UnityWebRequest.Get(url);
            ApplyAuth(req);

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke(req.error);
                yield break;
            }

            var json = req.downloadHandler.text;
            var cards = JsonHelper.FromJsonList<WorkshopCardDTO>(json);
            onSuccess?.Invoke(cards);
        }
    }
}
