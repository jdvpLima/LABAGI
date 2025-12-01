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
    public class DraftRowUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private Button selectButton;

        public void Setup(WorkshopCardDTO draft, Action onClick)
        {
            if (draft == null)
            {
                nameText.text = "";
                selectButton.onClick.RemoveAllListeners();
                return;
            }

            var label = string.IsNullOrEmpty(draft.name) ? "(sem nome)" : draft.name;
            if (!string.IsNullOrEmpty(draft.status))
                label += $" [{draft.status}]";

            nameText.text = label;

            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(() => onClick?.Invoke());
        }
    }
}

