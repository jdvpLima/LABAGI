using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class GameResultsUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI outcomeText; 
    public TextMeshProUGUI myScoreText;
    public TextMeshProUGUI myFlexText;
    public TextMeshProUGUI myBurnoutText;
    public TextMeshProUGUI myTokensText;
    public TextMeshProUGUI opponentScoreText;

    private void Start()
    {
        // Read the data we saved in the storage
        if (outcomeText != null) outcomeText.text = MatchResultsStorage.GameOutcome;
        
        if (myScoreText != null) myScoreText.text = $"Score: {MatchResultsStorage.MyScore}";
        if (myFlexText != null) myFlexText.text = $"Flexibility: {MatchResultsStorage.MyFlexibility}";
        if (myBurnoutText != null) myBurnoutText.text = $"Burnout: {MatchResultsStorage.MyBurnout}";
        if (myTokensText != null) myTokensText.text = $"Tokens Left: {MatchResultsStorage.MyTokens}";
        
        if (opponentScoreText != null) opponentScoreText.text = $"Opponent Score: {MatchResultsStorage.OpponentScore}";
        
    }

    public void OnMainMenuClicked()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
            Destroy(NetworkManager.Singleton.gameObject);
        }
        
        SceneManager.LoadScene("MainMenu"); 
    }
}