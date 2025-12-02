using Unity.Netcode;
using UnityEngine;

public class DeckController : NetworkBehaviour
{
    public GameObject cardPrefab;
    public Transform cardAnchor;
    public int numberOfCards = 30;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            SpawnDeckForOwner(OwnerClientId);
        }
    }

    private void SpawnDeckForOwner(ulong ownerId)
    {
        for (int i = 0; i < numberOfCards; i++)
        {
            Debug.Log("Spawned card!!!!!!!!!");
            Vector3 pos = cardAnchor.position + new Vector3(0, i * 0.002f, 0);
            Quaternion rot = Quaternion.identity;

            GameObject card = Instantiate(cardPrefab, pos, rot);
            var netObj = card.GetComponent<NetworkObject>();

            // Dá ownership ao dono
            netObj.SpawnWithOwnership(ownerId);

            // Define o ID do dono dentro da carta
            card.GetComponent<CardNetworkState>().SetOwnerServerRpc(ownerId);
        }
    }
}
