using UnityEngine;
using Assets.Scripts.CreateDeck;

public class GameReceiveDeck : MonoBehaviour
{
    void Start()
    {
        var deck = SelectedDeckHolder.SelectedDeck;
        if (deck != null)
        {
            Debug.Log($"Received deck id={deck.id} name={deck.name}");
            // TODO: use deck to initialize UI, gameplay, etc.
        }
        else
        {
            Debug.LogWarning("No deck found in SelectedDeckHolder (selectedDeck may not have been set).");
        }
    }
}
