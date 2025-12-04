using UnityEngine;
using UnityEngine.Video;

public class UIHandRenderer : MonoBehaviour
{
    public Player player;
    public Transform handContainer; // GridLayout ou Horizontal Layout
    public GameObject cardPrefab;

    private void OnEnable()
    {
        Debug.Log("UIHandRenderer ENABLED");
        player.OnCardDrawn += AddCardToHand;
    }

    private void OnDisable()
    {
        player.OnCardDrawn -= AddCardToHand;
    }

    private void AddCardToHand(Card card)
    {
        GameObject obj = Instantiate(cardPrefab, handContainer);
        var view = obj.GetComponent<CardViewGame>();
        view.Init(card, player);
    }
}
