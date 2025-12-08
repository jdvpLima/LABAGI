using Unity.Netcode;
using UnityEngine;
using TMPro;
using System.Collections;

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
    public void PlayerSubmittedCard(ulong clientId, long cardId, int points, string suit)
    {
        if (!IsServer) return;
        if (isGameOver) return; 

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
        // Initialize all gains to 0
        int hostPointsGain = 0; int hostBurnoutGain = 0; int hostFlexGain = 0;
        int clientPointsGain = 0; int clientBurnoutGain = 0; int clientFlexGain = 0;

        // 1. Scoring Logic
        if (hostAccepted && clientAccepted)
        {
            hostPointsGain = hostCardPoints;
            clientPointsGain = clientCardPoints;

            // Innovation Breakthrough Logic
            bool differentSuits = (hostCardSuit != clientCardSuit);
            if (differentSuits)
            {
                if (hostPlayer.Burnout.Value < 3) hostPointsGain += 1;
                if (clientPlayer.Burnout.Value < 3) clientPointsGain += 1;
                UpdateStatusClientRpc("Innovation Breakthrough! (+1 Bonus)");
            }
            else
            {
                UpdateStatusClientRpc("Both Accepted!");
            }
        }
        else if (hostAccepted && !clientAccepted)
        {
            clientPointsGain = clientCardPoints;
            UpdateStatusClientRpc("Client Refused!");
        }
        else if (!hostAccepted && clientAccepted)
        {
            hostPointsGain = hostCardPoints;
            UpdateStatusClientRpc("Host Refused!");
        }
        else
        {
            UpdateStatusClientRpc("Both Refused!");
        }

        // 2. Flexibility & Burnout Rules
        hostFlexGain = hostAccepted ? 1 : -1;
        if (hostAccepted) hostBurnoutGain -= 1; else hostBurnoutGain += 1;

        clientFlexGain = clientAccepted ? 1 : -1;
        if (clientAccepted) clientBurnoutGain -= 1; else clientBurnoutGain += 1;

        // 3. Apply Results
        bool hostReset = hostPlayer.ServerApplyRoundResult(hostPointsGain, hostBurnoutGain, hostFlexGain);
        bool clientReset = clientPlayer.ServerApplyRoundResult(clientPointsGain, clientBurnoutGain, clientFlexGain);

        // 4. Critical Flexibility Penalty
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
        hostCardId = -1; clientCardId = -1;
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

            // 1. Gather all stats
            int hScore = hostPlayer.Points.Value;
            int hFlex = hostPlayer.Flexibility.Value;
            int hBurn = hostPlayer.Burnout.Value;
            int hTok = hostPlayer.AccommodationTokens.Value;

            int cScore = clientPlayer.Points.Value;
            int cFlex = clientPlayer.Flexibility.Value;
            int cBurn = clientPlayer.Burnout.Value;
            int cTok = clientPlayer.AccommodationTokens.Value;

            // 2. Send data to all clients so they can save it locally
            EndGameDataClientRpc(hScore, hFlex, hBurn, hTok, cScore, cFlex, cBurn, cTok);

            // 3. Start Coroutine to switch scene after a delay
            StartCoroutine(EndGameRoutine());

            return true;
        }

        return false;
    }

    [ClientRpc]
    private void EndGameDataClientRpc(int hScore, int hFlex, int hBurn, int hTok, int cScore, int cFlex, int cBurn, int cTok)
    {
        // "IsHost" tells the storage whether to show me the Host stats or Client stats
        MatchResultsStorage.SetData(IsHost, hScore, hFlex, hBurn, hTok, cScore, cFlex, cBurn, cTok);
        
        UpdateStatusClientRpc("Match Finished! Loading Results...");
    }

    [ClientRpc]
    private void UpdateStatusClientRpc(string msg)
    {
        if (statusText != null) statusText.text = msg;
    }

    private IEnumerator EndGameRoutine()
    {
        // Wait 3 seconds so players see the final card result
        yield return new WaitForSeconds(3.0f);

        // Tell NetworkManager to switch scenes for everyone
        NetworkManager.Singleton.SceneManager.LoadScene("GameResults", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}