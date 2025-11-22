using Assets.Scripts.CreateDeck;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Timeline.Actions.MenuPriority;

public class CreateDeckManager : MonoBehaviour
{
    private AuthApiClient apiClient;
    private DeckService _deckService = new DeckService();
    public GameObject cardPrefab;

    public Transform cardCollectionView;

    public Transform cardOfDeckView;

    public TextMeshProUGUI deckName;

    [SerializeField]
    private int totalCards = 10;
    public TextMeshProUGUI totalCardUi;

    private int currentCards = 0;
    public TextMeshProUGUI currentCardsUi;

    private bool deckFull = false;


    private const string tokenKey = "sessionToken";

    private long userID = 7;

    //id=7


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        totalCardUi.text = totalCards.ToString();

        DecksDto novoDeck = new DecksDto
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

        //var token = PlayerPrefs.GetString(tokenKey);
        //apiClient.GetMe(token, onSuccess: me => userID = me.id,onUnauthorized: null, onError: err => { Debug.LogError("ERROOOO"); } );

        List<DecksDto> decks = await _deckService.GetDecksAsync(userID);

        if (decks != null)
        {
            foreach (var deck in decks)
            {
                Debug.Log("Deck: " + deck.name + " ID: " + deck.id);
            }
        }

        //foreach(DeckCards card  in decks[0].cards) // usar o primeiro deck de cada utilizador como o que guarda toda a coleção de cartas obtidas
        foreach (DeckCards card in novoDeck.cards)
        {
            SpawnCard(card.cardId, cardCollectionView, cardOfDeckView);
        }
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

    }

    void SpawnCard(int id, Transform parentUI, Transform secondParentUI)
    {
        GameObject card = Instantiate(cardPrefab, parentUI);
        card.GetComponent<CardView>().Initialize(id,parentUI,secondParentUI);
    }

    public void CreateDeck()
    {
        Debug.Log("creating deck with name: " + deckName.text);
        //createDeck(deckName.text, GetItems()); --- Descomentar quando estiver tudo integrado
        
    }


    public List<DeckCards> GetItems()
    {
        Dictionary<int, int> counter = new Dictionary<int, int>();

        // percorre todas as children
        foreach (Transform child in cardOfDeckView)
        {
            var item = child.GetComponent<CardView>(); // script com id

            if (item == null)
                continue;

            int id = item.cardId;

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

}
