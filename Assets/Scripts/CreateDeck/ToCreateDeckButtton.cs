using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ToCreateDeckButtton : MonoBehaviour
{
    private Button btn;

    void Awake()
    {
        btn = GetComponent<Button>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        btn.onClick.AddListener(GoToCreateDeck);
    }

    private void GoToCreateDeck()
    {
        SceneManager.LoadScene("DeckCreation",LoadSceneMode.Additive);
    }
}
