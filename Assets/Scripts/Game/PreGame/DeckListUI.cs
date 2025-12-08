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

    public Text statusText;

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

        // Populate List
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
            TextMeshProUGUI text = go.GetComponentInChildren<TextMeshProUGUI>();
            text.text = deck.name;

            var item = go.GetComponent<DeckItemUI>();
            if (item != null)
                item.Bind(deck, OnDeckSelected); // čia pririšam callback su konkrečiu deck
        }
    }

    /// <summary>
    /// Callback iš DeckItemUI – čia ateina būtent tas deck, ant kurio buvo paspausta.
    /// </summary>
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

    public void OnHostClicked()
    {
        if (!ConfirmSelection()) return;

        bool started = NetworkManager.Singleton.StartHost();

        if (started)
        {
            Debug.Log("Host Started! Loading Game Scene...");
            NetworkManager.Singleton.SceneManager.LoadScene("Game", LoadSceneMode.Single);
        }
        else
        {
            Debug.LogError("Failed to start Host.");
        }
    }

    public void OnJoinClicked()
    {
        if (!ConfirmSelection()) return;

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

    public void OnBackButtonClicked()
    {
        if (ARModeSwitcher.Instance != null && ARModeSwitcher.ar_active)
        {
            SceneManager.UnloadSceneAsync("PreGame");
        }
        else
            SceneManager.LoadScene("MainMenu");

    }

    /// <summary>
    /// Mygtukas "View Deck" – naudoja jau pasirinktą deck ir permeta jį į CardViewer sceną.
    /// </summary>
    public void OnViewDeckClicked()
    {
        // Čia ir turi būti užpildytas SelectedDeckHolder
        if (!ConfirmSelection())
        {
            Debug.LogWarning("OnViewDeckClicked: no deck selected.");
            return;
        }

        Debug.Log("Selected deck id (ViewDeck button): " + selectedDeck.id);

        
        if (ARModeSwitcher.Instance != null && ARModeSwitcher.ar_active)
        {
            FindAnyObjectByType<ARSceneLoader>().LoadSceneAtPosition("CardViewer2"); 
        }
        else
            // Atidarom sceną, kur veikia CardViewer
            SceneManager.LoadScene("CardViewer2");
    }
}
