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
        Debug.Log("Workshop!");
        //SceneManager.LoadScene("Workshop");
    }

    public void Shop()
    {
        playScenePanel.SetActive(false);
        shopPanel.SetActive(true);

    }

    public void Quit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
