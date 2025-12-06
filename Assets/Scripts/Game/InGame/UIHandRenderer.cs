using System.Collections.Generic;
using UnityEngine;

public class UIHandRenderer : MonoBehaviour
{
    [Header("Containers")]
    public Transform handContainer;
    public Transform middlePanel;
    public GameObject cardPrefab;

    private Player localPlayer;
    private List<CardViewGame> cardViews = new List<CardViewGame>();

    // Removed OnEnable/OnDisable automatic subscriptions.
    
    // Called by Player.cs in OnNetworkSpawn (only for the owner)
    public void SetOwner(Player p)
    {
        localPlayer = p;
        
        // Subscribe to events
        localPlayer.OnCardDrawn += AddCardToHand;
        localPlayer.OnCardRemoved += RemoveCardRenderer;

        Debug.Log("UIHandRenderer linked to " + p.gameObject.name);
    }

    private void OnDestroy()
    {
        // Clean up events to prevent memory leaks
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
        
        // Init logic for your specific card prefab
        view.Init(card, localPlayer); 
    }

    private void RemoveCardRenderer(Card card)
    {
        CardViewGame viewToRemove = null;

        // Find the visual card that matches the data card
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

        // Clear previous middle card if any
        foreach (Transform child in middlePanel) Destroy(child.gameObject);

        GameObject obj = Instantiate(cardPrefab, middlePanel);
        var view = obj.GetComponent<CardViewGame>();
        view.Init(card, localPlayer);
    }
}