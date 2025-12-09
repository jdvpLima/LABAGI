using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.CreateDeck;
using Assets.Scripts;
using Unity.Netcode;
using TMPro;
using UnityEngine.SceneManagement;
using Assets.Scripts.Service;

public class DeckListUI : MonoBehaviour
{
    [Header("References")]
    public Transform content;
    public GameObject deckItemTemplate;

    [Header("Multiplayer Controls")]
    public Button hostBtn;
    public Button joinBtn;
    public Button viewDeck;

    // Runtime
    private DeckService deckService;
    private List<DecksDto> decks;
    private DeckItemUI selectedItemUI;
    private DecksDto selectedDeck;
    private MmrService mmrService;
    private long playerMMR;

    private void Awake()
    {
        deckService = new DeckService();
        mmrService = new MmrService();

        // Hide buttons until a deck is picked
        if (hostBtn != null) hostBtn.gameObject.SetActive(false);
        if (joinBtn != null) joinBtn.gameObject.SetActive(false);
        if (viewDeck != null) viewDeck.gameObject.SetActive(false);

        if (deckItemTemplate == null)
            Debug.LogError("DeckItemTemplate not assigned.");
        else
            deckItemTemplate.SetActive(false);

        // Setup Button Listeners
        if (hostBtn != null) hostBtn.onClick.AddListener(OnHostClicked);
        if (joinBtn != null) joinBtn.onClick.AddListener(OnJoinClicked);
        if (viewDeck != null) viewDeck.onClick.AddListener(OnViewDeckClicked);
    }

    private async void Start()
    {
        long idToUse = AuthBootstrapper.CurrentUserId;
        if (idToUse == 0)
        {
            Debug.Log("AuthContext is 0, using Debug ID 1");
            idToUse = 1;
        }

        // To be used to DDA
        playerMMR = await mmrService.GetPlayerMMRAsync(idToUse);
        Debug.Log("Player MMR is: " + playerMMR);

        decks = await deckService.GetDecksAsync(idToUse);

        // Clear old list
        foreach (Transform child in content)
        {
            if (child.gameObject != deckItemTemplate)
                Destroy(child.gameObject);
        }

        foreach (var deck in decks)
        {
            GameObject go = Instantiate(deckItemTemplate, content);
            go.SetActive(true);
            go.name = $"DeckItem_{deck.id}";

            var item = go.GetComponent<DeckItemUI>();
            if (item != null)
                item.Bind(deck, OnDeckSelected);
        }
    }

    private void OnDeckSelected(DecksDto deck, DeckItemUI itemUI)
    {
        if (selectedItemUI != null) selectedItemUI.SetSelected(false);

        selectedDeck = deck;
        selectedItemUI = itemUI;
        selectedItemUI.SetSelected(true);

        Debug.Log("Clicked deck id: " + deck.id);

        // Show the Host/Join/View buttons now that a deck is ready
        if (hostBtn != null) hostBtn.gameObject.SetActive(true);
        if (joinBtn != null) joinBtn.gameObject.SetActive(true);
        if (viewDeck != null) viewDeck.gameObject.SetActive(true);
    }

    // --- MULTIPLAYER LOGIC ---

    [SerializeField] private GameObject waitingRoomUI;

    public void OnHostClicked()
    {
        if (!ConfirmSelection()) return;

        var nm = NetworkManager.Singleton;
        if (nm == null)
        {
            Debug.LogError("No NetworkManager found in scene!");
            return;
        }

        if (nm.IsListening)
            nm.Shutdown();

        bool started = nm.StartHost();

        if (!started)
        {
            Debug.LogError("Failed to start Host.");
            return;
        }

        Debug.Log("Host started. Showing waiting room UI...");
        ShowWaitingRoom();
    }

    public void OnJoinClicked()
    {
        if (!ConfirmSelection()) return;

        var nm = NetworkManager.Singleton;
        if (nm == null)
        {
            Debug.LogError("No NetworkManager found in scene!");
            return;
        }

        if (nm.IsListening)
            nm.Shutdown();

        bool started = nm.StartClient();

        if (!started)
        {
            Debug.LogError("Failed to start Client.");
            return;
        }

        Debug.Log("Client started. Showing waiting room UI...");
        ShowWaitingRoom();
    }

    // --- Show the waiting room ---
    private void ShowWaitingRoom()
    {
        if (waitingRoomUI != null)
            waitingRoomUI.SetActive(true);
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

    public void OnBackButtonClicked()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void OnViewDeckClicked()
    {
        if (!ConfirmSelection())
        {
            Debug.LogWarning("OnViewDeckClicked: no deck selected.");
            return;
        }

        Debug.Log("Selected deck id (ViewDeck button): " + selectedDeck.id);

        SceneManager.LoadScene("CardViewer2");
    }
}
