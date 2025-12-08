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
        [SerializeField] private AudioSource sfxSource;   // single SFX source routed to SFX group
        [SerializeField] private AudioSource musicSource; // routed to MUSIC group


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
            PlayOneShot(sfxSource, uiClickClip);
        }

        public void PlayUiHover()
        {
            PlayOneShot(sfxSource, uiHoverClip);
        }

        public void PlayUiError()
        {
            PlayOneShot(sfxSource, uiErrorClip);
        }

        public void PlayCardSubmit()
        {
            PlayOneShot(sfxSource, cardSubmitClip);
        }

        public void PlayCardRejected()
        {
            PlayOneShot(sfxSource, cardRejectedClip);
        }

        private void PlayOneShot(AudioSource source, AudioClip clip)
        {
            if (source == null || clip == null) return;
            source.PlayOneShot(clip);
        }
    }
}
