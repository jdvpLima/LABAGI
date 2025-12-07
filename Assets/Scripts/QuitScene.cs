using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

namespace Assets.Scripts
{
    public class QuitScene : MonoBehaviour
    {
        [SerializeField] private string fallbackSceneName = "MainMenu";

        // Call this from the Button OnClick
        public void UnloadCurrentScene()
        {
            Scene thisScene = gameObject.scene;
            MainMenuManager.Instance.videoPlayer.clip = MainMenuManager.Instance.videoClip;

            if (SceneManager.sceneCount > 1)
            {
                SceneManager.UnloadSceneAsync(thisScene);
                return;
            }

            if (!string.IsNullOrEmpty(fallbackSceneName))
            {
                SceneManager.LoadScene(fallbackSceneName);
            }
            else
            {
                Debug.LogWarning(
                    "[CloseSceneButton] Não posso descarregar a última cena " +
                    "e nenhum fallbackSceneName foi definido."
                );
            }
        }

    }
}
