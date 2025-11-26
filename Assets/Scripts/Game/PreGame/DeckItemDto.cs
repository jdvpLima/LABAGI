using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class DeckItemDto : MonoBehaviour {
    public Button rootButton;

    Assets.Scripts.CreateDeck.DecksDto dto;
    Action<Assets.Scripts.CreateDeck.DecksDto> onClick;

    public void Setup(Assets.Scripts.CreateDeck.DecksDto deckDto, Action<Assets.Scripts.CreateDeck.DecksDto> onClick) {
        this.dto = deckDto;
        this.onClick = onClick;
        int qty = dto.cards != null ? dto.cards.Count : 0;
        rootButton.onClick.RemoveAllListeners();
        rootButton.onClick.AddListener(() => this.onClick?.Invoke(this.dto));
    }
}
