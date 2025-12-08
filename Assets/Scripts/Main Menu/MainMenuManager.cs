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

    public Camera camera;

    public bool mainMenuAudioListener;

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

        mainMenuAudioListener = true;
    }

    public void Play()
    {
        if (ARModeSwitcher.Instance != null && ARModeSwitcher.ar_active)
        {
            FindAnyObjectByType<ARSceneLoader>().LoadSceneAtPosition("PreGame");
        }
        else
            SceneManager.LoadScene("PreGame");
        //SceneManager.LoadScene("Game");
    }

    public void Map()
    {
        SceneManager.LoadScene("Georeferencing");

        /*if (ARModeSwitcher.Instance != null && ARModeSwitcher.ar_active)
        {
            FindAnyObjectByType<ARSceneLoader>().LoadSceneAtPosition("Georeferencing");
        }
        else
            SceneManager.LoadScene("Georeferencing", LoadSceneMode.Additive);*/
    }

    public void Settings()
    {
        if (ARModeSwitcher.Instance != null && ARModeSwitcher.ar_active) {
            FindAnyObjectByType<ARSceneLoader>().LoadSceneAtPosition("Settings");
        } else
            SceneManager.LoadScene("Settings", LoadSceneMode.Additive);
    }

    public void Rules()
    {
        if (ARModeSwitcher.Instance != null && ARModeSwitcher.ar_active)
        {
            FindAnyObjectByType<ARSceneLoader>().LoadSceneAtPosition("Rules");
        }
        else
            SceneManager.LoadScene("Rules", LoadSceneMode.Additive);
    }

    public void Workshop()
    {
        if (ARModeSwitcher.Instance != null && ARModeSwitcher.ar_active)
        {
            FindAnyObjectByType<ARSceneLoader>().LoadSceneAtPosition("Workshop");
        }
        else
            SceneManager.LoadScene("Workshop", LoadSceneMode.Additive);
    }

    public void Shop()
    {
        playScenePanel.SetActive(false);
        shopPanel.SetActive(true);

    }

    public void OnButtonPressed()
    {
        Haptics.Vibrate();
    }

    public void ToggleAudioListener(bool audioListener)
    {
        camera.GetComponent<AudioListener>().gameObject.SetActive(audioListener);
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
