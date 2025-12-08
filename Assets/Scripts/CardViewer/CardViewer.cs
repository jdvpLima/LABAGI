using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.CreateDeck;
using Assets.Scripts; // dėl AuthBootstrapper, SelectedDeckHolder
using Assets.Scripts.Service;
using UnityEngine.SceneManagement;


public class CardViewer : MonoBehaviour
{
    public GameObject cardPrefab;
    public Transform cardCollectionView;
    public RectTransform fullscreenParent;

    private DeckService deckService = new DeckService();

    [SerializeField] private long targetDeckId = 25; // fallback, jei neateina SelectedDeckHolder

    private long userId;

    async void Start()
    {
        // 1) Jei DeckListUI jau nustatė pasirinktą deck'ą, naudok jį tiesiogiai.
        if (SelectedDeckHolder.SelectedDeck != null)
        {
            var deckFromHolder = SelectedDeckHolder.SelectedDeck;
            Debug.Log($"CardViewer: using SelectedDeckHolder deck {deckFromHolder.id} ({deckFromHolder.name})");
            LoadDeckFromDto(deckFromHolder);
            return;
        }

        // 2) Jei SelectedDeckHolder tuščias, bandome surasti decką pagal ID ir galimus userId.
        var possibleUserIds = new List<long>();

        // realus prisijungęs useris, jei yra
        if (AuthBootstrapper.CurrentUserId != 0)
        {
            possibleUserIds.Add(AuthBootstrapper.CurrentUserId);
        }

        // debug fallback’ai, kurie jau egzistuoja kituose scriptuose
        if (!possibleUserIds.Contains(12)) possibleUserIds.Add(12); // CreateDeckManager fallback
        if (!possibleUserIds.Contains(1))  possibleUserIds.Add(1);  // DeckListUI fallback

        DecksDto foundDeck = null;
        long foundUserId = 0;

        foreach (var uid in possibleUserIds)
        {
            Debug.Log($"CardViewer: trying to fetch decks for user {uid}");
            List<DecksDto> decks = await deckService.GetDecksAsync(uid);

            if (decks == null || decks.Count == 0)
            {
                Debug.LogWarning($"CardViewer: user {uid} has NO decks.");
                continue;
            }

            foreach (var d in decks)
            {
                Debug.Log($"CardViewer: found deck {d.id} ({d.name}) for user {uid}");
            }

            var deck = decks.Find(d => d.id == targetDeckId);
            if (deck != null)
            {
                foundDeck = deck;
                foundUserId = uid;
                break;
            }
        }

        if (foundDeck == null)
        {
            Debug.LogWarning(
                $"CardViewer: deck {targetDeckId} NOT FOUND for any of userIds: " +
                string.Join(", ", possibleUserIds)
            );
            return;
        }

        userId = foundUserId;
        Debug.Log($"CardViewer: LOADING deck {foundDeck.id} ({foundDeck.name}) for user {userId}");
        LoadDeckFromDto(foundDeck);
    }

    private void LoadDeckFromDto(DecksDto deck)
    {
        if (deck.cards == null || deck.cards.Count == 0)
        {
            Debug.LogWarning($"CardViewer: deck {deck.id} ({deck.name}) has no cards.");
            return;
        }

        // Tik pasirinkto deck'o kortos
        foreach (DeckCards card in deck.cards)
        {
            for (int i = 0; i < card.qty; i++)
            {
                SpawnCard(card.cardId, cardCollectionView);
            }
        }
    }

    public void BackButton()
    {
        Debug.Log("BACK2BACK2BACK");
        SceneManager.LoadScene("PreGame");
    }

    void SpawnCard(long id, Transform parentUI)
    {
        if (cardPrefab == null)
        {
            Debug.LogError("CardViewer: cardPrefab is not assigned.");
            return;
        }

        GameObject card = Instantiate(cardPrefab, parentUI);

        var view = card.GetComponent<CardView>();
        if (view != null)
        {
            view.Initialize(id, parentUI, null);
        }

        var fs = card.GetComponent<CardFullscreenToggle>();
        if (fs == null)
            fs = card.AddComponent<CardFullscreenToggle>();

        fs.Init(parentUI as RectTransform, fullscreenParent);

        var btn = card.GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.interactable = true;
            btn.onClick.AddListener(fs.ToggleFullscreen);
        }
    }
}
