using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleButton : MonoBehaviour
{
    public void LoadScene()
    {

        if (ARModeSwitcher.ar_active)
        {
            FindAnyObjectByType<ARSceneLoader>().LoadSceneAtPosition("MainMenu");
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }

    }
}
