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
        [SerializeField] private AspectRatioFitter suitRatioFitter;
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
            if (suitImg.texture != null)
            {
                float w = suitImg.texture.width;
                float h = suitImg.texture.height;
                suitRatioFitter.aspectRatio = w / h;
            }
            if (string.IsNullOrWhiteSpace(suit) || _clipBySuit == null)
            {
                suitVideo.clip = null;
                return;
            }

            if (_clipBySuit.TryGetValue(suit, out var clip))
            {
                suitVideo.clip = clip;
                suitVideo.isLooping = true; // opcional
                suitVideo.Play();
            }
            else
            {
                suitVideo.clip = null;
            }
        }
    }
}
