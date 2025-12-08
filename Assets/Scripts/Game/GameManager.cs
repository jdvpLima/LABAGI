using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.Rendering.GPUSort;

/// The Server-Side Referee.
/// Manages turn states, resolves scoring rules, and coordinates clients.
public class GameManager : NetworkBehaviour
{
	public static GameManager Instance;

	[Header("UI References")]
	public TextMeshProUGUI statusText;

	// Track both players
	private Player hostPlayer;
	private Player clientPlayer;

	// Stores the current round's card data
	private Card hostCard;
	private Card clientCard;

	// Stores player decisions (Accept/Refuse)
	private bool hostDecisionReceived = false;
	private bool hostAccepted = false;
	private bool clientDecisionReceived = false;
	private bool clientAccepted = false;

    // Track Game State
    private bool isGameOver = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    /// Called by Player.cs when a player connects to the Game Scene.
    public void RegisterPlayer(Player p)
    {
        if (!IsServer) return; 

		if (p.OwnerClientId == NetworkManager.ServerClientId) hostPlayer = p;
		else clientPlayer = p;

		if (hostPlayer != null && clientPlayer != null) UpdateStatusClientRpc("Game Start!");
	}

	/// Called when a player locks in a card. Stores data and checks if round can proceed.
	public void PlayerSubmittedCard(ulong clientId, long cardId, int cardPoints, string cardSuit, string cardActions)
	{
		List<string> actions = cardActions.Split(',').ToList();

		if (!IsServer) return;
		if (isGameOver) return;

		if (hostPlayer != null && clientId == hostPlayer.OwnerClientId)
		{
			hostCard = new Card(cardId, cardSuit, cardPoints, actions);
		}
		else if (clientPlayer != null && clientId == clientPlayer.OwnerClientId)
		{
			clientCard = new Card(cardId, cardSuit, cardPoints, actions);
		}

		CheckTurnResolution();
	}

	private void CheckTurnResolution()
	{
		// If both players have played cards, start the Decision Phase
		if (hostCard != null && clientCard != null) StartDecisionPhase();
		else UpdateStatusClientRpc("Waiting for other player...");
	}

	/// Triggers the "Accept/Refuse" phase and reveals opponent suits.
	private void StartDecisionPhase()
	{
		UpdateStatusClientRpc("Review Phase: Accept or Refuse?");
		// Reveal suits to opponents
		hostPlayer.RevealOpponentCardClientRpc(clientCard.Suit);
		clientPlayer.RevealOpponentCardClientRpc(hostCard.Suit);
		// Show UI Buttons
		hostPlayer.EnableDecisionPhaseClientRpc();
		clientPlayer.EnableDecisionPhaseClientRpc();
	}

    public void PlayerMadeDecision(ulong clientId, bool accepted)
    {
        if (!IsServer) return;
        if (isGameOver) return; 

		if (hostPlayer != null && clientId == hostPlayer.OwnerClientId)
		{
			hostDecisionReceived = true;
			hostAccepted = accepted;
		}
		else if (clientPlayer != null && clientId == clientPlayer.OwnerClientId)
		{
			clientDecisionReceived = true;
			clientAccepted = accepted;
		}

		// If both decided, calculate scores
		if (hostDecisionReceived && clientDecisionReceived) FinalizeRound();
	}

    /// Handle MANUAL token usage (Button Click).
    /// Rule: Using a token gives the Opponent +1 Point.
    public void PlayerUsedToken(ulong clientId)
    {
        if (!IsServer) return;
        if (isGameOver) return; 

        if (hostPlayer != null && clientId == hostPlayer.OwnerClientId)
        {
            clientPlayer.Points.Value += 1; 
            UpdateStatusClientRpc("Host used Token! Client +1 Point.");
        }
        else if (clientPlayer != null && clientId == clientPlayer.OwnerClientId)
        {
            hostPlayer.Points.Value += 1; 
            UpdateStatusClientRpc("Client used Token! Host +1 Point.");
        }

        // Check if this token usage ended the game
        CheckGameOver();
    }

	/// Calculates scores based on Acceptance/Refusal rules.
	private void FinalizeRound()
	{
		var hostGains = (Points: 0, Burnout: 0, Flex: 0, Tokens: 0);
		var clientGains = (Points: 0, Burnout: 0, Flex: 0, Tokens: 0);

		// Manage Proposal result (handle points, synergy bonus, and execute cards' actions)
		if (hostAccepted && clientAccepted)
		{
			// Check and apply Synergy Bonus
			if (hostCard.Suit == clientCard.Suit)
			{
				if (hostPlayer.Burnout.Value < 3) hostGains.Points++;
				if (clientPlayer.Burnout.Value < 3) clientGains.Points++;
			}

			// Both get points
			hostGains.Points += hostCard.Points;
			clientGains.Points += clientCard.Points;

			// Trigger both cards' actions
			// Execute host card's actions
			CardLogic.ExecuteCardActions(hostPlayer, clientPlayer, hostCard, clientCard, hostAccepted, clientAccepted, hostGains, clientGains);
			// Execute client card's actions
			CardLogic.ExecuteCardActions(clientPlayer, hostPlayer, clientCard, hostCard, clientAccepted, hostAccepted, clientGains, hostGains);

			UpdateStatusClientRpc("Both Accepted!");
		}
		else if (hostAccepted && !clientAccepted)
		{
			// Only Client refused, they get points
			clientGains.Points += clientCard.Points;

			// Execute client card's actions
			CardLogic.ExecuteCardActions(clientPlayer, hostPlayer, clientCard, hostCard, clientAccepted, hostAccepted, clientGains, hostGains);

			UpdateStatusClientRpc("Client Refused!");
		}
		else if (!hostAccepted && clientAccepted)
		{
			// Only Host refused, they get points
			hostGains.Points += hostCard.Points;

			// Execute host card's actions
			CardLogic.ExecuteCardActions(hostPlayer, clientPlayer, hostCard, clientCard, hostAccepted, clientAccepted, hostGains, clientGains);

			UpdateStatusClientRpc("Host Refused!");
		}
		else
		{
			// Both refused: No points nor card triggers for any
			UpdateStatusClientRpc("Both Refused!");
		}

		// Manage Flexibility (Rules => Accept: Flex +1, Refuse: Flex -1)
		hostGains.Flex += hostAccepted ? 1 : -1;
		clientGains.Flex += clientAccepted ? 1 : -1;

		// Manage Burnout (Rules => Accept+ Burnout -1, Refuse: Burnout +1)
		hostGains.Burnout += hostAccepted ? -1 : 1;
		clientGains.Burnout += clientAccepted ? -1 : 1;

		// 3. Apply Results to Players
		// Capture return value to see if Flexibility limit was broken
		bool hostReset = hostPlayer.ServerApplyRoundResult(hostGains.Points, hostGains.Burnout, hostGains.Flex);
		bool clientReset = clientPlayer.ServerApplyRoundResult(clientGains.Points, clientGains.Burnout, clientGains.Flex);

		// 4. Critical Flexibility Penalty (Opponent +1 Point)
		if (hostReset)
		{
			clientPlayer.Points.Value += 1;
			UpdateStatusClientRpc("Host Flexibility Break! Client +1 Point.");
		}
		if (clientReset)
		{
			hostPlayer.Points.Value += 1;
			UpdateStatusClientRpc("Client Flexibility Break! Host +1 Point.");
		}

		// --- Check Win Condition ---
		if (CheckGameOver()) return; 

        // Cleanup visuals
        hostPlayer.CleanupRoundClientRpc();
        clientPlayer.CleanupRoundClientRpc();

		// Reset variables
		hostCard = null; clientCard = null;
		hostDecisionReceived = false; clientDecisionReceived = false;
	}

    // --- Win Condition Helper ---
    private bool CheckGameOver()
    {
        if (hostPlayer == null || clientPlayer == null) return false;

        bool hostWins = hostPlayer.Points.Value >= 15;
        bool clientWins = clientPlayer.Points.Value >= 15;

        if (hostWins || clientWins)
        {
            isGameOver = true;

            // >>> CHANGE IS HERE <<<
            if (hostWins && clientWins) 
                UpdateStatusClientRpc("GAME OVER: BOTH OF YOU WIN!");
            else if (hostWins) 
                UpdateStatusClientRpc("GAME OVER: HOST WINS!");
            else 
                UpdateStatusClientRpc("GAME OVER: CLIENT WINS!");

            return true;
        }

        return false;
    }

    [ClientRpc]
    private void UpdateStatusClientRpc(string msg)
    {
        if (statusText != null) statusText.text = msg;
    }
}