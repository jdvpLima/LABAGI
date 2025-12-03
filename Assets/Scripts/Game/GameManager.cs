using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

// make sure this matches where your DecksDto lives:
using Assets.Scripts.CreateDeck; // DecksDto
// Card DTO assumed to be in global namespace as provided by you

public class GameManager : MonoBehaviour
{
    [Header("UI")]
    public Transform handContainer;    // UI parent for hand card prefabs
    public GameObject cardPrefab;      // prefab with CardView attached
    public Text statusText;

    [Header("Gameplay")]
    public int startingHandSize = 5;   // first 5 cards as requested
    public int expectedDeckSize = 20;  // optional check

    // runtime lists
    private List<Card> drawPile = new List<Card>();
    private List<Card> hand = new List<Card>();
    private List<Card> discard = new List<Card>();

    void Start()
    {
        var deckDto = SelectedDeckHolder.SelectedDeck;
        if (deckDto == null)
        {
            Debug.LogWarning("No selected deck found. Returning to deck selection or using fallback.");
            if (statusText != null) statusText.text = "No deck selected.";
            return;
        }

        if (!TryInitFromDeckDtoWithCards(deckDto))
        {
            Debug.LogError("Could not initialize deck from DecksDto. Make sure DecksDto contains a List<Card> under 'cards' or 'Cards'.");
            if (statusText != null) statusText.text = "Invalid deck format.";
            return;
        }

        StartGame();
    }

    // Attempt to find a list of Card objects inside the DecksDto (fields or properties)
    private bool TryInitFromDeckDtoWithCards(DecksDto deckDto)
    {
        // look for fields/properties named 'cards' or 'Cards'
        var candidates = new string[] { "cards", "Cards" };

        foreach (var name in candidates)
        {
            // field
            var field = deckDto.GetType().GetField(name, BindingFlags.Public | BindingFlags.Instance);
            if (field != null)
            {
                var obj = field.GetValue(deckDto) as System.Collections.IEnumerable;
                if (obj != null)
                {
                    drawPile = ConvertEnumerableToCardList(obj);
                    if (drawPile.Count > 0) return true;
                }
            }

            // property
            var prop = deckDto.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.CanRead)
            {
                var obj = prop.GetValue(deckDto, null) as System.Collections.IEnumerable;
                if (obj != null)
                {
                    drawPile = ConvertEnumerableToCardList(obj);
                    if (drawPile.Count > 0) return true;
                }
            }
        }

        return false;
    }

    // Convert a non-generic IEnumerable (coming from reflection) into List<Card> if possible
    private List<Card> ConvertEnumerableToCardList(System.Collections.IEnumerable enumerable)
    {
        var list = new List<Card>();
        foreach (var item in enumerable)
        {
            if (item is Card c)
            {
                list.Add(c);
            }
            else
            {
                // try mapping by reflection: create a Card using Name and Suit if available
                var mapped = MapToCard(item);
                if (mapped != null) list.Add(mapped);
            }
        }
        return list;
    }

    // Fallback mapping if server DTO type differs from your Card type
    private Card MapToCard(object obj)
    {
        if (obj == null) return null;
        var t = obj.GetType();

        // try to read name and suit (common minimal)
        var nameProp = t.GetProperty("Name") ?? t.GetProperty("name") ?? (MemberInfo)null;
        var suitProp = t.GetProperty("Suit") ?? t.GetProperty("suit") ?? (MemberInfo)null;

        string name = null;
        string suit = null;

        if (nameProp is PropertyInfo np) name = np.GetValue(obj)?.ToString();
        else if (nameProp is FieldInfo nf) name = nf.GetValue(obj)?.ToString();

        if (suitProp is PropertyInfo sp) suit = sp.GetValue(obj)?.ToString();
        else if (suitProp is FieldInfo sf) suit = sf.GetValue(obj)?.ToString();

        if (string.IsNullOrEmpty(name)) return null;

        var card = new Card(name, suit ?? "");
        // try to set Points / Rarity / FlavourText / Actions via reflection if present (use private setters? may fail)
        var pointsProp = t.GetProperty("Points") ?? t.GetProperty("points");
        if (pointsProp != null && int.TryParse(pointsProp.GetValue(obj)?.ToString(), out var p)) {
            // can't set Points (private setter) — skip or adjust DTO if needed
        }

        // If mapping beyond Name/Suit is required, consider adjusting DTOs to allow setting fields or using constructor that accepts values.
        return card;
    }

    private void StartGame()
    {
        if (drawPile == null || drawPile.Count == 0)
        {
            Debug.LogError("Draw pile empty at StartGame");
            return;
        }

        if (drawPile.Count != expectedDeckSize)
            Debug.LogWarning($"Deck has {drawPile.Count} cards (expected {expectedDeckSize}).");

        // Per your requirement: use first N cards (no shuffle)
        DrawInitialHand();
        RefreshHandUI();
        if (statusText != null) statusText.text = "Game started";
        Debug.Log($"Game started. Draw pile: {drawPile.Count}, Hand: {hand.Count}, Discard: {discard.Count}");
    }

    private void DrawInitialHand()
    {
        for (int i = 0; i < startingHandSize; i++)
        {
            DrawCard();
        }
    }

    public Card DrawCard()
    {
        if (drawPile == null || drawPile.Count == 0)
        {
            Debug.Log("Draw pile empty — reshuffling discard into draw pile (if any).");
            ReshuffleDiscardIntoDrawPile();
        }

        if (drawPile.Count == 0)
        {
            Debug.LogWarning("No cards left to draw.");
            return null;
        }

        var card = drawPile[0];
        drawPile.RemoveAt(0);
        hand.Add(card);
        RefreshHandUI();
        return card;
    }

    private void ReshuffleDiscardIntoDrawPile()
    {
        if (discard == null || discard.Count == 0) return;
        drawPile.AddRange(discard);
        discard.Clear();
        // If you want shuffle here add shuffle code
    }

    public void PlayCard(Card card)
    {
        if (card == null) return;

        if (!hand.Contains(card))
        {
            Debug.LogWarning("Trying to play a card not in hand.");
            return;
        }

        hand.Remove(card);
        discard.Add(card);

        // TODO: apply card actions (card.Actions) and game effects here
        Debug.Log($"Played card {card.Name} ({card.Suit}), Points={card.Points}, Rarity={card.Rarity}");
        RefreshHandUI();
    }

    #region UI

    private void RefreshHandUI()
    {
        if (handContainer == null || cardPrefab == null) return;

        // clear existing
        for (int i = handContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(handContainer.GetChild(i).gameObject);
        }

        // instantiate card views in order
        foreach (var c in hand)
        {
            var go = Instantiate(cardPrefab, handContainer);
            go.name = $"Card_{c.Name}_{c.Suit}";
            var view = go.GetComponent<CardViewGame>();
            if (view != null)
            {
                view.SetCard(c, OnCardViewClicked);
            }
            else
            {
                // fallback: set a child text if present
                var txt = go.GetComponentInChildren<Text>();
                if (txt != null) txt.text = $"{c.Name} ({c.Suit})\nP:{c.Points} R:{c.Rarity}";
            }
        }
    }

    private void OnCardViewClicked(CardViewGame view)
    {
        if (view == null) return;
        PlayCard(view.CardData);
    }

    #endregion
}
