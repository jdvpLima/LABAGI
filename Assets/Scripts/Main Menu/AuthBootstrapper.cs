using System.Collections;
using TMPro;
using UnityEngine;

public class AuthBootstrapper : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private AuthApiClient apiClient;
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject mainPanel; 
    [SerializeField] private TMP_Text statusText;

    public const string SessionTokenKey = "sessionToken";
    public const string UserIdKey = "userId";

    public static long CurrentUserId { get; private set; }

    private void Awake()
    {
        // para este objeto sobreviver a troca de cenas
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Debug.Log("[AuthBootstrapper] Start()");

        // No arranque, escondemos tudo e decidimos a seguir
        if (loginPanel != null) loginPanel.SetActive(false);
        if (mainPanel != null) mainPanel.SetActive(false);

        var token = PlayerPrefs.GetString(SessionTokenKey, null);

        if (string.IsNullOrEmpty(token))
        {
            Debug.Log("[AuthBootstrapper] No saved session token. Showing login.");
            if (statusText != null)
                statusText.text = "Please login with Google.";
            ShowLogin();
            return;
        }
        else
        {
            Debug.Log("[AuthBootstrapper] Found sessionToken in PlayerPrefs. Validating with backend...");

            // Já tem token guardado → validar no backend
            StartCoroutine(CheckExistingSession(token));
        }
    }

    
    private IEnumerator CheckExistingSession(string sessionToken)
    {
        Debug.Log("[AuthBootstrapper] Checking existing session...");

        yield return apiClient.GetMe(
            sessionToken,
            onSuccess: me =>
            {
                Debug.Log("[AuthBootstrapper] /api/Me OK. User id = " + me.id);

                CurrentUserId = me.id;

                PlayerPrefs.SetString(SessionTokenKey, sessionToken);
                PlayerPrefs.SetString(UserIdKey, me.id.ToString());
                PlayerPrefs.Save();

                if (statusText != null)
                    statusText.text = $"Welcome back, {me.displayName}!";

                ShowMain();
            },
            onUnauthorized: () =>
            {
                Debug.LogWarning("[AuthBootstrapper] Session invalid/expired.");
                if (statusText != null)
                    statusText.text = "Session expired. Please login with Google.";
                ShowLogin();
            },
            onError: err =>
            {
                Debug.LogError("[AuthBootstrapper] Error calling /api/Me: " + err);
                if (statusText != null)
                    statusText.text = "Error checking session: " + err;
                ShowLogin();
            }
        );
    }
    public void OnLoginCompleted(string sessionToken, long userId, string displayName, string email)
    {
        Debug.Log("[AuthBootstrapper] OnLoginCompleted() userId=" + userId);

        CurrentUserId = userId;

        PlayerPrefs.SetString(SessionTokenKey, sessionToken);
        PlayerPrefs.SetString(UserIdKey, userId.ToString());
        PlayerPrefs.Save();

        if (statusText != null)
            statusText.text = $"Welcome, {displayName}!";

        ShowMain();
    }
    private void ShowLogin()
    {
        Debug.Log("[AuthBootstrapper] ShowLogin()");

        if (loginPanel != null) loginPanel.SetActive(true);
        if (mainPanel != null) mainPanel.SetActive(false);
    }

    private void ShowMain()
    {
        Debug.Log("[AuthBootstrapper] ShowMain()");

        if (loginPanel != null) loginPanel.SetActive(false);
        if (mainPanel != null) mainPanel.SetActive(true);
    }

}
