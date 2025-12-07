using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Settings
{
    public class ColorblindManager : MonoBehaviour
    {
        private const string COLORBLIND_MODE_KEY = "ColorblindMode";

        public static ColorblindManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;

            // aplica o modo guardado à primeira scene
            ApplyCurrentMode();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ApplyCurrentMode();
        }

        public static void ApplyCurrentMode()
        {
            if (Instance == null)
                return;

            int savedMode = PlayerPrefs.GetInt(COLORBLIND_MODE_KEY, 0);
        }

        
    }

}
