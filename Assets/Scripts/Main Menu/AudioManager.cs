using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;

namespace Assets.Scripts.Main_Menu
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Mixer")]
        [SerializeField] private AudioMixer audioMixer;

        [Header("Sources")]
        [SerializeField] private AudioSource uiAudioSource;
        [SerializeField] private AudioSource sfxAudioSource;

        [Header("UI Clips")]
        [SerializeField] private AudioClip uiClickClip;
        [SerializeField] private AudioClip uiHoverClip;
        [SerializeField] private AudioClip uiErrorClip;

        [Header("Game Clips")]
        [SerializeField] private AudioClip cardSubmitClip;
        [SerializeField] private AudioClip cardRejectedClip;
        // adiciona mais conforme precisares

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void PlayUiClick()
        {
            PlayOneShot(uiAudioSource, uiClickClip);
        }

        public void PlayUiHover()
        {
            PlayOneShot(uiAudioSource, uiHoverClip);
        }

        public void PlayUiError()
        {
            PlayOneShot(uiAudioSource, uiErrorClip);
        }

        public void PlayCardSubmit()
        {
            PlayOneShot(sfxAudioSource, cardSubmitClip);
        }

        public void PlayCardRejected()
        {
            PlayOneShot(sfxAudioSource, cardRejectedClip);
        }

        private void PlayOneShot(AudioSource source, AudioClip clip)
        {
            if (source == null || clip == null) return;
            source.PlayOneShot(clip);
        }
    }
}
