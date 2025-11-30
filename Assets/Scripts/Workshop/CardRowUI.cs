using Assets.Scripts.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Workshop
{
    public class CardRowUI: MonoBehaviour
    {
        [SerializeField] private Text nameText;
        [SerializeField] private Text suitText;
        [SerializeField] private Text rarityText;
        [SerializeField] private Text pointsText;
        [SerializeField] private Button selectButton;

        public void Setup(CardDto card, Action onClick)
        {
            if (card == null)
            {
                nameText.text = "";
                suitText.text = "";
                rarityText.text = "";
                pointsText.text = "";
                selectButton.onClick.RemoveAllListeners();
                return;
            }

            nameText.text = string.IsNullOrEmpty(card.name) ? "(no name)" : card.name;
            suitText.text = card.suit;
            rarityText.text = card.rarity;
            pointsText.text = card.points.ToString();

            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(() => onClick?.Invoke());
        }
    }
}
