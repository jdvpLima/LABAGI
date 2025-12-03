using System;
using UnityEngine;
using UnityEngine.UI;

public class CardViewGame : MonoBehaviour
{
    public Text titleText;
    public Text subText;        // show suit / points / rarity
    public Text flavourText;
    public Text actionsText;    // join actions list
    public Button button;

    public Card CardData { get; private set; }

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
    }
}
