using Assets.Scripts.Service;
using Assets.Scripts.CreateDeck; 
using Assets.Scripts.Model; // Needed for CardDto
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode; 
using UnityEngine;
using UnityEngine.UI;

public class Player : NetworkBehaviour
{
    [Header("Networked Stats (Synced)")]
    public NetworkVariable<int> Points = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> Burnout = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> Flexibility = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> AccommodationTokens = new NetworkVariable<int>(2, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // This stores the logged-in user's ID
    private long userId;

    [Header("Local Game Data")]
    public List<Card> Deck { get; private set; } = new List<Card>();
    private List<Card> Hand { get; set; } = new List<Card>();

    [Header("UI References")]
    public Button proposeBtn;
    public GameObject selectedCardPanel;
    public GameObject showSuitsPanel;
    public TextMeshProUGUI suitTxt;

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
        // 1. Hook up UI Listeners
        Points.OnValueChanged += (oldVal, newVal) => OnPointsChanged?.Invoke(newVal);
        Burnout.OnValueChanged += (oldVal, newVal) => OnBurnoutChanged?.Invoke(newVal);
        Flexibility.OnValueChanged += (oldVal, newVal) => OnFlexibilityChanged?.Invoke(newVal);
        AccommodationTokens.OnValueChanged += (oldVal, newVal) => OnTokensChanged?.Invoke(newVal);

        // 2. Register with Game Manager (Server side)
        if (IsServer)
        {
            GameManager.Instance.RegisterPlayer(this);
        }

        // 3. Client Side Initialization
        if (IsOwner)
        {
            // Get User ID safely
            if (typeof(Assets.Scripts.AuthContext).GetField("UserId") != null)
                 userId = Assets.Scripts.AuthContext.UserId; 
            
            // Fallback for testing
            if (userId == 0) userId = 1; 

            // Find UI
            uIHandRenderer = FindObjectOfType<UIHandRenderer>();
            if (uIHandRenderer != null) uIHandRenderer.SetOwner(this);
            
            if (proposeBtn != null) proposeBtn.gameObject.SetActive(false);

            // Load Deck
            _ = InitializeDeckAsync();
        }
    }

    private async Task InitializeDeckAsync()
    {
        var selectedDeckDto = SelectedDeckHolder.SelectedDeck;

        // Fetch full card data (List<CardDto>) from API
        var fullLibrary = await cardService.GetPlayerCardCollectionAsync(userId);
        
        Deck.Clear();

        if (selectedDeckDto != null && selectedDeckDto.cards != null && fullLibrary != null)
        {
            Debug.Log($"Loading Selected Deck: {selectedDeckDto.name}");

            foreach (var deckItem in selectedDeckDto.cards)
            {
                // FIX: Use 'cardId' (lowercase) to match the DTO definition
                var cardDto = fullLibrary.Find(c => c.cardId == deckItem.cardId);

                if (cardDto != null)
                {
                    for (int i = 0; i < deckItem.qty; i++)
                    {
                        // FIX: Use the Factory method!
                        // This handles the DTO -> Card conversion and parses abilities automatically
                        Deck.Add(Card.FromDto(cardDto));
                    }
                }
                else
                {
                    Debug.LogWarning($"Card ID {deckItem.cardId} in deck but not found in library!");
                }
            }
        }
        else
        {
            Debug.LogWarning("No Deck Selected or Library empty. Loading Debug Deck.");
            InitializeDebugDeck();
        }

        ShuffleDeck();
        InitializeHand();
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
        for (int i = 0; i < 5; i++) DrawCard();
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
        
        if(proposeBtn != null) proposeBtn.gameObject.SetActive(true);
    }

    public void LockSelectedCard()
    {
        if (!IsOwner) return;
        if (selectedCard == null) return;

        if(Hand.Contains(selectedCard))
        {
            Hand.Remove(selectedCard);
            OnCardRemoved?.Invoke(selectedCard);
        }

        if(selectedCardPanel != null) selectedCardPanel.SetActive(true);
        uIHandRenderer.DisplayCardInMiddle(selectedCard);
        
        if(proposeBtn != null) proposeBtn.gameObject.SetActive(false);

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