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
        int hostFlexGain = 0;

        int clientPointsGain = 0;
        int clientBurnoutGain = 0;
        int clientFlexGain = 0;

        // --- 1. POINTS LOGIC ---
        if (hostAccepted && clientAccepted)
        {
            // Both Accept: Both get points
            hostPointsGain = hostCardPoints;
            clientPointsGain = clientCardPoints;
            UpdateStatusClientRpc("Both Accepted!");
        }
        else if (hostAccepted && !clientAccepted)
        {
            // Client Refused: Client steals points
            clientPointsGain = clientCardPoints;
            UpdateStatusClientRpc("Client Refused! Client gains Points.");
        }
        else if (!hostAccepted && clientAccepted)
        {
            // Host Refused: Host steals points
            hostPointsGain = hostCardPoints;
            UpdateStatusClientRpc("Host Refused! Host gains Points.");
        }
        else
        {
            // Both Refused: No points
            UpdateStatusClientRpc("Both Refused!");
        }

        // --- 2. BURNOUT & FLEXIBILITY LOGIC  ---
        
        // Host Stats
        if (hostAccepted)
        {
            hostBurnoutGain = -1; // Decrease Burnout
            hostFlexGain = 1;     // Increase Flexibility
        }
        else // Host Refused
        {
            hostBurnoutGain = 1;  // Increase Burnout
            hostFlexGain = -1;    // Decrease Flexibility
        }

        // Client Stats
        if (clientAccepted)
        {
            clientBurnoutGain = -1;
            clientFlexGain = 1;
        }
        else // Client Refused
        {
            clientBurnoutGain = 1;
            clientFlexGain = -1;
        }

        // Pass the calculated Flexibility (3rd argument)
        hostPlayer.ServerApplyRoundResult(hostPointsGain, hostBurnoutGain, hostFlexGain);
        clientPlayer.ServerApplyRoundResult(clientPointsGain, clientBurnoutGain, clientFlexGain);

        // Cleanup
        hostPlayer.CleanupRoundClientRpc();
        clientPlayer.CleanupRoundClientRpc();

        // Reset Server State
        hostCardId = -1; clientCardId = -1;
        hostDecisionReceived = false; clientDecisionReceived = false;
    }

    [ClientRpc]
    private void UpdateStatusClientRpc(string msg)
    {
        if (statusText != null) statusText.text = msg;
    }
}