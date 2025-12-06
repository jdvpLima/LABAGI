using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.CreateDeck;
using Assets.Scripts; 
using Unity.Netcode; // IMPORT NETCODE
using TMPro;

public class DeckListUI : MonoBehaviour
{
    [Header("References")]
    public Transform content;
    public GameObject deckItemTemplate;
    
    // CHANGED: We now have two buttons instead of just "Confirm"
    [Header("Multiplayer Controls")]
    public Button hostBtn;
    public Button joinBtn;
    
    public Text statusText; // Use standard Text or TextMeshProUGUI based on your setup

    // Runtime
    private DeckService deckService;
    private List<DecksDto> decks;
    private DeckItemUI selectedItemUI;
    private DecksDto selectedDeck;

    private void Awake()
    {
        deckService = new DeckService();

        // Hide buttons until a deck is picked
        if (hostBtn != null) hostBtn.gameObject.SetActive(false);
        if (joinBtn != null) joinBtn.gameObject.SetActive(false);

        if (deckItemTemplate == null)
            Debug.LogError("DeckItemTemplate not assigned.");
        else
            deckItemTemplate.SetActive(false);

        // Setup Button Listeners
        if (hostBtn != null) hostBtn.onClick.AddListener(OnHostClicked);
        if (joinBtn != null) joinBtn.onClick.AddListener(OnJoinClicked);
    }

    private async void Start()
{
    // 1. Get ID (Fallback to a debug ID if AuthContext is 0 in Editor)
    long idToUse = AuthContext.UserId;
    if (idToUse == 0) 
    {
        Debug.Log("AuthContext is 0, using Debug ID 1");
        idToUse = 1; // CHANGE THIS to a valid ID from your database for testing
    }

    // 2. Call the CORRECT method name (GetDecksAsync)
    decks = await deckService.GetDecksAsync(idToUse); 

    // 3. Clear old list
    foreach (Transform child in content)
    {
        if (child.gameObject != deckItemTemplate)
            Destroy(child.gameObject);
    }

    // 4. Populate List
    if (decks == null || decks.Count == 0)
    {
        if (statusText != null) statusText.text = "No decks found.";
        Debug.LogWarning("Deck list is empty or null.");
        return;
    }

    foreach (var deck in decks)
    {
        GameObject go = Instantiate(deckItemTemplate, content);
        go.SetActive(true);
        go.name = $"DeckItem_{deck.id}";
        
        var item = go.GetComponent<DeckItemUI>();
        if (item != null)
            item.Bind(deck, OnDeckClicked);
    }
}

    private void OnDeckClicked(DecksDto deck, DeckItemUI itemUI)
    {
        if (selectedItemUI != null) selectedItemUI.SetSelected(false);

        selectedDeck = deck;
        selectedItemUI = itemUI;
        selectedItemUI.SetSelected(true);

        // Show the Host/Join buttons now that a deck is ready
        if (hostBtn != null) hostBtn.gameObject.SetActive(true);
        if (joinBtn != null) joinBtn.gameObject.SetActive(true);
    }

    // --- MULTIPLAYER LOGIC ---

    public void OnHostClicked()
    {
        if (!ConfirmSelection()) return;

        // 1. Start the Host (Server + Player)
        bool started = NetworkManager.Singleton.StartHost();
        
        if (started)
        {
            Debug.Log("Host Started! Loading Game Scene...");
            // 2. Load Scene using NetworkSceneManager
            // This tells all connected clients to switch scenes automatically
            NetworkManager.Singleton.SceneManager.LoadScene("Game", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
        else
        {
            Debug.LogError("Failed to start Host.");
        }
    }

    public void OnJoinClicked()
    {
        if (!ConfirmSelection()) return;

        // 1. Start Client
        // Note: For localhost, this connects to 127.0.0.1 immediately.
        // For real online play, you'd need an InputField to type the Host's IP.
        bool started = NetworkManager.Singleton.StartClient();
        
        if (started)
        {
            Debug.Log("Client Started! Waiting for Host to switch scenes...");
            if (statusText != null) statusText.text = "Connecting...";
        }
        else
        {
            Debug.LogError("Failed to start Client.");
        }
    }

    private bool ConfirmSelection()
    {
        if (selectedDeck == null) 
        { 
            Debug.LogWarning("No deck selected"); 
            return false; 
        }

        // Store deck for the next scene
        SelectedDeckHolder.SelectedDeck = selectedDeck;
        return true;
    }
}