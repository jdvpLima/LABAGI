using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;

namespace Assets.Scripts.Settings
{
    [RequireComponent(typeof(VideoPlayer))]
    public class LowSensoryVideoController : MonoBehaviour
    {
        private VideoPlayer _video;
        private PersistentSettingsManager _settings;
        private Coroutine _waitCoroutine;

        private void Awake()
        {
            _video = GetComponent<VideoPlayer>();
        }

        private void OnEnable()
        {
            _waitCoroutine = StartCoroutine(WaitForSettingsAndSubscribe());
        }

        private void OnDisable()
        {
            if (_waitCoroutine != null)
            {
                StopCoroutine(_waitCoroutine);
                _waitCoroutine = null;
            }

            if (_settings != null)
            {
                _settings.OnLowSensoryModeChanged -= ApplyLowSensory;
            }
        }

        private IEnumerator WaitForSettingsAndSubscribe()
        {
            while (PersistentSettingsManager.Instance == null)
            {
                yield return null;
            }

            _settings = PersistentSettingsManager.Instance;

            // evitar registos duplicados
            _settings.OnLowSensoryModeChanged -= ApplyLowSensory;
            _settings.OnLowSensoryModeChanged += ApplyLowSensory;

            // aplica logo o estado atual
            ApplyLowSensory(_settings.lowSensoryModeEnabled);
        }

        private void Update()
        {
            // Guarda-chuva: se o modo estiver ativo, nunca deixa o vídeo tocar
            if (_settings == null || _video == null)
                return;

            if (_settings.lowSensoryModeEnabled && _video.isPlaying)
            {
                _video.Pause();
                try { _video.frame = 0; } catch { }
            }
        }

        private void ApplyLowSensory(bool lowSensory)
        {
            if (_video == null)
                return;

            if (lowSensory)
            {
                if (_video.isPlaying)
                    _video.Pause();

                _video.playOnAwake = false;
                _video.isLooping = false;

                try
                {
                    _video.frame = 0;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[LowSensoryVideoController] Erro ao definir frame = 0 em {gameObject.name}: {e.Message}");
                }
            }
            else
            {
                _video.Play();
            }
        }
    }
}
