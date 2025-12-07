using Assets.Scripts.Service;
using Assets.Scripts.CreateDeck; 
using Assets.Scripts.Model; 
using System;
using System.Collections;
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

        if (IsServer) StartCoroutine(WaitForGameManagerAndRegister());

        if (IsOwner)
        {
            if (typeof(Assets.Scripts.AuthContext).GetField("UserId") != null)
                 userId = Assets.Scripts.AuthContext.UserId; 
            if (userId == 0) userId = 1; 

            _ = InitializeDeckAsync();
        }else
        {
            // --- I AM THE OPPONENT (NEW) ---
            // If IsOwner is false, this object represents the other player.
            // We just need to tell the UI to listen to this object's score.
            StartCoroutine(InitOpponentStats());
        }
    }
	private IEnumerator InitOpponentStats()
    {
        // Wait until UI is ready (safeguard)
        while (uIHandRenderer == null)
        {
            uIHandRenderer = FindObjectOfType<UIHandRenderer>();
            yield return null;
        }

        if (uIHandRenderer != null)
        {
            Debug.Log($"[Player] Linking Opponent (ID: {OwnerClientId}) to UI Stats.");
            uIHandRenderer.SetOpponent(this);
        }
    }

    private IEnumerator WaitForGameManagerAndRegister()
    {
        while (GameManager.Instance == null) yield return null;
        GameManager.Instance.RegisterPlayer(this);
    }

    private async Task InitializeDeckAsync()
    {
        // Small delay to ensure Scene is loaded
        await Task.Delay(500);

        if (uIHandRenderer == null)
        {
            uIHandRenderer = FindObjectOfType<UIHandRenderer>();
            if (uIHandRenderer != null) uIHandRenderer.SetOwner(this);
        }

        var selectedDeckDto = SelectedDeckHolder.SelectedDeck;
        var fullLibrary = await cardService.GetPlayerCardCollectionAsync(userId);
        
        Deck.Clear();
        if (fullLibrary == null) fullLibrary = new List<CardDto>();

        if (selectedDeckDto != null && selectedDeckDto.cards != null)
        {
            foreach (var deckItem in selectedDeckDto.cards)
            {
                var cardDto = fullLibrary.Find(c => c.cardId == deckItem.cardId);

                if (cardDto != null)
                {
                    for (int i = 0; i < deckItem.qty; i++)
                        Deck.Add(Card.FromDto(cardDto));
                }
                else
                {
                    // --- THE STABLE FALLBACK ---
                    // This ensures cards exist even if database fails
                    for (int i = 0; i < deckItem.qty; i++)
                    {
                        Deck.Add(new Card(
                            deckItem.cardId, 
                            $"Missing {deckItem.cardId}", 
                            "Analytical", // Default Suit so game logic works
                            5,            // Default Points
                            "Common", 
                            "Database Error"
                        ));
                    }
                }
            }
        }
        else
        {
            // If no deck selected, load debug cards
            for(int i=0; i<20; i++) 
                Deck.Add(new Card(i, $"Debug {i}", "Analytical", 5));
        }

        // Filler to prevent "Empty Hand" bug if deck is small
        while (Deck.Count < 5)
        {
             Deck.Add(new Card(999, "Filler", "Social", 1));
        }

        ShuffleDeck();
        InitializeHand();
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
        if(uIHandRenderer != null) uIHandRenderer.SetProposeButtonActive(true);
    }

    public void LockSelectedCard()
    {
        if (!IsOwner) return;
        if (selectedCard == null) return;
        if (uIHandRenderer == null) return;

        if(Hand.Contains(selectedCard))
        {
            Hand.Remove(selectedCard);
            OnCardRemoved?.Invoke(selectedCard);
        }

        uIHandRenderer.DisplayCardInMiddle(selectedCard);
        uIHandRenderer.SetProposeButtonActive(false);

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

    // Called by GameManager to apply results
    public void ServerApplyRoundResult(int pointsToAdd, int burnoutToAdd, int flexToAdd)
    {
        if (!IsServer) return;

        Points.Value += pointsToAdd;
        
        // Apply Burnout Change (Ensure it doesn't go below 0 if that's a rule, otherwise just add)
        Burnout.Value += burnoutToAdd;
        if (Burnout.Value < 0) Burnout.Value = 0; // Optional safety clamp

        // Apply Flexibility Change
        Flexibility.Value += flexToAdd;
        // if (Flexibility.Value < 0) Flexibility.Value = 0; // Optional safety clamp

        IsReady = false;
        tokenUsed = false;
    }
	public void SubmitDecision(bool accepted)
    {
        // Hide buttons immediately so they can't spam click
        if (uIHandRenderer != null) uIHandRenderer.ToggleDecisionUI(false);

        // Tell Server
        SubmitDecisionServerRpc(accepted);
    }

    [ServerRpc]
    private void SubmitDecisionServerRpc(bool accepted)
    {
        Debug.Log($"Player {OwnerClientId} decision: {(accepted ? "Accept" : "Refuse")}");
        GameManager.Instance.PlayerMadeDecision(OwnerClientId, accepted);
    }

    // Called by GameManager when it's time to decide
    [ClientRpc]
    public void EnableDecisionPhaseClientRpc()
    {
        // Only show buttons for the owner of this player object
        if (IsOwner && uIHandRenderer != null)
        {
            uIHandRenderer.ToggleDecisionUI(true);
        }
    }

    // Called by GameManager when round is fully over
    [ClientRpc]
    public void CleanupRoundClientRpc()
    {
        if (IsOwner && uIHandRenderer != null)
        {
            uIHandRenderer.ClearMiddleCards();
            uIHandRenderer.ToggleDecisionUI(false); // Safety hide
        }
        
        // Reset state for next turn
        IsReady = false;
        tokenUsed = false;
    }
	[ClientRpc]
    public void RevealOpponentCardClientRpc(long cardId, string suit, int points)
    {
        if (!IsOwner) return;
        
        if (uIHandRenderer != null)
        {
            uIHandRenderer.DisplayOpponentSuit(suit);
        }
    }
}