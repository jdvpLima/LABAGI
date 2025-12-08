using Assets.Scripts.Model;
using Assets.Scripts.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Assets.Scripts.Workshop
{
    public class CardPreviewUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text suitText;
        [SerializeField] private TMP_Text rarityText;
        [SerializeField] private TMP_Text pointsText;
        [SerializeField] private TMP_Text abilityText;
        [SerializeField] private TMP_Text flavorText;
        [SerializeField] private RawImage suitImg;
        //[SerializeField] private AspectRatioFitter suitRatioFitter;
        [SerializeField] private VideoPlayer suitVideo;

        [Header("Suit videos")]
        [SerializeField] private List<VideoClip> suitClips = new();
        private Dictionary<string, VideoClip> _clipBySuit;

        private void Awake()
        {
            // Mapa nome-do-clip -> VideoClip (case-insensitive)
            _clipBySuit = suitClips
                .Where(c => c != null)
                .GroupBy(c => c.name, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
        }
        public void UpdatePreview(WorkshopCardDTO dto)
        {
            if (dto == null)
            {
                titleText.text = "";
                suitText.text = "";
                rarityText.text = "";
                pointsText.text = "";
                abilityText.text = "";
                flavorText.text = "";
                return;
            }

            titleText.text = string.IsNullOrEmpty(dto.name) ? "(no name)" : dto.name;
            suitText.text = dto.suit;
            rarityText.text = dto.rarity;
            pointsText.text = dto.points.ToString();
            abilityText.text = dto.ability;
            flavorText.text = dto.flavorText;

            UpdateSuitVideo(dto.suit);

        }
        private void UpdateSuitVideo(string suit)
        {
            if (string.IsNullOrWhiteSpace(suit) || _clipBySuit == null)
            {
                suitVideo.clip = null;
                return;
            }

            // tentar com suit e também com ToLower só para garantir
            if (!_clipBySuit.TryGetValue(suit, out var clip) &&
                !_clipBySuit.TryGetValue(suit.ToLower(), out clip))
            {
                suitVideo.clip = null;
                return;
            }

            suitVideo.clip = clip;

            var settings = PersistentSettingsManager.Instance;
            bool lowSensory = settings != null && settings.lowSensoryModeEnabled;

            if (lowSensory)
            {
                // mostrar só a primeira frame, sem loop
                suitVideo.playOnAwake = false;
                suitVideo.isLooping = false;

                // corrutina para preparar e “captar” o primeiro frame
                StartCoroutine(ShowFirstFrameStatic());
            }
            else
            {
                // modo normal: vídeo em loop
                suitVideo.isLooping = true;
                suitVideo.Play();
            }
        }

        private System.Collections.IEnumerator ShowFirstFrameStatic()
        {
            if (suitVideo.clip == null)
                yield break;

            suitVideo.Prepare();

            // espera até o vídeo estar preparado
            while (!suitVideo.isPrepared)
                yield return null;

            // toca um pouco para renderizar o primeiro frame
            suitVideo.Play();
            yield return null; // 1 frame
            suitVideo.Pause();

            // opcional: tentar garantir que está no frame 0
            try
            {
                suitVideo.frame = 0;
            }
            catch { }
        }

    }
}
