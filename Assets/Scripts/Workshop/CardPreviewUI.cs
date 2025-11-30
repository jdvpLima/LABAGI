using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Workshop
{
    public class CardPreviewUI : MonoBehaviour
    {
        [SerializeField] private Text titleText;
        [SerializeField] private Text suitText;
        [SerializeField] private Text rarityText;
        [SerializeField] private Text pointsText;
        [SerializeField] private Text abilityText;
        [SerializeField] private Text flavorText;

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

            titleText.text = string.IsNullOrEmpty(dto.name) ? "(sem nome)" : dto.name;
            suitText.text = dto.suit;
            rarityText.text = dto.rarity;
            pointsText.text = dto.points.ToString();
            abilityText.text = dto.ability;
            flavorText.text = dto.flavorText;
        }
    }
 }
