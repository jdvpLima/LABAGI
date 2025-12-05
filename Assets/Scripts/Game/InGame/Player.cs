using Assets.Scripts.Service;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
	[Header("Player Attributes")]
	public int Points { get; private set; } = 0;
	public int Burnout { get; private set; } = 0;
	public int Flexibility { get; private set; } = 0;
	public int AccommodationTokens { get; private set; } = 2;

	[Header("Deck Array")]
	public List<Card> Deck { get; private set; } = new List<Card>();

	private List<Card> Hand { get; set; } = new List<Card>();

	[Header("Opponent")]
	public Player Opponent; //Assing in Inspector or dynamically

	public bool tokenUsed = false;

	public Card selectedCard = null;

    public CardViewGame selectedCardUI = null;

	public Button proposeBtn;

    public bool selectedDecision = false, actionExecuted = false;

    public GameObject selectedCardPanel, showSuitsPanel;

	public TextMeshProUGUI suitTxt;


    private CardService cardService;
	private UIHandRenderer uIHandRenderer;
    public long userId;

    // Events
    public event Action<int> OnBurnoutChanged;
	public event Action<int> OnFlexibilityChanged;
	public event Action<int> OnPointsChanged;
	public event Action<int> OnTokensChanged;
	public event Action<Card> OnCardDrawn;
	public event Action<Card> OnCardAdded;
	public event Action<Card> OnCardRemoved;

    private void Awake()
    {
        cardService = new CardService(); // construtor normal, sem MonoBehaviour
		uIHandRenderer = FindFirstObjectByType<UIHandRenderer>();
    }


    private async void Start()
    {
        try
        {
            await LoadDeckFromAPI();
            InitializeHand();
        }
        catch (Exception ex)
        {
            Debug.LogError("[Player] Failed to load deck: " + ex);
            // fallback: inicializa um deck vazio para evitar bloqueios na UI
            InitializeDeck();
            InitializeHand();
        }

		proposeBtn.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!actionExecuted && selectedDecision == true)
        {
            actionExecuted = true;     // garante que só corre uma vez
			ShowSuit();
        }
    }

    private void InitializeDeck()
	{
		// Fill the deck with 20 placeholder cards
		for (int i = 0; i < 20; i++)
		{
			Card newCard = new Card($"Card {i + 1}", "Analytical");

			Deck.Add(newCard);
		}
	}

	private void InitializeHand()
	{
		// Fill the hand with 5 cards
		for (int i = 0; i < 5; i++)
		{
			DrawCard();
		}
	}

    // Player Attributes API
    public void AddBurnout(int amount)
	{
		Burnout += amount;
		OnBurnoutChanged?.Invoke(Burnout);
	}

	public void AddFlexibility(int amount)
	{
		Flexibility += amount;
		OnFlexibilityChanged?.Invoke(Flexibility);
	}

	public void AddPoints(int amount)
	{
		Points += amount;
		OnPointsChanged?.Invoke(Points);
	}

	public void AddTokens(int amount)
	{
		AccommodationTokens += amount;
		OnTokensChanged?.Invoke(AccommodationTokens);
	}

	public void UseToken()
	{
		AddTokens(-1);
		tokenUsed = true;
	}

	// Deck API
	public Card DrawCard()
	{
		if (Deck.Count == 0)
			return null;

		Card card = Deck[0];
		Deck.RemoveAt(0);

		Hand.Add(card);

		OnCardDrawn?.Invoke(card);
		return card;
	}


	public void PickCard(CardViewGame cardUI)
	{
        selectedCardUI = cardUI;
        selectedCard = cardUI.card;

        proposeBtn.gameObject.SetActive(true);
    }

    public void LockSelectedCard()
    {
        if (selectedCard == null) return;

        selectedDecision = true;

		Debug.Log("Locked card: " + selectedCard.Name);

		DiscardCard(selectedCard);

        selectedCardPanel.SetActive(true);

        uIHandRenderer.DisplayCardInMiddle(selectedCard);

    }

	public void ShowSuit()
	{
		if (selectedDecision){
            showSuitsPanel.SetActive(true);
			suitTxt.text = selectedCard.Suit;
        }
	}


    public void DiscardCard(Card card)
	{
		if (Hand.Remove(card))
			OnCardRemoved?.Invoke(card);
	}


	public void DiscardHand()
	{
		foreach (var c in Hand.ToArray())
		{
			DiscardCard(c);
		}

	}


	// Game progress


	// TODO: make this function actually work
	public void newRoundStart()
	{
		checkWinConditions();
		tokenUsed = false;
		selectedCard = null;
		selectedDecision = false;
		actionExecuted = false;
        showSuitsPanel.SetActive(false);

        DrawCard();
		/*await ???*/
		StartDebateTimer();

		// TODO: add remaining logic here

		DebateResult();
		checkWinConditions();
		// TODO
		//	- Check flexibility/burnout and apply consequences
		//	- restart new round
	}

	public void checkWinConditions()
	{
		//TODO
	}

	public /*async ???*/ void StartDebateTimer()
	{
		//TODO
	}




	public void DebateResult()
	{
		Card playerCard = selectedCard;
		Card opponentCard = Opponent.selectedCard;
		bool playerDecision = selectedDecision;
		bool opponentDecision = Opponent.selectedDecision;

		TriggerCardsActions(playerCard, opponentCard, playerDecision, opponentDecision);

		// If player accepts
		if (playerDecision)
		{
			AddBurnout(-1);
			AddFlexibility(1);

			// If both accept
			if (opponentDecision)
			{
				AddPoints(playerCard.Points);
				CheckAndApplySynergy(playerCard, opponentCard);
			}
		}
		else // if player refuses
		{
			AddBurnout(1);
			AddFlexibility(-1);
		}
	}

	public void TriggerCardsActions(Card playerCard, Card opponentCard, bool playerDecision, bool opponentDecision)
	{
		// Action example:

		//    BothAccept?AddBurnout&AddFlexibility

		for (int i = 0; i < playerCard.Actions.Count; i++)
		{
			bool actionWillTrigger = true;
			string[] splitAction = playerCard.Actions[i].Split('?');

			string condition = splitAction[0];
			string result = splitAction[1];

			string[] splitconditions = condition.Split('&');

			for (int j = 0; j < splitconditions.Length; j++)
			{
				// Possible Conditions
				switch (splitconditions[j])
				{
					// Decision Results
					case "BothAccept":
						actionWillTrigger &= playerDecision && opponentDecision; break;
					case "BothRefuse":
						actionWillTrigger &= !playerDecision && !opponentDecision; break;
					case "PlayerAccepts":
						actionWillTrigger &= playerDecision; break;
					case "PlayerRejects":
						actionWillTrigger &= !playerDecision; break;

					// SUITS
					case "EqualSuits":
						actionWillTrigger &= playerCard.Suit == opponentCard.Suit; break;
					case "DiffSuits":
						actionWillTrigger &= playerCard.Suit != opponentCard.Suit; break;

					// Cards Points Comparison
					case "CardPointsEQOpp": actionWillTrigger &= playerCard.Points == opponentCard.Points; break;
					case "CardPointsGTOpp": actionWillTrigger &= playerCard.Points > opponentCard.Points; break;
					case "CardPointsLTOpp": actionWillTrigger &= playerCard.Points < opponentCard.Points; break;

					// Player Burnout Comparison
					case "BurnoutEQOpp": actionWillTrigger &= Burnout == Opponent.Burnout; break;
					case "BurnoutGTOpp": actionWillTrigger &= Burnout > Opponent.Burnout; break;
					case "BurnoutLTOpp": actionWillTrigger &= Burnout < Opponent.Burnout; break;

					// Player Flexibility Comparison
					case "FlexibilityEQOpp": actionWillTrigger &= Flexibility == Opponent.Flexibility; break;
					case "FlexibilityGTOpp": actionWillTrigger &= Flexibility > Opponent.Flexibility; break;
					case "FlexibilityLTOpp": actionWillTrigger &= Flexibility < Opponent.Flexibility; break;

					// Player Points Comparison
					case "PlayerPointsEQOpp": actionWillTrigger &= Points == Opponent.Points; break;
					case "PlayerPointsGTOpp": actionWillTrigger &= Points > Opponent.Points; break;
					case "PlayerPointsLTOpp": actionWillTrigger &= Points < Opponent.Points; break;

					// If done this round
					case "TokenUsed": actionWillTrigger &= tokenUsed; break;
					case "OppTokenUsed": actionWillTrigger &= Opponent.tokenUsed; break;
				}
			}

			if (actionWillTrigger)
			{
				string[] splitresults = result.Split('&');

				for (int j = 0; j < splitresults.Length; j++)
				{
					// Possible Results
					switch (splitresults[j])
					{
						// Burnout
						case "AddBurnout":
							AddBurnout(1); break;
						case "RemoveBurnout":
							AddBurnout(-1); break;

						// Flexibility
						case "AddFlexibility":
							AddFlexibility(1); break;
						case "RemoveFlexibility":
							AddFlexibility(-1); break;

						// Points
						case "AddPoint":
							AddPoints(1); break;
						case "RemovePoint":
							AddPoints(-1); break;

						// Cards
						case "DrawCard": DrawCard(); break;
						//case "DiscardCard": DiscardCard(PickCard());	break;

							// TODO: Other actions
					}
				}
			}
		}
	}

	public void CheckAndApplySynergy(Card playerCard, Card opponentCard)
	{
		if (playerCard.Suit == opponentCard.Suit && Burnout < 3)
		{
			AddPoints(1);
		}
	}

    #region Card communication with backend

    private async Task LoadDeckFromAPI()
    {
        var cardDtos = await cardService.GetPlayerCardCollectionAsync(userId);

        Deck.Clear();

        if (cardDtos == null || cardDtos.Count == 0)
        {
            Debug.Log("No cards returned from API.");
            return;
        }

        foreach (var dto in cardDtos)
        {
            var runtimeCard = Card.FromDto(dto);
            Deck.Add(runtimeCard);
            OnCardAdded?.Invoke(runtimeCard);
        }
    }

    #endregion
}
