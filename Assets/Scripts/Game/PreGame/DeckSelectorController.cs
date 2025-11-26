using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using Assets.Scripts.CreateDeck; 

public class DeckSelectorController : MonoBehaviour
{
    public GameObject deckItemPrefab;
    public Transform contentParent;
    public Button confirmButton;

    private DeckService _deckService = new DeckService();
    private List<DecksDto> fetchedDecks;
    private DecksDto selectedDeck;

    private long userId = 12; 

    async void Start()
    {
        
        confirmButton.interactable = false;
        

        var decks = await _deckService.GetDecksAsync(userId);
        if (decks == null)
        {
            Debug.Log("Failed to load decks. Check network.");
            return;
        }

        fetchedDecks = decks;
        PopulateList();
    }

    void PopulateList()
    {
        foreach (Transform t in contentParent) Destroy(t.gameObject);

        foreach (var dto in fetchedDecks)
        {
            var go = Instantiate(deckItemPrefab, contentParent);
            var di = go.GetComponent<DeckItemDto>();
            if (di != null) di.Setup(dto, OnDeckClicked);
        }
    }

    void OnDeckClicked(DecksDto dto)
    {
        selectedDeck = dto;
        confirmButton.interactable = true;
    }
/*
    void ShowDetails(DecksDto dto)
    {
        detailsPanel.SetActive(true);
        detailsTitleText.text = dto.name ?? $"Deck #{dto.id}";
        var sb = new System.Text.StringBuilder();
        if (dto.cards != null && dto.cards.Count > 0)
        {
            foreach (var c in dto.cards)
                sb.AppendLine($"Card ID: {c.cardId}  x{c.qty}");
        }
        else sb.AppendLine("<empty deck>");
        detailsBodyText.text = sb.ToString();
    }*/

    public void OnConfirmButtonPressed()
    {
        if (selectedDeck == null) return;
        DeckManager.Instance.SelectedDeckDto = selectedDeck;
        SceneManager.LoadScene("Game");
    }
}
