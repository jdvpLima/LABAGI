using System;

public static class CardLogic
{
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
		foreach (string rule in currentCard.Actions)
		{
			string[] split = rule.Split('?');
			string condition = split[0];
			string result = split[1];

			bool trigger = true;

			// --------------------------
			// Evaluate CONDITIONS
			// --------------------------
			foreach (string cond in condition.Split('&'))
			{
				switch (cond)
				{
					// Decision logic
					case "BothAccept": trigger &= currentAccepted && opponentAccepted; break;
					case "BothRefuse": trigger &= !currentAccepted && !opponentAccepted; break;
					case "PlayerAccepts": trigger &= currentAccepted; break;
					case "PlayerRejects": trigger &= !currentAccepted; break;

					// Suits
					case "EqualSuits": trigger &= currentCard.Suit == opponentCard.Suit; break;
					case "DiffSuits": trigger &= currentCard.Suit != opponentCard.Suit; break;

					// Card point comparisons
					case "CardPointsEQOpp": trigger &= currentCard.Points == opponentCard.Points; break;
					case "CardPointsGTOpp": trigger &= currentCard.Points > opponentCard.Points; break;
					case "CardPointsLTOpp": trigger &= currentCard.Points < opponentCard.Points; break;

					// Burnout
					case "BurnoutEQOpp": trigger &= currentPlayer.Burnout.Value == opponentPlayer.Burnout.Value; break;
					case "BurnoutGTOpp": trigger &= currentPlayer.Burnout.Value > opponentPlayer.Burnout.Value; break;
					case "BurnoutLTOpp": trigger &= currentPlayer.Burnout.Value < opponentPlayer.Burnout.Value; break;

					// Flexibility
					case "FlexibilityEQOpp": trigger &= currentPlayer.Flexibility.Value == opponentPlayer.Flexibility.Value; break;
					case "FlexibilityGTOpp": trigger &= currentPlayer.Flexibility.Value > opponentPlayer.Flexibility.Value; break;
					case "FlexibilityLTOpp": trigger &= currentPlayer.Flexibility.Value < opponentPlayer.Flexibility.Value; break;

					// Points
					case "PlayerPointsEQOpp": trigger &= currentPlayer.Points.Value == opponentPlayer.Points.Value; break;
					case "PlayerPointsGTOpp": trigger &= currentPlayer.Points.Value > opponentPlayer.Points.Value; break;
					case "PlayerPointsLTOpp": trigger &= currentPlayer.Points.Value < opponentPlayer.Points.Value; break;

					// Tokens
					case "TokenUsed": trigger &= currentPlayer.tokenUsed; break;
					case "OppTokenUsed": trigger &= opponentPlayer.tokenUsed; break;
				}
			}

			if (!trigger)
				return;

			// --------------------------
			// Execute RESULTS
			// --------------------------
			foreach (string res in result.Split('&'))
			{
				switch (res)
				{
					case "AddBurnout": currentPlayerGains.Burnout++; break;
					case "RemoveBurnout": currentPlayerGains.Burnout--; break;

					case "AddFlexibility": currentPlayerGains.Flex++; break;
					case "RemoveFlexibility": currentPlayerGains.Flex--; break;

					case "AddPoint": currentPlayerGains.Points++; break;
					case "RemovePoint": currentPlayerGains.Points--; break;

					case "DrawCard": currentPlayer.DrawCard(); break;
						//case "DiscardCard": currentPlayer.DiscardCard(currentPlayer.PickCard()); break;
				}
			}
		}
	}

}
