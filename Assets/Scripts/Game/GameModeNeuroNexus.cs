using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Templates.MRTTabletopAssets;
using XRMultiplayer;

public class GameModeNeuroNexus : MonoBehaviour, IGameMode
{
    public int gameModeID => m_GameModeID;
    [SerializeField] int m_GameModeID = 1;

    public GameObject cardPrefab;
    public Transform cardSpawnRoot; // um empty object em frente ao player

    // Test
    // [SerializeField] GameObject modelToView;

    void Start()
    {
        HideGameMode();

        if(cardPrefab == null)
        {
            Debug.LogWarning("No card prefab assigned!!");
        }

        if (cardSpawnRoot == null)
        {
            Debug.LogWarning("No card root assigned!!");
        }
    }

    public void HideGameMode()
    {
        
    }

    public void ShowGameMode()
    {
        Debug.Log("Showing NeuroNexus.");
        for (int i = 0; i < 5; i++)
        {
            Vector3 offset = new Vector3(i * 0.09f, 0, 0);
            Instantiate(cardPrefab, cardSpawnRoot.position + offset, cardSpawnRoot.rotation, cardSpawnRoot);
            Debug.Log("CARTA INSTANCIADA");
        }
    }

    public void OnGameModeStart()
    {
        Debug.Log("OnGameModeStart() IS ON!");
        for (int i = 0; i < 5; i++)
        {
            Vector3 offset = new Vector3(i * 0.2f, 0, 0);
            Instantiate(cardPrefab, cardSpawnRoot.position + offset, cardSpawnRoot.rotation, cardSpawnRoot);
            Debug.Log("CARTA INSTANCIADA");
        }

    }

    public void OnGameModeEnd()
    {
        Debug.Log("Ended game NeuroNexus.");
        
    }

}
