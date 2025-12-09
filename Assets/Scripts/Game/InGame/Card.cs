using Assets.Scripts.Model;
using Assets.Scripts.Workshop;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Card
{
	public long CardId { get; private set; }
	public string Name { get; private set; } = "";
	public string Suit { get; private set; } = "";
	public int Points { get; private set; } = 0;
	public string Rarity { get; private set; } = "";
	public string FlavourText { get; private set; } = "";

	// Initialize here to be safe
	public AbilityJsonPayload Action { get; private set; }

	public Card(long id, string name, string suit, int points = 0, string rarity = "", string flavourText = null)
	{
		CardId = id;
		Name = name;
		Suit = suit;
		Points = points;
		Rarity = rarity;
		if (flavourText != null) FlavourText = flavourText;		
	}

	//public Card(long id, string suit, int points, List<string> actions)
	//{
	//	CardId = id;
	//	Suit = suit;
	//	Points = points;
	//	Actions = actions;
	//}

	public static Card FromDto(CardDto dto)
	{
		string flavour = dto.flavorText ?? "";
		string cardName = dto.name ?? $"Card {dto.cardId}";
		string cardSuit = dto.suit ?? "";
		string cardRarity = dto.rarity ?? "";

		var card = new Card(dto.cardId, cardName, cardSuit, dto.points, cardRarity, flavour);

		// -----------------------------
		// NEW JSON ACTION PARSING
		// -----------------------------
		if (!string.IsNullOrEmpty(dto.abilityJson))
		{
			try
			{
				card.Action = JsonConvert.DeserializeObject<AbilityJsonPayload>(dto.abilityJson);
				if (card.Action != null)
					return card;
			}
			catch
			{
				Debug.LogWarning("Failed to parse abilityJson for card " + dto.cardId);
			}
		}

		// ----------------------------------------
		// FALLBACK (legacy fields converted to one action)
		// ----------------------------------------

		// Check if any legacy fields exist
		if (!string.IsNullOrEmpty(dto.effect) ||
			!string.IsNullOrEmpty(dto.trigger) ||
			!string.IsNullOrEmpty(dto.ability))
		{
			card.Action = new AbilityJsonPayload
			{
				trigger = dto.trigger,
				effect = dto.effect,
				amount = dto.amount,
				target = dto.target,
				oncePerGame = dto.oncePerGame
			};

			return card;
		}

		// ----------------------------------------
		// NO ACTION -> assign empty payload
		// ----------------------------------------
		card.Action = new AbilityJsonPayload
		{
			trigger = "",
			effect = "",
			amount = 0,
			target = "",
			oncePerGame = false
		};




		//if (!string.IsNullOrEmpty(dto.abilityJson))
		//{
		//	try
		//	{
		//		var actions = JsonConvert.DeserializeObject<List<string>>(dto.abilityJson);
		//		if (actions != null) card.Actions.AddRange(actions);
		//		else card.Actions.Add(dto.abilityJson);
		//	}
		//	catch
		//	{
		//		card.Actions.Add(dto.abilityJson);
		//	}
		//}
		//else
		//{
		//	if (!string.IsNullOrEmpty(dto.effect)) card.Actions.Add(dto.effect);
		//	if (!string.IsNullOrEmpty(dto.ability)) card.Actions.Add(dto.ability);
		//	if (!string.IsNullOrEmpty(dto.trigger)) card.Actions.Add(dto.trigger);
		//}

		return card;
	}
}