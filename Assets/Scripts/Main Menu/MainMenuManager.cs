using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject playScenePanel;

    public void Play()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void Settings()
    {
        Debug.Log("Settings!");
    }

    public void Rules()
    {
        Debug.Log("Rules!");
    }

    public void Workshop()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 2);
    }

    public void Shop()
    {
        playScenePanel.SetActive(false);
        shopPanel.SetActive(true);

    }

    public void Quit()
    {
        // Logout simples: limpar token guardado
        Debug.Log("[MainMenuManager] Logout()");
        PlayerPrefs.DeleteKey(AuthBootstrapper.SessionTokenKey);
        PlayerPrefs.Save();
        Debug.Log("[MainMenuManager] Session token cleared on Quit.");

        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
