using SOHNE.Accessibility.Colorblindness;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class MainMenuManager : MonoBehaviour
{

    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject playScenePanel;

    public VideoPlayer videoPlayer;
    public VideoClip videoClip;

    public static MainMenuManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    public void Play()
    {
        SceneManager.LoadScene("PreGame");
        //SceneManager.LoadScene("Game");
    }

    public void Map()
    {
        SceneManager.LoadScene("Georeferencing");
    }

    public void Settings()
    {
        SceneManager.LoadScene("Settings", LoadSceneMode.Additive);
    }

    public void Rules()
    {
        SceneManager.LoadScene("Rules", LoadSceneMode.Additive);
    }

    public void Workshop()
    {
        SceneManager.LoadScene("Workshop", LoadSceneMode.Additive);
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
