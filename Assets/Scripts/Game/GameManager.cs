using Unity.Netcode;
using UnityEngine;
using TMPro;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    [Header("UI References")]
    public TextMeshProUGUI statusText;

    // Connected Players
    private Player hostPlayer;
    private Player clientPlayer;

    // Round Data
    private long hostCardId = -1;
    private int hostCardPoints = 0;
    private string hostCardSuit = "";

    private long clientCardId = -1;
    private int clientCardPoints = 0;
    private string clientCardSuit = "";

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    // Called by Player.cs OnNetworkSpawn
    public void RegisterPlayer(Player p)
    {
        if (!IsServer) return;

        if (p.OwnerClientId == NetworkManager.ServerClientId)
        {
            hostPlayer = p;
            Debug.Log("Host Registered");
        }
        else
        {
            clientPlayer = p;
            Debug.Log("Client Registered");
        }

        CheckGameStart();
    }

    private void CheckGameStart()
    {
        if (hostPlayer != null && clientPlayer != null)
        {
            UpdateStatusClientRpc("Game Start! Both players connected.");
        }
        else
        {
            UpdateStatusClientRpc("Waiting for opponent...");
        }
    }

    // Called by Player.cs
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
        // Check if both have played
        if (hostCardId != -1 && clientCardId != -1)
        {
            ResolveTurn();
        }
        else
        {
            UpdateStatusClientRpc("Waiting for other player...");
        }
    }

    private void ResolveTurn()
    {
        // --- YOUR GAME LOGIC HERE ---
        // Example: Compare Suits or Points
        
        Debug.Log($"Resolving: Host({hostCardSuit}) vs Client({clientCardSuit})");

        int hostPointsGain = 0;
        int clientPointsGain = 0;

        // Simple Synergy Rule Example
        if (hostCardSuit == clientCardSuit)
        {
            hostPointsGain += 5;
            clientPointsGain += 5;
            UpdateStatusClientRpc($"Synergy! Both played {hostCardSuit}");
        }
        else
        {
            hostPointsGain += hostCardPoints;
            clientPointsGain += clientCardPoints;
            UpdateStatusClientRpc("Turn Resolved.");
        }

        // Apply to network variables
        hostPlayer.ServerApplyRoundResult(hostPointsGain, 0);
        clientPlayer.ServerApplyRoundResult(clientPointsGain, 0);

        // Reset for next turn
        hostCardId = -1;
        clientCardId = -1;
    }

    [ClientRpc]
    private void UpdateStatusClientRpc(string msg)
    {
        if (statusText != null) statusText.text = msg;
    }
}