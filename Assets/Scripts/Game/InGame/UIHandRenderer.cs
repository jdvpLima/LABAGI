using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIHandRenderer : MonoBehaviour
{
    [Header("Containers")]
    public Transform handContainer;
    public Transform middlePanel;
    public GameObject cardPrefab;
    public Button proposeBtn;
    public GameObject selectedCardPanel; 
    public CanvasGroup selectedPanelGroup; 

    [Header("Decision UI")]
    public Button acceptBtn;
    public Button refuseBtn;

    [Header("Suit Reveal UI")]
    public GameObject showSuitsPanel;
    public TextMeshProUGUI suitTxt;


    [Header("Stats UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI burnoutText;
    public TextMeshProUGUI flexibilityText;
    public TextMeshProUGUI opponentScoreText;

    private Player localPlayer;
    private List<CardViewGame> cardViews = new List<CardViewGame>();

    public void SetOwner(Player p)
    {
        localPlayer = p;
        
        // Clear trash
        foreach(Transform child in handContainer) Destroy(child.gameObject);
        cardViews.Clear();

        // 1. Link Hand Events
        localPlayer.OnCardDrawn += AddCardToHand;
        localPlayer.OnCardRemoved += RemoveCardRenderer;

        // 2. Link Stats Events (This fixes the missing points update)
        localPlayer.OnPointsChanged += UpdateScoreUI;
        localPlayer.OnBurnoutChanged += UpdateBurnoutUI;
        localPlayer.OnFlexibilityChanged += UpdateFlexibilityUI;

        // Initialize Stats immediately
        UpdateScoreUI(localPlayer.Points.Value);
        UpdateBurnoutUI(localPlayer.Burnout.Value);
        UpdateFlexibilityUI(localPlayer.Flexibility.Value);

        if (proposeBtn != null) 
        {
            proposeBtn.gameObject.SetActive(false);
            proposeBtn.onClick.RemoveAllListeners();
            proposeBtn.onClick.AddListener(() => localPlayer.LockSelectedCard());
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

    public void SetOpponent(Player p)
    {
        // When the opponent's score changes, update the specific Opponent Text
        p.OnPointsChanged += (val) => 
        {
            if (opponentScoreText != null) opponentScoreText.text = val.ToString();
        };

        // Initialize immediately
        if (opponentScoreText != null) 
            opponentScoreText.text = p.Points.Value.ToString();
    }

    private void OnDestroy()
    {
        if (localPlayer != null)
        {
            localPlayer.OnCardDrawn -= AddCardToHand;
            localPlayer.OnCardRemoved -= RemoveCardRenderer;
            localPlayer.OnPointsChanged -= UpdateScoreUI;
            localPlayer.OnBurnoutChanged -= UpdateBurnoutUI;
            localPlayer.OnFlexibilityChanged -= UpdateFlexibilityUI;
        }
    }

    
    private void UpdateScoreUI(int value) 
    { 
        if(scoreText != null) scoreText.text = value.ToString(); 
    }
    private void UpdateBurnoutUI(int value) 
    { 
        if(burnoutText != null) burnoutText.text = value.ToString(); 
    }
    private void UpdateFlexibilityUI(int value) 
    { 
        if(flexibilityText != null) flexibilityText.text = value.ToString(); 
    }
    
    private void AddCardToHand(Card card)
    {
        if (cardPrefab == null || handContainer == null) return;
        GameObject obj = Instantiate(cardPrefab, handContainer);
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

    public void DisplayCardInMiddle(Card card)
    {
        if (middlePanel == null) return;
        ShowPopup();
        foreach (Transform child in middlePanel) Destroy(child.gameObject);
        
        GameObject obj = Instantiate(cardPrefab, middlePanel);
        var view = obj.GetComponent<CardViewGame>();
        view.Init(card, localPlayer);
        
        RectTransform rect = obj.GetComponent<RectTransform>();
        if (rect != null) rect.anchoredPosition = Vector2.zero; 
    }

    public void DisplayOpponentSuit(string suit)
    {
        if (showSuitsPanel != null) 
        {
            showSuitsPanel.SetActive(true);
            
            // This forces the panel to draw ON TOP of the video player
            showSuitsPanel.transform.SetAsLastSibling(); 
        }
        
        if (suitTxt != null) suitTxt.text = "Opponent Suit:\n" + suit;
    }

    public void ToggleDecisionUI(bool isActive)
    {
        if (isActive) ShowPopup();
        if (acceptBtn != null) acceptBtn.gameObject.SetActive(isActive);
        if (refuseBtn != null) refuseBtn.gameObject.SetActive(isActive);
        
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