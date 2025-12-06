using Assets.Scripts.Service;
using Assets.Scripts.CreateDeck; 
using Assets.Scripts.Model; 
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode; 
using UnityEngine;
using UnityEngine.UI;

public class Player : NetworkBehaviour
{
    [Header("Networked Stats")]
    public NetworkVariable<int> Points = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> Burnout = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> Flexibility = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> AccommodationTokens = new NetworkVariable<int>(2, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private long userId;

    [Header("Local Game Data")]
    public List<Card> Deck { get; private set; } = new List<Card>();
    private List<Card> Hand { get; set; } = new List<Card>();

    // REMOVED: public Button proposeBtn; 
    // REMOVED: public GameObject selectedCardPanel;

    public Card selectedCard = null;
    public CardViewGame selectedCardUI = null;
    public bool IsReady { get; private set; } = false;
    
    private UIHandRenderer uIHandRenderer;
    private CardService cardService;
    public bool tokenUsed = false;

    // Events
    public event Action<int> OnPointsChanged;
    public event Action<int> OnBurnoutChanged;
    public event Action<int> OnFlexibilityChanged;
    public event Action<int> OnTokensChanged;
    public event Action<Card> OnCardDrawn;
    public event Action<Card> OnCardRemoved;

    private void Awake()
    {
        cardService = new CardService();
    }

    public override void OnNetworkSpawn()
    {
        Points.OnValueChanged += (oldVal, newVal) => OnPointsChanged?.Invoke(newVal);
        Burnout.OnValueChanged += (oldVal, newVal) => OnBurnoutChanged?.Invoke(newVal);
        Flexibility.OnValueChanged += (oldVal, newVal) => OnFlexibilityChanged?.Invoke(newVal);
        AccommodationTokens.OnValueChanged += (oldVal, newVal) => OnTokensChanged?.Invoke(newVal);

        if (IsServer) GameManager.Instance.RegisterPlayer(this);

        if (IsOwner)
        {
            if (typeof(Assets.Scripts.AuthContext).GetField("UserId") != null)
                 userId = Assets.Scripts.AuthContext.UserId; 
            if (userId == 0) userId = 1; 

            // Find the UI Manager in the scene
            uIHandRenderer = FindObjectOfType<UIHandRenderer>();
            
            if (uIHandRenderer != null)
            {
                 uIHandRenderer.SetOwner(this);
            }
            else
            {
                 Debug.LogError("CRITICAL: UIHandRenderer not found in scene!");
            }

            _ = InitializeDeckAsync();
        }
    }

    private async Task InitializeDeckAsync()
    {
        var selectedDeckDto = SelectedDeckHolder.SelectedDeck;

        // Debug Log: Check which User ID we are actually using
        Debug.Log($"[Player] Initializing Deck for User ID: {userId}");

        var fullLibrary = await cardService.GetPlayerCardCollectionAsync(userId);
        
        // Debug Log: Check what the API actually returned
        if (fullLibrary != null)
        {
            Debug.Log($"[Player] Library loaded. Count: {fullLibrary.Count}. First Card ID: {(fullLibrary.Count > 0 ? fullLibrary[0].cardId : -1)}");
        }
        else
        {
            Debug.LogError("[Player] CRITICAL: Library returned NULL!");
        }

        Deck.Clear();

        if (selectedDeckDto != null && selectedDeckDto.cards != null)
        {
            Debug.Log($"Loading Selected Deck: {selectedDeckDto.name}");

            // Ensure library isn't null to prevent crash
            if (fullLibrary == null) fullLibrary = new List<CardDto>();

            foreach (var deckItem in selectedDeckDto.cards)
            {
                // Try to find the card in the library
                var cardDto = fullLibrary.Find(c => c.cardId == deckItem.cardId);

                if (cardDto != null)
                {
                    // CASE A: Card Found (Normal)
                    for (int i = 0; i < deckItem.qty; i++)
                    {
                        Deck.Add(Card.FromDto(cardDto));
                    }
                }
                else
                {
                    // CASE B: Card Missing 
                    // Add a "Placeholder" card so the game works.
                    Debug.LogError($"[Player] DATA MISMATCH: Card ID {deckItem.cardId} is in the Deck but NOT in the User's Library.");
                    
                    for (int i = 0; i < deckItem.qty; i++)
                    {
                        // Create a temporary "Error Card"
                        Card errorCard = new Card(
                            deckItem.cardId, 
                            $"MISSING {deckItem.cardId}", 
                            "System", 
                            0, 
                            "Common", 
                            "Database Error: Card not found in library"
                        );
                        Deck.Add(errorCard);
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("No Deck Selected. Loading Debug Deck.");
            InitializeDebugDeck();
        }
		if (Deck.Count < 5)
        {
            Debug.LogWarning($"[Player {OwnerClientId}] Deck is too small ({Deck.Count}). Filling with 5 Backup Cards.");
            while (Deck.Count < 5)
            {
                // Add a simple filler card
                Deck.Add(new Card(999, "Backup Card", "Analytical", 1, "Common", "Auto-generated to fill hand"));
            }
        }

        ShuffleDeck();
        InitializeHand(); // Now this will actually have cards to draw!
    }

    private void InitializeDebugDeck()
    {
        for (int i = 0; i < 20; i++)
        {
            // Debug deck still works because we updated the Card constructor
            Deck.Add(new Card(i, $"Debug Card {i}", "Analytical", 5, "Common"));
        }
    }

    private void ShuffleDeck()
    {
        for (int i = 0; i < Deck.Count; i++)
        {
            Card temp = Deck[i];
            int randomIndex = UnityEngine.Random.Range(i, Deck.Count);
            Deck[i] = Deck[randomIndex];
            Deck[randomIndex] = temp;
        }
    }

    private void InitializeHand()
    {
        Debug.Log($"[Player {OwnerClientId}] Drawing Initial Hand. Deck Size: {Deck.Count}");
        
        for (int i = 0; i < 5; i++) 
        {
            var card = DrawCard();
            if (card == null)
            {
                Debug.LogError($"[Player {OwnerClientId}] Could not draw card #{i+1}. The Deck is empty!");
            }
        }
    }

    public Card DrawCard()
    {
        if (Deck.Count == 0) return null;
        Card card = Deck[0];
        Deck.RemoveAt(0);
        Hand.Add(card);
        OnCardDrawn?.Invoke(card);
        return card;
    }

    public void PickCard(CardViewGame cardUI)
    {
        if (!IsOwner) return;

        selectedCardUI = cardUI;
        selectedCard = cardUI.card;
        
        // Use the UI Manager to show the button
        if(uIHandRenderer != null) uIHandRenderer.SetProposeButtonActive(true);
    }

    public void LockSelectedCard()
{
    if (!IsOwner) return;
    if (selectedCard == null) return;

    // --- SAFETY CHECK ---
    // Ensure the UI Manager exists before destroying the card from hand
    if(uIHandRenderer == null)
    {
        Debug.LogError("Cannot lock card: UIHandRenderer is missing!");
        return;
    }

    // 1. Remove from Data Hand
    if(Hand.Contains(selectedCard))
    {
        Hand.Remove(selectedCard);
        OnCardRemoved?.Invoke(selectedCard); // Removes visual from hand
    }

    // 2. Show in Middle (Visuals)
    uIHandRenderer.DisplayCardInMiddle(selectedCard);
    
    // 3. Hide the button since we just clicked it
    uIHandRenderer.SetProposeButtonActive(false);

    // 4. Send Data to Server
    SubmitCardServerRpc(selectedCard.CardId, selectedCard.Points, selectedCard.Suit);
}

    public void UseToken()
    {
        if (!IsOwner) return;
        RequestTokenUseServerRpc();
    }

    [ServerRpc]
    private void SubmitCardServerRpc(long cardId, int points, string suit)
    {
        Debug.Log($"Player {OwnerClientId} submitted card {cardId}");
        IsReady = true;
        GameManager.Instance.PlayerSubmittedCard(OwnerClientId, cardId, points, suit);
    }

    [ServerRpc]
    private void RequestTokenUseServerRpc()
    {
        if (AccommodationTokens.Value > 0)
        {
            AccommodationTokens.Value -= 1;
            tokenUsed = true;
        }
    }

    public void ServerApplyRoundResult(int pointsToAdd, int burnoutToAdd)
    {
        if (!IsServer) return;
        Points.Value += pointsToAdd;
        Burnout.Value += burnoutToAdd;
        IsReady = false;
        tokenUsed = false;
    }
}