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

/// Manages the individual Player entity. 
/// Handles Networking (Stats), Deck Data, and interaction with the Server.
public class Player : NetworkBehaviour
{
	// NetworkVariables are authoritative on the Server but readable by Everyone.
    // This ensures both players see updated scores instantly.
    [Header("Networked Stats (Synced)")]
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
    
    // Reference to the UI Manager in the scene
    private UIHandRenderer uIHandRenderer;
    private CardService cardService;
    public bool tokenUsed = false;

    // Events to update UI when NetworkVariables change
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

    /// Called by Netcode when this object spawns.
    /// This is where we determine if "I am the Player" or "I am the Opponent".
    public override void OnNetworkSpawn()
    {
        // 1. Subscribe to Network Variables (Runs on both Owner and Opponent)
        Points.OnValueChanged += (oldVal, newVal) => OnPointsChanged?.Invoke(newVal);
        Burnout.OnValueChanged += (oldVal, newVal) => OnBurnoutChanged?.Invoke(newVal);
        Flexibility.OnValueChanged += (oldVal, newVal) => OnFlexibilityChanged?.Invoke(newVal);
        AccommodationTokens.OnValueChanged += (oldVal, newVal) => OnTokensChanged?.Invoke(newVal);

        // 2. Start initialization routine
        StartCoroutine(InitializeWhenInGameScene());
    }

    /// Waits for the Scene to fully switch to "Game" before initializing logic.
    /// Prevents the "Double Hand" bug where logic runs in the PreGame lobby.
    private IEnumerator InitializeWhenInGameScene()
    {
        // A. Wait for Scene Switch
        while (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Game") 
        {
            yield return null; 
        }

        // B. Server Side: Register this player with the Referee (GameManager)
        if (IsServer)
        {
            while (GameManager.Instance == null) yield return null;
            GameManager.Instance.RegisterPlayer(this);
        }

        // Find the UI Manager
        if (uIHandRenderer == null) uIHandRenderer = FindObjectOfType<UIHandRenderer>();

        // C. Client Side: Split Logic
        if (IsOwner)
        {
            // --- I AM THE LOCAL PLAYER ---
            // Load User ID and Deck
            if (typeof(Assets.Scripts.AuthContext).GetField("UserId") != null)
                 userId = Assets.Scripts.AuthContext.UserId; 
            if (userId == 0) userId = 1; 

            _ = InitializeDeckAsync();
        }
        else
        {
            // --- I AM THE OPPONENT ---
            // Just link the stats so the UI shows the enemy score
            if (uIHandRenderer != null)
            {
                uIHandRenderer.SetOpponent(this);
            }
        }
    }

    /// Loads cards from the API or creates a fallback deck if the DB is empty.
    private async Task InitializeDeckAsync()
    {
        await Task.Delay(500); // Safety delay for UI

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
                    // Fallback: Create dummy cards if database ID is missing
                    for (int i = 0; i < deckItem.qty; i++)
                    {
                        Deck.Add(new Card(
                            deckItem.cardId, 
                            $"Missing {deckItem.cardId}", 
                            "Analytical", 
                            5,            
                            "Common", 
                            "Database Error"
                        ));
                    }
                }
            }
        }
        else
        {
            // Debug Deck for testing without API
            for(int i=0; i<20; i++) 
                Deck.Add(new Card(i, $"Debug {i}", "Analytical", 5));
        }

        // Ensure hand is never empty
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

    /// <summary>
    /// UI Event: Called when clicking a card in hand.
    /// </summary>
    public void PickCard(CardViewGame cardUI)
    {
        if (!IsOwner) return;
        selectedCardUI = cardUI;
        selectedCard = cardUI.card;
        if(uIHandRenderer != null) uIHandRenderer.SetProposeButtonActive(true);
    }

    /// <summary>
    /// UI Event: Locks the card, moves it to the middle, and sends data to Server.
    /// </summary>
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

    // --- TOKEN LOGIC ---

    public void UseToken()
    {
        if (!IsOwner) return;
        RequestTokenUseServerRpc();
    }

    [ServerRpc]
    private void RequestTokenUseServerRpc()
    {
        if (AccommodationTokens.Value > 0)
        {
            AccommodationTokens.Value -= 1;
            
            // Mechanic: Reset Flexibility immediately
            Flexibility.Value = 0; 
            tokenUsed = true;

            // 1. Trigger Visuals (Draw Card)
            DrawExtraCardClientRpc();

            // 2. Apply Penalty (Give opponent point via GameManager)
            GameManager.Instance.PlayerUsedToken(OwnerClientId);
        }
    }

    [ClientRpc]
    private void DrawExtraCardClientRpc()
    {
        if (!IsOwner) return;
        Debug.Log("[Player] Token Used: Drawing Extra Card...");

        // Safety: Prevent crash if deck is empty
        if (Deck.Count == 0)
        {
            Deck.Add(new Card(999, "Emergency Card", "Analytical", 1, "Common", "Drawn via Token"));
        }

        DrawCard();
    }

    // --- GAME LOOP LOGIC ---

    [ServerRpc]
    private void SubmitCardServerRpc(long cardId, int points, string suit)
    {
        Debug.Log($"Player {OwnerClientId} submitted card {cardId}");
        IsReady = true;
        GameManager.Instance.PlayerSubmittedCard(OwnerClientId, cardId, points, suit);
    }

    
    /// Core Math Logic for applying game rules. 
    /// Returns true if Flexibility broke limits (-3), signaling a penalty point.
    public bool ServerApplyRoundResult(int pointsToAdd, int burnoutToAdd, int flexToAdd)
    {
        if (!IsServer) return false;

        bool opponentGainsPoint = false;

        // 1. Apply Points
        Points.Value += pointsToAdd;

        // 2. Flexibility Logic
        int currentFlex = Flexibility.Value + flexToAdd;

        // Rule: Flex >= 3 -> Gain Burnout
        if (currentFlex >= 3)
        {
            currentFlex = 3; 
            burnoutToAdd += 1; 
        }
        // Rule: Flex <= -3 -> Forced Token Usage
        else if (currentFlex <= -3)
        {
            if (AccommodationTokens.Value > 0)
            {
                AccommodationTokens.Value -= 1; 
                currentFlex = 0; // Reset
                opponentGainsPoint = true; // Signal penalty
                DrawExtraCardClientRpc();
            }
            else
            {
                currentFlex = -3; // Cap at -3 if no tokens
            }
        }
        Flexibility.Value = currentFlex;

        // 3. Burnout Logic (Clamped -3 to 3)
        int currentBurnout = Burnout.Value + burnoutToAdd;
        if (currentBurnout > 3) currentBurnout = 3;
        if (currentBurnout < -3) currentBurnout = -3;
        Burnout.Value = currentBurnout;

        IsReady = false;
        tokenUsed = false;

        return opponentGainsPoint;
    }

    // --- DECISION PHASE RPCs ---

    public void SubmitDecision(bool accepted)
    {
        if (uIHandRenderer != null) uIHandRenderer.ToggleDecisionUI(false);
        SubmitDecisionServerRpc(accepted);
    }

    [ServerRpc]
    private void SubmitDecisionServerRpc(bool accepted)
    {
        GameManager.Instance.PlayerMadeDecision(OwnerClientId, accepted);
    }

    [ClientRpc]
    public void EnableDecisionPhaseClientRpc()
    {
        if (IsOwner && uIHandRenderer != null) uIHandRenderer.ToggleDecisionUI(true);
    }

    [ClientRpc]
    public void CleanupRoundClientRpc()
    {
        if (IsOwner && uIHandRenderer != null)
        {
            uIHandRenderer.ClearMiddleCards();
            uIHandRenderer.ToggleDecisionUI(false); 
        }
        IsReady = false;
        tokenUsed = false;
    }

    [ClientRpc]
    public void RevealOpponentCardClientRpc(long cardId, string suit, int points)
    {
        if (!IsOwner) return;
        if (uIHandRenderer != null) uIHandRenderer.DisplayOpponentSuit(suit);
    }
}