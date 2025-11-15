using System.Collections;
using TMPro;
using UnityEngine;

public class AuthBootstrapper : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private AuthApiClient apiClient;
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject mainPanel; // menu principal / shop
    [SerializeField] private TMP_Text statusText;

    private const string SessionTokenKey = "sessionToken";

    private void Start()
    {
        Debug.Log("[AuthBootstrapper] Start()");

        // No arranque, escondemos tudo e decidimos a seguir
        if (loginPanel != null) loginPanel.SetActive(false);
        if (mainPanel != null) mainPanel.SetActive(false);

        if (statusText != null)
            statusText.text = "Checking login status...";

        var token = PlayerPrefs.GetString(SessionTokenKey, null);

        if (string.IsNullOrEmpty(token))
        {
            Debug.Log("[AuthBootstrapper] No sessionToken in PlayerPrefs. Showing login.");

            // Nunca fez login → mostrar painel de login
            ShowLogin();
        }
        else
        {
            Debug.Log("[AuthBootstrapper] Found sessionToken in PlayerPrefs. Validating with backend...");

            // Já tem token guardado → validar no backend
            StartCoroutine(CheckExistingToken(token));
        }
    }

    private IEnumerator CheckExistingToken(string token)
    {
        yield return apiClient.GetMe(
            token,
            onSuccess: me =>
            {
                Debug.Log("[AuthBootstrapper] /me success. UserId=" + me.id + " Handle=" + me.handle);

                // Token válido
                if (statusText != null)
                    statusText.text = $"Welcome back, {me.displayName ?? me.handle}!";

                ShowMain();
            },
            onUnauthorized: () =>
            {
                Debug.LogWarning("[AuthBootstrapper] /me unauthorized. Clearing token and showing login.");

                // Token inválido/expirado → limpar e pedir login de novo
                PlayerPrefs.DeleteKey(SessionTokenKey);
                PlayerPrefs.Save();

                if (statusText != null)
                    statusText.text = "Session expired. Please login with Google.";

                ShowLogin();
            },
            onError: err =>
            {
                Debug.LogError("[AuthBootstrapper] Error calling /me: " + err);

                // Erro de rede ou outra coisa → podes decidir o que fazer
                if (statusText != null)
                    statusText.text = "Error checking session: " + err;

                // Em caso de dúvida, mostra login
                ShowLogin();
            });
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
