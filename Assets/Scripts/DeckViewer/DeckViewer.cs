using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class DeckListViewer : MonoBehaviour
{
    public GameObject deckPrefab;         // prefab su Text arba TMP_Text
    public Transform deckListParent;      // kur sudėti visus deckus

    private long userId;
    private const string BaseUrl = "https://your-api-url.com/api/decks";

    [System.Serializable]
    public class DeckSummaryDto
    {
        public long id;
        public string name;
        public string createdAt;
    }

    // Wrapperis Unity Json deserializacijai iš masyvo
    [System.Serializable]
    private class DeckListWrapper<T>
    {
        public T[] items;
    }

    async void Start()
    {
        userId = AuthBootstrapper.CurrentUserId != 0 ? AuthBootstrapper.CurrentUserId : 7;

        List<DeckSummaryDto> decks = await LoadDecks(userId);

        if (decks == null || decks.Count == 0)
        {
            Debug.Log("User has no decks.");
            return;
        }

        foreach (var deck in decks)
        {
            SpawnDeck(deck);
        }
    }

    async Task<List<DeckSummaryDto>> LoadDecks(long userId)
    {
        string url = $"{BaseUrl}?userId={userId}";
        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            var op = req.SendWebRequest();
            while (!op.isDone)
                await Task.Yield();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to load deck list: " + req.error);
                return null;
            }

            string json = req.downloadHandler.text;

            // Unity neparskaitys tiesiai masyvo, reikia wrapinti
            string wrapped = "{\"items\":" + json + "}";
            DeckListWrapper<DeckSummaryDto> wrapper =
                JsonUtility.FromJson<DeckListWrapper<DeckSummaryDto>>(wrapped);

            return new List<DeckSummaryDto>(wrapper.items);
        }
    }

    void SpawnDeck(DeckSummaryDto deck)
    {
        GameObject obj = Instantiate(deckPrefab, deckListParent);

        var txt = obj.GetComponentInChildren<Text>();
        if (txt != null)
            txt.text = deck.name;

        var btn = obj.GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                Debug.Log("Selected deck ID: " + deck.id);
                // jei reikės — čia iškviesti kortų užkrovimą
            });
        }
    }
}
