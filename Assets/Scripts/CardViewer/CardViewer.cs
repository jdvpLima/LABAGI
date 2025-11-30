using Assets.Scripts.CreateDeck;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardViewer : MonoBehaviour
{
    private DeckService _deckService = new DeckService();

    public GameObject cardPrefab;
    public Transform cardCollectionView;

    // kur korta turi atsidurti kai yra fullscreen (pvz. Canvas ar speciali panelė)
    public RectTransform fullscreenParent;

    private long userID;

    async void Start()
    {
        userID = AuthBootstrapper.CurrentUserId != 0 ? AuthBootstrapper.CurrentUserId : 7;

        List<DecksDto> decks = await _deckService.GetDecksAsync(userID);

        if (decks == null || decks.Count == 0)
        {
            Debug.LogWarning("No decks found for user " + userID);
            return;
        }

        foreach (var deck in decks)
        {
            Debug.Log("Deck: " + deck.name + " ID: " + deck.id);
        }

        // Just load and display all cards from the first deck (collection)
        foreach (DeckCards card in decks[0].cards)
        {
            SpawnCard(card.cardId, cardCollectionView);
        }
    }

    void SpawnCard(long id, Transform parentUI)
    {
        GameObject card = Instantiate(cardPrefab, parentUI);

        var view = card.GetComponent<CardView>();
        if (view != null)
        {
            // užkrauna vizualą pagal id
            view.Initialize(id, parentUI, null);

            // išjungiame elgseną (kad jis nejudintų kortų ir pan.)
            view.enabled = false;
        }

        // fullscreen toggle komponentas
        var fs = card.GetComponent<CardFullscreenToggle>();
        if (fs == null)
        {
            fs = card.AddComponent<CardFullscreenToggle>();
        }

        fs.Init(
            originalParent: parentUI as RectTransform,
            fullscreenParent: fullscreenParent
        );

        // Button click → fullscreen toggle
        var btn = card.GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.interactable = true;
            btn.onClick.AddListener(fs.ToggleFullscreen);
        }
    }
}
