using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Templates.MRTTabletopAssets;
using XRMultiplayer;

public class GameModeNeuroNexus : MonoBehaviour, IGameMode
{
    public int gameModeID => m_GameModeID;
    [SerializeField] int m_GameModeID = 1;


    [SerializeField] GameObject modelToView;

    void Start()
    {
        HideGameMode();
    }

    public void HideGameMode()
    {
        modelToView.SetActive(false);
    }

    public void ShowGameMode()
    {
        Debug.Log("Showing NeuroNexus.");
        modelToView.SetActive(true);
    }

    public void OnGameModeStart()
    {
        Debug.Log("Started game NeuroNexus.");
        
    }

    public void OnGameModeEnd()
    {
        Debug.Log("Ended game NeuroNexus.");
        
    }

}
