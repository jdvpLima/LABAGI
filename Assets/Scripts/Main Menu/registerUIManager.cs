using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class RegisterUIManager : MonoBehaviour
{
    [SerializeField] private GameObject playScenePanel;

  
    [Header("Refs")]
    [SerializeField] private AuthApiClient apiClient;
    [SerializeField] private TMP_Text infoText;

    [Header("Config")]
    [SerializeField] private float defaultPollIntervalSeconds = 5f;

    private string _loginRequestId;
    private Coroutine _pollCoroutine;
    private string _sessionToken;

    private const string SessionTokenKey = "sessionToken";

    private void Start()
    {
        if (infoText != null)
            infoText.text = "Click the button to login with Google.";

        // Se quiseres tentar auto-login no futuro:
        if (PlayerPrefs.HasKey(SessionTokenKey))
        {
            _sessionToken = PlayerPrefs.GetString(SessionTokenKey);
            Debug.Log("[RegisterUIManager] Found existing sessionToken in PlayerPrefs.");
        }
        else
        {
            Debug.Log("[RegisterUIManager] No existing sessionToken found.");
        }
    
    }

    // Ligado ao botão "Login/Register with Google"
    public void OnLoginWithGoogleClicked()
    {
        Debug.Log("[RegisterUIManager] OnLoginWithGoogleClicked()");
        if (infoText != null)
            infoText.text = "Starting Google login...";

        StartCoroutine(apiClient.StartGoogleDeviceFlow(
            onSuccess: OnDeviceFlowStarted,
            onError: err =>
            {
                Debug.LogError("[RegisterUIManager] Error starting device flow: " + err);
                if (infoText != null)
                    infoText.text = "Error starting Google login: " + err;
            }));
    }

    private void OnDeviceFlowStarted(DeviceStartResponseDTO resp)
    {

        Debug.Log("[RegisterUIManager] Device flow started. loginRequestId=" + resp.loginRequestId +
                  " userCode=" + resp.userCode +
                  " verificationUrl=" + resp.verificationUrl);

        _loginRequestId = resp.loginRequestId;

        // Abre o browser na página da Google (PC: Chrome/Edge, Quest: Oculus Browser)
        Application.OpenURL(resp.verificationUrl);

        // Copia o código para o clipboard do sistema (PC, Android/Quest, etc.)
        GUIUtility.systemCopyBuffer = resp.userCode;

        Debug.Log("[RegisterUIManager] Opened browser and copied userCode to clipboard.");

        if (infoText != null)
        {
            infoText.text =
                "We opened the Google login page in your browser.\n\n" +
                "The code has been copied to your clipboard.\n" +
                $"When the browser asks for a code, paste or type:\n[ {resp.userCode} ]";
        }

        if (_pollCoroutine != null)
            StopCoroutine(_pollCoroutine);

        var interval = resp.pollInterval > 0
        ? resp.pollInterval
        : (int)defaultPollIntervalSeconds;

        Debug.Log("[RegisterUIManager] Starting PollLoop with interval=" + interval + " seconds.");

        _pollCoroutine = StartCoroutine(PollLoop(interval));
    }

    private IEnumerator PollLoop(int intervalSeconds)
    {
        while (true)
        {
            yield return new WaitForSeconds(intervalSeconds);
            Debug.Log("[RegisterUIManager] PollLoop tick. Requesting status for loginRequestId=" + _loginRequestId);

            if (infoText != null)
                infoText.text = "\nChecking login status...";

            yield return apiClient.PollGoogleDeviceFlow(
                _loginRequestId,
                onSuccess: OnPollResult,
                onError: err =>
                {
                    Debug.LogError("[RegisterUIManager] Poll error: " + err);

                    if (infoText != null)
                        infoText.text = "Error checking status: " + err;
                });
        }
    }

    private void OnPollResult(DevicePollResponseDTO resp)
    {
        Debug.Log("[RegisterUIManager] Poll result: status=" + resp.status);

        switch (resp.status)
        {
            case "pending":
                // Nada de especial, ainda à espera do utilizador no browser
                Debug.Log("[RegisterUIManager] Login still pending.");

                break;

            case "ok":
                Debug.Log("[RegisterUIManager] Login completed. Storing sessionToken.");

                if (_pollCoroutine != null)
                {
                    StopCoroutine(_pollCoroutine);
                    _pollCoroutine = null;
                }

                _sessionToken = resp.sessionToken;
                PlayerPrefs.SetString(SessionTokenKey, _sessionToken);
                PlayerPrefs.Save();

                if (infoText != null)
                {
                    var name = string.IsNullOrWhiteSpace(resp.displayName)
                        ? "Player"
                        : resp.displayName;
                    infoText.text = $"Logged in as {name}.";
                }

                playScenePanel.SetActive(true);
                gameObject.SetActive(false);

                break;

            case "expired":
                Debug.LogWarning("[RegisterUIManager] Device code expired.");

                if (_pollCoroutine != null)
                {
                    StopCoroutine(_pollCoroutine);
                    _pollCoroutine = null;
                }

                if (infoText != null)
                    infoText.text = "Code expired. Please click the button again.";
                break;

            case "error":
                Debug.LogError("[RegisterUIManager] Login error. message=" + (resp.message ?? "null"));

                if (_pollCoroutine != null)
                {
                    StopCoroutine(_pollCoroutine);
                    _pollCoroutine = null;
                }

                if (infoText != null)
                    infoText.text = "Login error: " + (resp.message ?? "unknown error");
                break;

            default:
                Debug.LogWarning("[RegisterUIManager] Unexpected status: " + resp.status);

                if (infoText != null)
                    infoText.text = "Unexpected status: " + resp.status;
                break;
        }
    }

    public string GetSessionToken() => _sessionToken;
}
