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
    public class DraftRowUI : MonoBehaviour
    {
        [SerializeField] private Text nameText;
        [SerializeField] private Button selectButton;

        public void Setup(CardDto draft, Action onClick)
        {
            if (draft == null)
            {
                nameText.text = "";
                selectButton.onClick.RemoveAllListeners();
                return;
            }

            nameText.text = string.IsNullOrEmpty(draft.name) ? "(sem nome)" : draft.name;

            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(() => onClick?.Invoke());
        }
    }
}

