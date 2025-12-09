using System;
using System.Collections.Generic;
using UnityEngine;

public static class CardLogic
{
	// Track used once-per-game abilities
	private static HashSet<string> usedOncePerGame = new();

	public static void ExecuteCardActions(
		Player currentPlayer,
		Player opponentPlayer,
		Card currentCard,
		Card opponentCard,
		bool currentAccepted,
		bool opponentAccepted,
		 (int Points, int Burnout, int Flex, int Tokens) currentPlayerGains,
		 (int Points, int Burnout, int Flex, int Tokens) opponentGains
	)
	{
		var action = currentCard.Action;
		if (action == null) return;

		// ----------------------------------------
		// ONCE PER GAME CHECK
		// ----------------------------------------
		if (action.oncePerGame)
		{
			string key = $"{currentPlayer.GetInstanceID()}_{currentCard.CardId}";

			if (usedOncePerGame.Contains(key))
			{
				Debug.Log($"Ability for card {currentCard.CardId} already used once per game. Skipping.");
				return;
			}

			usedOncePerGame.Add(key);
		}

		// ----------------------------------------
		// CHECK TRIGGER CONDITIONS
		// ----------------------------------------
		if (!CheckTrigger(action.trigger, currentAccepted, opponentAccepted))
			return;

		// ----------------------------------------
		// APPLY EFFECT
		// ----------------------------------------
		ApplyEffect(
			currentPlayer,
			opponentPlayer,
			action.effect,
			action.target,
			action.amount,
			ref currentPlayerGains,
			ref opponentGains
		);
	}

	private static bool CheckTrigger(string trigger, bool accepted, bool oppAccepted)
	{
		// OLD Triggers
		//switch (cond)
		//{
		//	// Decision logic
		//	case "BothAccept": trigger &= currentAccepted && opponentAccepted; break;
		//	case "BothRefuse": trigger &= !currentAccepted && !opponentAccepted; break;
		//	case "PlayerAccepts": trigger &= currentAccepted; break;
		//	case "PlayerRejects": trigger &= !currentAccepted; break;
		//	// Suits
		//	case "EqualSuits": trigger &= currentCard.Suit == opponentCard.Suit; break;
		//	case "DiffSuits": trigger &= currentCard.Suit != opponentCard.Suit; break;
		//	// Card point comparisons
		//	case "CardPointsEQOpp": trigger &= currentCard.Points == opponentCard.Points; break;
		//	case "CardPointsGTOpp": trigger &= currentCard.Points > opponentCard.Points; break;
		//	case "CardPointsLTOpp": trigger &= currentCard.Points < opponentCard.Points; break;
		//	// Burnout
		//	case "BurnoutEQOpp": trigger &= currentPlayer.Burnout.Value == opponentPlayer.Burnout.Value; break;
		//	case "BurnoutGTOpp": trigger &= currentPlayer.Burnout.Value > opponentPlayer.Burnout.Value; break;
		//	case "BurnoutLTOpp": trigger &= currentPlayer.Burnout.Value < opponentPlayer.Burnout.Value; break;
		//	// Flexibility
		//	case "FlexibilityEQOpp": trigger &= currentPlayer.Flexibility.Value == opponentPlayer.Flexibility.Value; break;
		//	case "FlexibilityGTOpp": trigger &= currentPlayer.Flexibility.Value > opponentPlayer.Flexibility.Value; break;
		//	case "FlexibilityLTOpp": trigger &= currentPlayer.Flexibility.Value < opponentPlayer.Flexibility.Value; break;
		//	// Points
		//	case "PlayerPointsEQOpp": trigger &= currentPlayer.Points.Value == opponentPlayer.Points.Value; break;
		//	case "PlayerPointsGTOpp": trigger &= currentPlayer.Points.Value > opponentPlayer.Points.Value; break;
		//	case "PlayerPointsLTOpp": trigger &= currentPlayer.Points.Value < opponentPlayer.Points.Value; break;
		//	// Tokens
		//	case "TokenUsed": trigger &= currentPlayer.tokenUsed; break;
		//	case "OppTokenUsed": trigger &= opponentPlayer.tokenUsed; break;
		//}


		switch (trigger)
		{
			case "":
			case null:
			case "none":
				return true;

			case "on_accept_accept": return accepted && oppAccepted;
			case "on_accept_refuse": return accepted && !oppAccepted;
			case "on_refuse_refuse": return !accepted && !oppAccepted;
			case "on_reveal":
			case "on_points":
			case "on_choice":
				return true;     // None are actual triggers (do nothing)
			default:
				Debug.LogWarning("Unknown trigger: " + trigger);
				return false;
		}
	}

	private static void ApplyEffect(
	Player current,
	Player opponent,
	string effect,
	string target,
	int amount,
	ref (int Points, int Burnout, int Flex, int Tokens) currentGains,
	ref (int Points, int Burnout, int Flex, int Tokens) opponentGains
)
	{

		// OLD EFFECTS
		//switch (res)
		//{
		//	case "AddBurnout": currentPlayerGains.Burnout++; break;
		//	case "RemoveBurnout": currentPlayerGains.Burnout--; break;
		//	case "AddFlexibility": currentPlayerGains.Flex++; break;
		//	case "RemoveFlexibility": currentPlayerGains.Flex--; break;
		//	case "AddPoint": currentPlayerGains.Points++; break;
		//	case "RemovePoint": currentPlayerGains.Points--; break;
		//	case "DrawCard":
		//		currentPlayer.DrawCard(); break;
		//		//case "DiscardCard": currentPlayer.DiscardCard(currentPlayer.PickCard()); break; }

		//}


		if (amount < 1) amount = 1;

		switch (effect)
		{
			// ---------------------------------------
			// DRAW (NOT IMPLEMENTED YET but might be easy to add -> safe ignore for now)
			// ---------------------------------------
			case "draw":
				Debug.Log("Effect not implemented: " + effect);
				break;
			//if (target == "self") current.DrawCards(amount);
			//else if (target == "opponent") opponent.DrawCards(amount);
			//else if (target == "both") { current.DrawCards(amount); opponent.DrawCards(amount); }
			//break;

			// ---------------------------------------
			// REDUCE BURNOUT
			// ---------------------------------------
			case "reduce_burnout":
				if (target == "self" || target == "both") currentGains.Burnout -= amount;
				if (target == "opponent" || target == "both") opponentGains.Burnout -= amount;
				break;

			// ---------------------------------------
			// GAIN POINTS
			// ---------------------------------------
			case "gain_points":
				if (target == "self" || target == "both") currentGains.Points += amount;
				if (target == "opponent" || target == "both") opponentGains.Points += amount;
				break;

			// ---------------------------------------
			// TOKENS
			// ---------------------------------------
			case "gain_token":
				if (target == "self" || target == "both") currentGains.Tokens += amount;
				if (target == "opponent" || target == "both") opponentGains.Tokens += amount;
				break;

			// ---------------------------------------
			// PREVENT BURNOUT 
			// ---------------------------------------
			case "prevent_burnout":
				if (target == "self" || target == "both") currentGains.Burnout = 0;
				if (target == "opponent" || target == "both") opponentGains.Burnout = 0;
				break;

			// ---------------------------------------
			// NOT IMPLEMENTED YET (safe ignore)
			// ---------------------------------------
			case "peek":
			case "reorder_top":
			case "swap_with_top":
			case "hold_overdraw":
				Debug.Log("Effect not implemented: " + effect);
				break;

			default:
				Debug.LogWarning("Unknown card effect: " + effect);
				break;
		}
	}
}
