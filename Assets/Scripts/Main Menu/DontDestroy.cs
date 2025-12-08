using UnityEngine;
using UnityEngine.EventSystems;

public class DontDestroy : MonoBehaviour
{
    private void Awake()
    {
        if (this.gameObject == EventSystem.current.gameObject)
        {
            var all = FindObjectsOfType<EventSystem>();

            // já existe outro EventSystem na cena (provavelmente o que veio de outra scene)?
            if (all.Length > 1)
            {
                // destrói esta cópia nova
                Destroy(gameObject);
            }           
        } 

        DontDestroyOnLoad(gameObject);
    }
}
