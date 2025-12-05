using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class UIHandRenderer : MonoBehaviour
{
    public Player player;
    public Transform handContainer; // GridLayout ou Horizontal Layout
    public Transform middlePanel;
    public GameObject cardPrefab;

    // list of rendered cards
    private List<CardViewGame> cardViews = new List<CardViewGame>();

    private void OnEnable()
    {
        Debug.Log("UIHandRenderer ENABLED");
        player.OnCardDrawn += AddCardToHand;
        player.OnCardRemoved += RemoveCardRenderer;
    }

    private void OnDisable()
    {
        player.OnCardDrawn -= AddCardToHand;
    }

    private void AddCardToHand(Card card)
    {
        GameObject obj = Instantiate(cardPrefab, handContainer);
        var view = obj.GetComponent<CardViewGame>();
        cardViews.Add(view);
        view.Init(card, player);
    }

    private void RemoveCardRenderer(Card card)
    {
        Debug.Log("RemoveCardRenderer was called!");
        // encontra o CardView correspondente à carta removida
        CardViewGame viewToRemove = null;

        foreach (var view in cardViews)
        {
            if (view.card.CardId == card.CardId)   // comparar referência da Card
            {
                viewToRemove = view;
                Debug.Log("Card to remove is " + viewToRemove);
                break;
            }
            else
            {
                Debug.Log("No card with corresponding ID");

            }
        }

        if (viewToRemove != null)
        {
            cardViews.Remove(viewToRemove);
            Destroy(viewToRemove.gameObject);
        }
        else
        {
            Debug.LogWarning($"Could not find card renderer for card id {card.CardId}");
        }
    }


    public void DisplayCardInMiddle(Card card)
    {
        GameObject obj = Instantiate(cardPrefab, middlePanel);
        var view = obj.GetComponent<CardViewGame>();
        view.Init(card, player);
    }
}
