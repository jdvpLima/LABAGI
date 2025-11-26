using UnityEngine;

public class DeckManager : MonoBehaviour {
   public static DeckManager Instance { get; private set; }
    public Assets.Scripts.CreateDeck.DecksDto SelectedDeckDto { get; set; }

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
