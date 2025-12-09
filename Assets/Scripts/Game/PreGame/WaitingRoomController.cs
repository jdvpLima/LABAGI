using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WaitingRoomController : NetworkBehaviour
{
    [SerializeField] private GameObject waitingRoomUI;
    [SerializeField] private GameObject hostBtn;
    [SerializeField] private GameObject joinBtn;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        int playerCount = NetworkManager.Singleton.ConnectedClients.Count;

        if (playerCount >= 2)
        {
            // Both players connected, start the game
            StartGame();
        }
    }

    private void StartGame()
    {
        if (!IsServer) return; // Only the host triggers scene load

        Debug.Log("Both players connected. Loading Game scene...");

        // Hide the waiting room UI before changing scene
        if (waitingRoomUI != null)
            waitingRoomUI.SetActive(false);

        // Load the Game scene for all clients
        NetworkManager.Singleton.SceneManager.LoadScene("Game", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    public void OnBackButtonClicked()
    {
        // Hide waiting room UI
        if (waitingRoomUI != null)
            waitingRoomUI.SetActive(false);

        if (hostBtn != null)
            hostBtn.SetActive(false);

        if (joinBtn != null)
            joinBtn.SetActive(false);

        // 2. Shutdown the network session
        var nm = NetworkManager.Singleton;
        if (nm != null && nm.IsListening)
        {
            // Unregister callbacks if server
            if (IsServer)
            {
                nm.OnClientConnectedCallback -= OnClientConnected;
            }

            nm.Shutdown();
        }

        Debug.Log("Exited waiting room. Network shutdown.");
    }

}
