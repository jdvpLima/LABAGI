// DeckItemUI.cs
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class DeckItemUI : MonoBehaviour, IPointerClickHandler
    {
        [Header("Child UI refs")]
        public Text deckNameText;
        public Text cardCountText;
        public Button clickableButton; // optional
        public Image backgroundImage; // optional

        // selection colors (set in inspector)
        public Color normalColor = Color.white;
        public Color selectedColor = new Color(0.8f, 0.9f, 1f);

        private object deckObject;
        private Action<object, DeckItemUI> onClick;

        public void Initialize(object deck, Action<object, DeckItemUI> onClickCallback)
        {
            deckObject = deck;
            onClick = onClickCallback;

            if (deckNameText != null) deckNameText.text = GetDeckName(deckObject) ?? $"Deck";
            if (cardCountText != null) cardCountText.text = $"{GetCardCount(deckObject)} cards";

            SetSelected(false);

            if (clickableButton != null)
            {
                clickableButton.onClick.RemoveAllListeners();
                clickableButton.onClick.AddListener(() => NotifyClick());
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            NotifyClick();
        }

        private void NotifyClick()
        {
            onClick?.Invoke(deckObject, this);
        }

        public void SetSelected(bool selected)
        {
            if (backgroundImage != null)
                backgroundImage.color = selected ? selectedColor : normalColor;
        }

        // -----------------------
        // Reflection helpers
        // -----------------------
        private string GetDeckName(object o)
        {
            if (o == null) return null;
            var v = GetMemberValue(o, "name") ?? GetMemberValue(o, "Name");
            return v?.ToString();
        }

        private int GetCardCount(object o)
        {
            if (o == null) return 0;
            var cards = GetMemberValue(o, "cards") ?? GetMemberValue(o, "Cards");
            if (cards == null) return 0;
            // If it's an ICollection or IList, get Count
            var asCollection = cards as System.Collections.ICollection;
            if (asCollection != null) return asCollection.Count;
            // fallback: try property "Count"
            var countObj = GetMemberValue(cards, "Count") ?? GetMemberValue(cards, "count");
            if (countObj != null && int.TryParse(countObj.ToString(), out var n)) return n;
            return 0;
        }

        // Generic getter: looks for property or field with given name (case-sensitive / fallback)
        private object GetMemberValue(object obj, string memberName)
        {
            if (obj == null) return null;
            var t = obj.GetType();

            // try property (case-sensitive)
            var prop = t.GetProperty(memberName, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null) return prop.GetValue(obj);

            // try field (case-sensitive)
            var field = t.GetField(memberName, BindingFlags.Public | BindingFlags.Instance);
            if (field != null) return field.GetValue(obj);

            // try case-insensitive property
            prop = t.GetProperty(memberName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop != null) return prop.GetValue(obj);

            // try case-insensitive field
            field = t.GetField(memberName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (field != null) return field.GetValue(obj);

            return null;
        }
    }
}
