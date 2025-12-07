using Unity.Netcode;
using UnityEngine;
using TMPro;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    [Header("UI References")]
    public TextMeshProUGUI statusText;

    private Player hostPlayer;
    private Player clientPlayer;

    // Card Data
    private long hostCardId = -1;
    private int hostCardPoints = 0;
    private string hostCardSuit = "";

    private long clientCardId = -1;
    private int clientCardPoints = 0;
    private string clientCardSuit = "";

    // Decisions
    private bool hostDecisionReceived = false;
    private bool hostAccepted = false;
    private bool clientDecisionReceived = false;
    private bool clientAccepted = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    public void RegisterPlayer(Player p)
    {
        if (!IsServer) return;
        if (p.OwnerClientId == NetworkManager.ServerClientId) hostPlayer = p;
        else clientPlayer = p;
        
        if (hostPlayer != null && clientPlayer != null) UpdateStatusClientRpc("Game Start!");
    }

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
        if (hostCardId != -1 && clientCardId != -1)
        {
            StartDecisionPhase();
        }
        else
        {
            UpdateStatusClientRpc("Waiting for other player...");
        }
    }

    private void StartDecisionPhase()
    {
        UpdateStatusClientRpc("Review Phase: Accept or Refuse?");
        
        // Show buttons
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

        if (hostDecisionReceived && clientDecisionReceived)
        {
            FinalizeRound();
        }
    }

    private void FinalizeRound()
    {
        int hostPointsGain = 0;
        int hostBurnoutGain = 0;
        int clientPointsGain = 0;
        int clientBurnoutGain = 0;

        // --- YOUR NEW RULES ---

        // 1. Both Accept: Earn Card Points
        if (hostAccepted && clientAccepted)
        {
            hostPointsGain = hostCardPoints;
            clientPointsGain = clientCardPoints;
            UpdateStatusClientRpc("Both Accepted! Points gained.");
        }
        // 2. Host Accepts, Client Refuses
        else if (hostAccepted && !clientAccepted)
        {
            // Host gets nothing
            // Client gets points + 1 Burnout
            clientPointsGain = clientCardPoints;
            clientBurnoutGain = 1;
            UpdateStatusClientRpc("Client Refused! Client gains Points + Burnout.");
        }
        // 3. Host Refuses, Client Accepts
        else if (!hostAccepted && clientAccepted)
        {
            // Host gets points + 1 Burnout
            // Client gets nothing
            hostPointsGain = hostCardPoints;
            hostBurnoutGain = 1;
            UpdateStatusClientRpc("Host Refused! Host gains Points + Burnout.");
        }
        // 4. Both Refuse
        else
        {
            // Both get 1 Burnout
            hostBurnoutGain = 1;
            clientBurnoutGain = 1;
            UpdateStatusClientRpc("Both Refused! Burnout increased.");
        }

        // Apply
        hostPlayer.ServerApplyRoundResult(hostPointsGain, hostBurnoutGain);
        clientPlayer.ServerApplyRoundResult(clientPointsGain, clientBurnoutGain);

        // Cleanup
        hostPlayer.CleanupRoundClientRpc();
        clientPlayer.CleanupRoundClientRpc();

        // Reset
        hostCardId = -1; clientCardId = -1;
        hostDecisionReceived = false; clientDecisionReceived = false;
    }

    [ClientRpc]
    private void UpdateStatusClientRpc(string msg)
    {
        if (statusText != null) statusText.text = msg;
    }
}