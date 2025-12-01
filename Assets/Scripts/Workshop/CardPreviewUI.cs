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
        }
    }
}
