using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// Handles all visual elements for the Player.
/// Spawns cards, updates Score Text, handles Buttons, and manages Panels.
public class UIHandRenderer : MonoBehaviour
{
    [Header("Containers")]
    public Transform handContainer;
    public Transform middlePanel;
    public GameObject cardPrefab;
    public Button proposeBtn;
    
    // The panel that holds the card + buttons
    public GameObject selectedCardPanel; 
    public CanvasGroup selectedPanelGroup; 

    [Header("Decision UI")]
    public Button acceptBtn;
    public Button refuseBtn;
    public GameObject decisionPanel; 

    [Header("Suit Reveal UI")]
    public GameObject showSuitsPanel;
    public TextMeshProUGUI suitTxt;

    [Header("Stats UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI burnoutText;
    public TextMeshProUGUI flexibilityText;
    public TextMeshProUGUI opponentScoreText;

    [Header("Token UI")]
    public Button tokenBtn;
    public TextMeshProUGUI tokenText;

    private Player localPlayer;
    private List<CardViewGame> cardViews = new List<CardViewGame>();

    /// Called when the Local Player spawns. Connects Logic to UI.
    public void SetOwner(Player p)
    {
        localPlayer = p;
        
        // Clean up any editor artifacts or previous session trash
        if (handContainer != null)
        {
            foreach(Transform child in handContainer) Destroy(child.gameObject);
        }
        cardViews.Clear();

        // 1. Link Events (Draw Card, Remove Card)
        localPlayer.OnCardDrawn += AddCardToHand;
        localPlayer.OnCardRemoved += RemoveCardRenderer;
        
        // 2. Link Stats Updates
        localPlayer.OnPointsChanged += UpdateScoreUI;
        localPlayer.OnBurnoutChanged += UpdateBurnoutUI;
        localPlayer.OnFlexibilityChanged += UpdateFlexibilityUI;
        localPlayer.OnTokensChanged += UpdateTokenUI;

        // Initialize Text values
        UpdateScoreUI(localPlayer.Points.Value);
        UpdateBurnoutUI(localPlayer.Burnout.Value);
        UpdateFlexibilityUI(localPlayer.Flexibility.Value);
        UpdateTokenUI(localPlayer.AccommodationTokens.Value);

        // 3. Setup Buttons
        if (proposeBtn != null) 
        {
            proposeBtn.gameObject.SetActive(false);
            proposeBtn.onClick.RemoveAllListeners();
            proposeBtn.onClick.AddListener(() => localPlayer.LockSelectedCard());
        }

        if (tokenBtn != null)
        {
            tokenBtn.onClick.RemoveAllListeners();
            tokenBtn.onClick.AddListener(() => localPlayer.UseToken());
        }

        if (acceptBtn != null && refuseBtn != null)
        {
            ToggleDecisionUI(false); 
            acceptBtn.onClick.RemoveAllListeners();
            acceptBtn.onClick.AddListener(() => localPlayer.SubmitDecision(true));
            refuseBtn.onClick.RemoveAllListeners();
            refuseBtn.onClick.AddListener(() => localPlayer.SubmitDecision(false));
        }

        if (showSuitsPanel != null) showSuitsPanel.SetActive(false);
    }

    /// <summary>
    /// Called when the Opponent spawns. Connects only their score to the UI.
    /// </summary>
    public void SetOpponent(Player p)
    {
        p.OnPointsChanged += (val) => { if (opponentScoreText != null) opponentScoreText.text = val.ToString(); };
        if (opponentScoreText != null) opponentScoreText.text = p.Points.Value.ToString();
    }

    private void AddCardToHand(Card card)
    {
        if (cardPrefab == null || handContainer == null) return;

        GameObject obj = Instantiate(cardPrefab, handContainer);
        
        // Ensure visibility (Scale fix)
        obj.transform.localScale = Vector3.one; 
        
        var view = obj.GetComponent<CardViewGame>();
        cardViews.Add(view);
        view.Init(card, localPlayer);
    }

    private void RemoveCardRenderer(Card card)
    {
        CardViewGame viewToRemove = null;
        foreach (var view in cardViews)
        {
            if (view.card.CardId == card.CardId) 
            {
                viewToRemove = view;
                break;
            }
        }
        if (viewToRemove != null)
        {
            cardViews.Remove(viewToRemove);
            Destroy(viewToRemove.gameObject);
        }
    }

    // --- UI UPDATE HELPERS ---
    private void UpdateScoreUI(int value) { if(scoreText != null) scoreText.text = value.ToString(); }
    private void UpdateBurnoutUI(int value) { if(burnoutText != null) burnoutText.text = value.ToString(); }
    private void UpdateFlexibilityUI(int value) { if(flexibilityText != null) flexibilityText.text = value.ToString(); }
    
    private void UpdateTokenUI(int value) 
    { 
        if(tokenText != null) tokenText.text = value.ToString(); 
        // Grey out button if no tokens
        if(tokenBtn != null) tokenBtn.interactable = (value > 0);
    }

    public void DisplayCardInMiddle(Card card)
    {
        if (middlePanel == null) return;
        
        // Show the panel (restoring scale/active state)
        ShowPopup();

        foreach (Transform child in middlePanel) Destroy(child.gameObject);
        GameObject obj = Instantiate(cardPrefab, middlePanel);
        var view = obj.GetComponent<CardViewGame>();
        view.Init(card, localPlayer);
        
        // Center the card
        RectTransform rect = obj.GetComponent<RectTransform>();
        if (rect != null) rect.anchoredPosition = Vector2.zero; 
    }

    public void DisplayOpponentSuit(string suit)
    {
        if (showSuitsPanel != null) 
        {
            showSuitsPanel.SetActive(true);
            showSuitsPanel.transform.SetAsLastSibling(); 
        }
        if (suitTxt != null) suitTxt.text = "Opponent Suit:\n" + suit;
    }

    public void ToggleDecisionUI(bool isActive)
    {
        if (isActive) ShowPopup();

        if (decisionPanel != null) decisionPanel.SetActive(isActive);
        else
        {
            if (acceptBtn != null) acceptBtn.gameObject.SetActive(isActive);
            if (refuseBtn != null) refuseBtn.gameObject.SetActive(isActive);
        }
    }

    public void ClearMiddleCards()
    {
        if (middlePanel == null) return;
        foreach (Transform child in middlePanel) Destroy(child.gameObject);
        
        if (showSuitsPanel != null) showSuitsPanel.SetActive(false);
        HidePopup();
    }

    public void SetProposeButtonActive(bool isActive)
    {
        if (proposeBtn != null) proposeBtn.gameObject.SetActive(isActive);
    }

    // --- VISIBILITY HELPERS (ATTEMPT TO FIX VIDEO PLAYER) ---
    // Instead of Deactivating objects (which causes the Video to flicker/disappear),
    // we manage Scale or Canvas Groups.

    private void ShowPopup()
    {
        if (selectedPanelGroup != null)
        {
            selectedPanelGroup.alpha = 1;
            selectedPanelGroup.interactable = true;
            selectedPanelGroup.blocksRaycasts = true;
        }
        else if (selectedCardPanel != null)
        {
            selectedCardPanel.SetActive(true);
            selectedCardPanel.transform.localScale = Vector3.one; 
        }
    }

    private void HidePopup()
    {
        if (selectedPanelGroup != null)
        {
            selectedPanelGroup.alpha = 0;
            selectedPanelGroup.interactable = false;
            selectedPanelGroup.blocksRaycasts = false;
        }
        else if (selectedCardPanel != null)
        {
            selectedCardPanel.transform.localScale = Vector3.zero;
        }
    }
}