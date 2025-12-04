using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject playScenePanel;

    public static bool workshop = false;

    [SerializeField] private GameObject video;


    void Update()
    {
        if(workshop == true)
            video.SetActive(false);
        else
            video.SetActive(true);
    }

    public void Play()
    {
        //SceneManager.LoadScene("PreGame");
        SceneManager.LoadScene("Game");
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
        workshop = true;
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
