using UnityEngine;
using UnityEngine.SceneManagement;
public class MenuButtonsRules : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void PlayGame()
    {
        SceneManager.LoadScene("Game");
    }
    public void QuitPreGame()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
