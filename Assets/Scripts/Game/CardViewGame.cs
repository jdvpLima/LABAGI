using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardViewGame : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI subText;        
    public TextMeshProUGUI flavourText;
    public TextMeshProUGUI actionsText;   
    public Button button;

    public Card card;
    private Player owner;

    public event Action<CardViewGame> OnCardClicked;

    public void Init(Card card, Player owner)
    {
        this.card = card;
        this.owner = owner;

        titleText.text = card.Name;
        subText.text = card.Suit;
        flavourText.text = card.FlavourText;

        // --- FIX: Check for Null Actions ---
        if (card.Actions != null && card.Actions.Count > 0)
        {
            actionsText.text = string.Join("\n", card.Actions);
        }
        else
        {
            actionsText.text = ""; // Empty string if null
        }
        // -----------------------------------

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            OnCardClicked?.Invoke(this);
            owner.PickCard(this);
            Debug.Log("Selected card: " + card.Name);
        });
    }
}