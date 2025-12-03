using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.CreateDeck;       // DecksDto
using Assets.Scripts;                 // AuthContext
using UnityEngine.SceneManagement;
public class DeckListUI_Template : MonoBehaviour
{
    [Header("References")]
    public Transform content;               // AllDecks/ViewPort/Content
    public GameObject deckItemTemplate;     // DeckItemTemplate (in-scene, disabled)
    public Button confirmBtn;               // ConfirmBtn (hidden until selection)
    public Text statusText;                 // optional status text

    [Header("Inspector for testing only")]
    [Tooltip("Only used when not logged in (editor/testing). If user is logged-in, AuthContext.UserId is used).")]
    public long userId = 0;

    // runtime
    private DeckService deckService;
    private List<DecksDto> decks;
    private DeckItemUI selectedItemUI;
    private DecksDto selectedDeck;
    private long runtimeUserId;

    private void Awake()
    {
        // prefer the DeckService in Game.PreGame (which supports bearer token)
        deckService = new DeckService();

        if (confirmBtn != null)
            confirmBtn.gameObject.SetActive(false);

        if (deckItemTemplate == null)
            Debug.LogError("DeckItemTemplate not assigned.");
        else
            deckItemTemplate.SetActive(false); // ensure template disabled at runtime

        // decide which user id to use:
        if (AuthContext.IsLoggedIn)
        {
            runtimeUserId = AuthContext.UserId;
        }
        else
        {
            runtimeUserId = userId; // fallback (useful in editor or if not logged)
        }
    }

    private void Start()
    {
        StartCoroutine(LoadDecksCoroutine());
    }

    private IEnumerator LoadDecksCoroutine()
    {
        if (statusText != null) statusText.text = "Loading decks...";

        // call service with optional bearer token from AuthContext (null if not logged)
        var task = deckService.GetDecksAsync(runtimeUserId);

        while (!task.IsCompleted) yield return null;

        if (task.IsFaulted)
        {
            Debug.LogError("Error fetching decks: " + task.Exception);
            if (statusText != null) statusText.text = "Error loading decks.";
            yield break;
        }

        decks = task.Result;
        Populate(decks);
    }

    private void Populate(List<DecksDto> list)
    {
        // destroy previous clones but keep the template (if present)
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            var child = content.GetChild(i);
            if (deckItemTemplate != null && child.gameObject == deckItemTemplate) continue;
            Destroy(child.gameObject);
        }

        if (list == null || list.Count == 0)
        {
            if (statusText != null) statusText.text = "No decks found.";
            return;
        }

        if (statusText != null) statusText.text = "";

        foreach (var deck in list)
        {
            GameObject go;
            if (deckItemTemplate != null)
            {
                go = Instantiate(deckItemTemplate, content);
                go.SetActive(true);
            }
            else
            {
                Debug.LogWarning("No template; skipping deck " + deck.id);
                continue;
            }

            go.name = $"DeckItem_{deck.id}";
            var item = go.GetComponent<DeckItemUI>();
            if (item != null)
                item.Bind(deck, OnDeckClicked);
            else
                Debug.LogWarning("DeckItemTemplate missing DeckItemUI component.");
        }
    }

    private void OnDeckClicked(DecksDto deck, DeckItemUI itemUI)
    {
        // deselect previous
        if (selectedItemUI != null) selectedItemUI.SetSelected(false);

        selectedDeck = deck;
        selectedItemUI = itemUI;
        selectedItemUI.SetSelected(true);

        if (confirmBtn != null)
            confirmBtn.gameObject.SetActive(true);
    }

    // Hook this to ConfirmBtn.onClick in the inspector
    public void OnConfirmDeck()
    {
        if (selectedDeck == null) { Debug.LogWarning("No deck selected"); return; }
        Debug.Log($"Confirmed deck id={selectedDeck.id} name={selectedDeck.name}");
         SceneManager.LoadScene("Game2");
    }

    // Public method other systems can call when the user logs in/out to refresh the list
    public void Refresh()
    {
        // if logged in now, update runtimeUserId
        runtimeUserId = AuthContext.IsLoggedIn ? AuthContext.UserId : userId;
        StartCoroutine(LoadDecksCoroutine());
    }
}
