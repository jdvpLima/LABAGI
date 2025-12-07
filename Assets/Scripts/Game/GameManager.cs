using Unity.Netcode;
using UnityEngine;
using TMPro;

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

    // Stores the current round's data
    private long hostCardId = -1;
    private int hostCardPoints = 0;
    private string hostCardSuit = "";

    private long clientCardId = -1;
    private int clientCardPoints = 0;
    private string clientCardSuit = "";

    // Stores player decisions (Accept/Refuse)
    private bool hostDecisionReceived = false;
    private bool hostAccepted = false;
    private bool clientDecisionReceived = false;
    private bool clientAccepted = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    /// Called by Player.cs when a player connects to the Game Scene.
    public void RegisterPlayer(Player p)
    {
        if (!IsServer) return; // Only Server manages registration

        if (p.OwnerClientId == NetworkManager.ServerClientId) hostPlayer = p;
        else clientPlayer = p;
        
        if (hostPlayer != null && clientPlayer != null) UpdateStatusClientRpc("Game Start!");
    }

    /// Called when a player locks in a card. Stores data and checks if round can proceed.
    public void PlayerSubmittedCard(ulong clientId, long cardId, int points, string suit)
    {
        if (!IsServer) return;

        if (hostPlayer != null && clientId == hostPlayer.OwnerClientId)
        {
            hostCardId = cardId;
            hostCardPoints = points;
            hostCardSuit = suit;
        }
        else if (clientPlayer != null && clientId == clientPlayer.OwnerClientId)
        {
            clientCardId = cardId;
            clientCardPoints = points;
            clientCardSuit = suit;
        }

        CheckTurnResolution();
    }

    private void CheckTurnResolution()
    {
        // If both players have played cards, start the Decision Phase
        if (hostCardId != -1 && clientCardId != -1) StartDecisionPhase();
        else UpdateStatusClientRpc("Waiting for other player...");
    }

    /// Triggers the "Accept/Refuse" phase and reveals opponent suits.
    private void StartDecisionPhase()
    {
        UpdateStatusClientRpc("Review Phase: Accept or Refuse?");
        // Reveal suits to opponents
        hostPlayer.RevealOpponentCardClientRpc(clientCardId, clientCardSuit, clientCardPoints);
        clientPlayer.RevealOpponentCardClientRpc(hostCardId, hostCardSuit, hostCardPoints);
        // Show UI Buttons
        hostPlayer.EnableDecisionPhaseClientRpc();
        clientPlayer.EnableDecisionPhaseClientRpc();
    }

    public void PlayerMadeDecision(ulong clientId, bool accepted)
    {
        if (!IsServer) return;

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
    }

    /// Calculates scores based on Acceptance/Refusal rules.
    private void FinalizeRound()
    {
        int hostPointsGain = 0; int hostBurnoutGain = 0; int hostFlexGain = 0;
        int clientPointsGain = 0; int clientBurnoutGain = 0; int clientFlexGain = 0;

        // 1. Scoring Logic (Who gets the points?)
        if (hostAccepted && clientAccepted)
        {
            hostPointsGain = hostCardPoints;
            clientPointsGain = clientCardPoints;
            UpdateStatusClientRpc("Both Accepted!");
        }
        else if (hostAccepted && !clientAccepted)
        {
            // Client refused: They steal points + 1 Burnout
            clientPointsGain = clientCardPoints;
            clientBurnoutGain = 1;
            UpdateStatusClientRpc("Client Refused!");
        }
        else if (!hostAccepted && clientAccepted)
        {
            // Host refused: They steal points + 1 Burnout
            hostPointsGain = hostCardPoints;
            hostBurnoutGain = 1;
            UpdateStatusClientRpc("Host Refused!");
        }
        else
        {
            // Both refused: Only Burnout
            hostBurnoutGain = 1;
            clientBurnoutGain = 1;
            UpdateStatusClientRpc("Both Refused!");
        }

        // 2. Flexibility Rules (Accept = Flex +1, Refuse = Flex -1)
        hostFlexGain = hostAccepted ? 1 : -1;
        if (hostAccepted) hostBurnoutGain -= 1; else hostBurnoutGain += 1;

        clientFlexGain = clientAccepted ? 1 : -1;
        if (clientAccepted) clientBurnoutGain -= 1; else clientBurnoutGain += 1;

        // 3. Apply Results to Players
        // Capture return value to see if Flexibility limit was broken
        bool hostReset = hostPlayer.ServerApplyRoundResult(hostPointsGain, hostBurnoutGain, hostFlexGain);
        bool clientReset = clientPlayer.ServerApplyRoundResult(clientPointsGain, clientBurnoutGain, clientFlexGain);

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

        // Cleanup visuals
        hostPlayer.CleanupRoundClientRpc();
        clientPlayer.CleanupRoundClientRpc();

        // Reset variables
        hostCardId = -1; clientCardId = -1;
        hostDecisionReceived = false; clientDecisionReceived = false;
    }

    [ClientRpc]
    private void UpdateStatusClientRpc(string msg)
    {
        if (statusText != null) statusText.text = msg;
    }
}