using Assets.Scripts.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Main_Menu
{
    public class UIButtonSound : MonoBehaviour, IPointerEnterHandler
    {
        public void PlayClick()
        {
            if (AudioManager.Instance == null) return;
            AudioManager.Instance.PlayUiClick();
            HapticsManager.Instance?.PlayClickHaptic();
        }

        public void PlayHover()
        {
            if (AudioManager.Instance == null) return;
            AudioManager.Instance.PlayUiHover();
        }

        public void PlayError()
        {
            if (AudioManager.Instance == null) return;
            AudioManager.Instance.PlayUiError();
            HapticsManager.Instance?.PlayClickHaptic();

        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            PlayHover();
        }
    }
}
