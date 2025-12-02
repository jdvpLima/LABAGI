using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GameCard : MonoBehaviour
{
    public void OnSelectEntered(SelectEnterEventArgs args)
    {
        Debug.Log("Card selected: " + name);
        // Aqui metes a lógica da jogada
    }
}

