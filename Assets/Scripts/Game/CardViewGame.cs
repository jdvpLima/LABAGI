using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardViewGame : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI subText;        // show suit / points / rarity
    public TextMeshProUGUI flavourText;
    public TextMeshProUGUI actionsText;    // join actions list
    public Button button;

    private Card card;
    private Player owner;

    public void Init(Card card, Player owner)
    {
        this.card = card;
        this.owner = owner;

        titleText.text = card.Name;
        subText.text = card.Suit;
        flavourText.text = card.FlavourText;
        actionsText.text = string.Join("\n", card.Actions);
    }

    public void OnClick()
    {
        owner.selectedCard = card;
        Debug.Log("Selected card: " + card.Name);
    }




    /*public Card CardData { get; private set; }

    // onClick callback receives this CardView so the manager can access CardData
    public void SetCard(Card card, Action<CardViewGame> onClick)
    {
        CardData = card;
        if (titleText != null) titleText.text = card?.Name ?? "Unknown";
        if (subText != null) subText.text = $"{card?.Suit}  |  Points: {card?.Points}  |  {card?.Rarity}";
        if (flavourText != null) flavourText.text = card?.FlavourText ?? "";
        if (actionsText != null)
        {
            if (card?.Actions != null && card.Actions.Count > 0)
                actionsText.text = string.Join(", ", card.Actions);
            else
                actionsText.text = "";
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick?.Invoke(this));
        }
    }*/
}
