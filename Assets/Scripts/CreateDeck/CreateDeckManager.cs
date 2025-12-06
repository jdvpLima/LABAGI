using Assets.Scripts.CreateDeck;
using Assets.Scripts.Model;
using Assets.Scripts.Service;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CreateDeckManager : MonoBehaviour
{
    public GameObject endPanel;

    private DeckService _deckService = new DeckService();
    private CardService _cardService = new CardService();

    public GameObject cardPrefab;

    public Transform cardCollectionView;

    public Transform cardOfDeckView;

    public TMP_InputField deckName;

    [SerializeField]
    private int totalCards = 10;
    public TextMeshProUGUI totalCardUi;

    private int currentCards = 0;
    public TextMeshProUGUI currentCardsUi;

    private bool deckFull = false;


    private long userID;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        verifyEventSystem();
        totalCardUi.text = totalCards.ToString();
        endPanel.SetActive(false);
        

        userID = AuthBootstrapper.CurrentUserId != 0 ? AuthBootstrapper.CurrentUserId : 12 ;

        List<CardDto> collection = await _cardService.GetPlayerCardCollectionAsync(userID);

        if (collection != null)
        {
            Debug.Log("NUM DIF CARDS OF COLLECTION: " + collection.Count);
            
        }

        foreach (CardDto card in collection) {
            for (int i = 0; i < card.quantity; i++)
            {
                SpawnCard(card.cardId, cardCollectionView, cardOfDeckView);
            }

        }

        /*
        List<DecksDto> decks = await _deckService.GetDecksAsync(userID);

        if (decks != null)
        {
            foreach (var deck in decks)
            {
                Debug.Log("Deck: " + deck.name + " ID: " + deck.id);
            }
        }

        foreach(DeckCards card  in decks[0].cards) // usar o primeiro deck de cada utilizador como o que guarda toda a cole��o de cartas obtidas
        {
            SpawnCard(card.cardId, cardCollectionView, cardOfDeckView);
        }*/
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCurrentCards();
        if (currentCards >= totalCards)
        {
            deckFull = true;
            SetButtonsActive(false);
        }
        else if (currentCards < totalCards && deckFull == false)
        {
            SetButtonsActive(true);
            deckFull = false;
        }
        else { deckFull = false; }

        
    }

    private async void createDeck(string name, List<DeckCards> cards)
    {

        DecksDto novoDeck = new DecksDto
        {
            name = name,
            cards = cards
        };

        bool enviado = await _deckService.PostDeckAsync(userID, novoDeck);
        
        Debug.Log(enviado ? "Deck enviado com sucesso." : "Falha ao enviar deck.");
        if (enviado) { 
            endPanel.SetActive(true);
        
        }


    }

    void SpawnCard(long id, Transform parentUI, Transform secondParentUI)
    {
        GameObject card = Instantiate(cardPrefab, parentUI);
        card.GetComponent<CardView>().Initialize(id,parentUI,secondParentUI);
    }

    public void CreateDeck()
    {
        List<DeckCards> cards = GetItems();
        if (!string.IsNullOrEmpty(deckName.text) && cards.Count > 0)
        {
            Debug.Log("creating deck with name: " + deckName.text);
            createDeck(deckName.text, GetItems()); // Descomentar quando estiver tudo integrado
        }
    }


    public List<DeckCards> GetItems()
    {
        Dictionary<long, int> counter = new Dictionary<long, int>();

        // percorre todas as children
        foreach (Transform child in cardOfDeckView)
        {
            var item = child.GetComponent<CardView>(); // script com id

            if (item == null)
                continue;

            long id = item.cardId;

            if (counter.ContainsKey(id))
                counter[id]++;
            else
                counter[id] = 1;
        }

        // converte o dictionary para lista de ItemInfo
        List<DeckCards> result = new List<DeckCards>();
        foreach (var kv in counter)
        {
            result.Add(new DeckCards { cardId = kv.Key, qty = kv.Value });
        }

        return result;
    }

    private void UpdateCurrentCards()
    {
        int total = 0;
        foreach (var card in GetItems()) {
            total = total + card.qty;
        }

        currentCards = total;
        currentCardsUi.text = currentCards.ToString();
    }

    public void SetButtonsActive(bool active)
    {
        // percorre todos os filhos
        foreach (Transform child in cardCollectionView)
        {
            // tenta obter o componente Button
            Button btn = child.GetComponent<Button>();

            if (btn != null)
                btn.interactable = active; // ativa/desativa
        }
    }

    private DecksDto defaultDeck()
    {
        return new DecksDto
        {
            name = "Collection",
            cards = new List<DeckCards>
            {
                new DeckCards { cardId = 1, qty = 1 },
                new DeckCards { cardId = 2, qty = 1 },
                new DeckCards { cardId = 3, qty = 1 },
                new DeckCards { cardId = 4, qty = 1 },
                new DeckCards { cardId = 5, qty = 1 },
                new DeckCards { cardId = 6, qty = 1 },
                new DeckCards { cardId = 7, qty = 1 },
                new DeckCards { cardId = 8, qty = 1 },
                new DeckCards { cardId = 9, qty = 1 },
                new DeckCards { cardId = 10, qty = 1 }
            }
        };
    }

    public void OkButton()
    {
        endPanel.SetActive(false);
        BackButton();

    }

    public void BackButton()
    {
        Debug.Log("BACK2BACK2BACK");
        SceneManager.UnloadSceneAsync("DeckCreation");
    }



    private void verifyEventSystem()
    {
        EventSystem[] eventSystems = FindObjectsByType<EventSystem>(FindObjectsSortMode.InstanceID);
        AudioListener[] audioListeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.InstanceID);

        // Conta quantas cenas est�o atualmente carregadas
        int loadedScenes = SceneManager.sceneCount;

        bool isAdditive = loadedScenes > 1;

        if (isAdditive)
        {
            // Desativa o �ltimo EventSystem encontrado (o da cena loaded)
            if (eventSystems.Length > 0)
                eventSystems[eventSystems.Length - 1].gameObject.SetActive(false);

            // Desativa o �ltimo AudioListener encontrado (o da cena loaded)
            if (audioListeners.Length > 0)
                audioListeners[audioListeners.Length - 1].enabled = false;
        }
        else
        {
            // Cena carregada sozinha -> mant�m ativos os primeiros (ou �nicos)
            if (eventSystems.Length > 0)
                eventSystems[0].gameObject.SetActive(true);

            if (audioListeners.Length > 0)
                audioListeners[0].enabled = true;
        }
    }

}
