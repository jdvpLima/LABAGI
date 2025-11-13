using UnityEngine;

public class registerUIManager : MonoBehaviour
{
    [SerializeField] private GameObject playScenePanel;

   public void Register()
    {
        playScenePanel.SetActive(true);
        gameObject.SetActive(false);
    }
}
