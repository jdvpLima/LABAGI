using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace UnityEngine.XR.Templates.MRTTabletopAssets
{
    public class GameModeDeckSetup : MonoBehaviour, IGameMode
    {
        public int gameModeID => m_GameModeID;

        [SerializeField]
        int m_GameModeID = 4;

        [Header("Deck Prefab (NetworkObject)")]
        [SerializeField]
        private GameObject deckPrefab;

        [Header("Opcional: Parent para os decks")]
        [SerializeField]
        private Transform decksParent;

        // estado interno
        bool m_IsShown = false;
        readonly List<NetworkObject> m_SpawnedDecks = new List<NetworkObject>();

        public void OnGameModeEnd()
        {
            // servidor limpa os decks que criou
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                for (int i = m_SpawnedDecks.Count - 1; i >= 0; i--)
                {
                    var net = m_SpawnedDecks[i];
                    if (net != null && net.IsSpawned)
                    {
                        // Despacha o NetworkObject (remover da rede)
                        net.Despawn(true);
                        // Também destrói o GO localmente
                        if (net.gameObject != null)
                            Destroy(net.gameObject);
                    }
                }

                m_SpawnedDecks.Clear();
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            }

            // qualquer limpeza visual/local adicional aqui
        }

        public void OnGameModeStart()
        {
            if (!NetworkManager.Singleton.IsServer)
                return;

            // Spawn a deck for each connected client
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                SpawnDeckForClient(client.ClientId);
            }

            // Spawn deck for any client joining later
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }

        void OnClientConnected(ulong clientId)
        {
            if (!NetworkManager.Singleton.IsServer)
                return;

            SpawnDeckForClient(clientId);
        }

        private void SpawnDeckForClient(ulong clientId)
        {
            if (deckPrefab == null)
            {
                Debug.LogError($"[{nameof(GameModeDeckSetup)}] deckPrefab não está atribuído.");
                return;
            }

            GameObject deckGO = Instantiate(deckPrefab, decksParent != null ? decksParent : null);
            var netObj = deckGO.GetComponent<NetworkObject>();
            if (netObj == null)
            {
                Debug.LogError($"[{nameof(GameModeDeckSetup)}] Deck prefab precisa de um NetworkObject.");
                Destroy(deckGO);
                return;
            }

            // Spawn com ownership do cliente
            netObj.SpawnWithOwnership(clientId);

            // Guarda referência para limpeza posterior (servidor apenas)
            if (NetworkManager.Singleton.IsServer)
                m_SpawnedDecks.Add(netObj);
        }

        public void HideGameMode()
        {
            if (!m_IsShown)
                return;

            m_IsShown = false;

            // chama a lógica de "end" (limpeza)
            OnGameModeEnd();

            // desativa a representação visual do GameMode (se houver)
            gameObject.SetActive(false);
        }

        public void ShowGameMode()
        {
            if (m_IsShown)
                return;

            m_IsShown = true;

            // ativa o objeto do GameMode (útil para UI/inspector)
            gameObject.SetActive(true);

            // chama a lógica de start
            OnGameModeStart();
        }
    }
}

