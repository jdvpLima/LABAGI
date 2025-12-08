using System.Collections.Generic;
using System.Linq;                        // <- dėl ToDictionary
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Assets.Scripts.CreateDeck;
using Assets.Scripts;                    // AuthBootstrapper, SelectedDeckHolder
using Assets.Scripts.Service;
using Assets.Scripts.Model;              // <- dėl CardDto

public class CardViewer : MonoBehaviour
{
    public GameObject cardPrefab;
    public Transform cardCollectionView;
    public RectTransform fullscreenParent;

    private DeckService deckService = new DeckService();
    private CardService cardService = new CardService();

    private Dictionary<long, CardDto> cardLookup;

    async void Start()
    {
        long userId = AuthBootstrapper.CurrentUserId != 0
            ? AuthBootstrapper.CurrentUserId
            : 12;

        // 1) užsikraunam visas žaidėjo kortas
        var collection = await cardService.GetPlayerCardCollectionAsync(userId);

        if (collection == null)
        {
            Debug.LogError("CardViewer: Could not load card collection");
            return;
        }

        cardLookup = collection.ToDictionary(c => c.cardId, c => c);

        // 2) naudojam pasirinktą decką
        if (SelectedDeckHolder.SelectedDeck != null)
        {
            var deckFromHolder = SelectedDeckHolder.SelectedDeck;
            Debug.Log($"CardViewer: using SelectedDeckHolder deck {deckFromHolder.id} ({deckFromHolder.name})");
            LoadDeckFromDto(deckFromHolder);
            return;
        }

        Debug.LogError("CardViewer: No deck found in SelectedDeckHolder.");
    }

    private void LoadDeckFromDto(DecksDto deck)
    {
        if (deck.cards == null || deck.cards.Count == 0)
        {
            Debug.LogWarning($"CardViewer: deck {deck.id} ({deck.name}) has no cards.");
            return;
        }

        foreach (DeckCards deckCard in deck.cards)
        {
            if (!cardLookup.TryGetValue(deckCard.cardId, out CardDto dto))
            {
                Debug.LogWarning($"CardViewer: card {deckCard.cardId} not found in player collection.");
                continue;
            }

            for (int i = 0; i < deckCard.qty; i++)
            {
                SpawnCard(dto, cardCollectionView);
            }
        }
    }

    void SpawnCard(CardDto dto, Transform parentUI)
    {
        if (cardPrefab == null)
        {
            Debug.LogError("CardViewer: cardPrefab is not assigned.");
            return;
        }

        GameObject card = Instantiate(cardPrefab, parentUI);

        // tas pats pattern kaip CreateDeckManager
        Card data = Card.FromDto(dto);

        var view = card.GetComponent<CardView>();
        if (view != null)
        {
            view.Init(data, parentUI, null);   // naudok tą patį metodą kaip ten (Init, ne Initialize, jei toks pas tave)
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

    public void BackButton()
    {
        if (ARModeSwitcher.Instance != null && ARModeSwitcher.ar_active)
        {
            SceneManager.UnloadSceneAsync("CardViewer2");
        }
        else
            SceneManager.LoadScene("PreGame");
    }
}
