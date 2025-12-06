using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Required for Button

public class UIHandRenderer : MonoBehaviour
{
    [Header("Containers")]
    public Transform handContainer;
    public Transform middlePanel; // Make sure this is assigned in Inspector
    public GameObject cardPrefab;

    [Header("Global UI Elements")]
    public Button proposeBtn;           // Drag your Propose Button here
    public GameObject selectedCardPanel; // Drag your Middle/Selected Panel here

    private Player localPlayer;
    private List<CardViewGame> cardViews = new List<CardViewGame>();

    public void SetOwner(Player p)
{
    localPlayer = p;
    
    // 1. Link Visual Events (Hand)
    localPlayer.OnCardDrawn += AddCardToHand;
    localPlayer.OnCardRemoved += RemoveCardRenderer;

    Debug.Log("UIHandRenderer linked to " + p.gameObject.name);
    
    // 2. Hide UI initially
    if (proposeBtn != null) 
    {
        proposeBtn.gameObject.SetActive(false);
        
        // --- THE FIX: CONNECT THE BUTTON ---
        // Remove old listeners to prevent clicking for the wrong player/ghost clicks
        proposeBtn.onClick.RemoveAllListeners(); 
        
        // Add the new listener dynamically
        proposeBtn.onClick.AddListener(() => 
        {
            localPlayer.LockSelectedCard();
        });
    }

    if (selectedCardPanel != null) selectedCardPanel.SetActive(false);
}

    private void OnDestroy()
    {
        if (localPlayer != null)
        {
            localPlayer.OnCardDrawn -= AddCardToHand;
            localPlayer.OnCardRemoved -= RemoveCardRenderer;
        }
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

        // 1. Activate the panel (Fixing the missing reference issue)
        if (selectedCardPanel != null) selectedCardPanel.SetActive(true);

        // 2. Clear previous
        foreach (Transform child in middlePanel) Destroy(child.gameObject);

        // 3. Spawn
        GameObject obj = Instantiate(cardPrefab, middlePanel);
        var view = obj.GetComponent<CardViewGame>();
        view.Init(card, localPlayer);
    }

    // Helper to toggle button from Player.cs
    public void SetProposeButtonActive(bool isActive)
    {
        if (proposeBtn != null) proposeBtn.gameObject.SetActive(isActive);
    }
}