// DeckListUI.cs
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class DeckListUI : MonoBehaviour
    {
        [Header("UI References")]
        public RectTransform contentParent;
        public GameObject deckItemPrefab; // prefab must have DeckItemUI
        public GameObject confirmBtn;
        public Text selectedDeckLabel;
        public GameObject emptyStateObject;

        private List<GameObject> spawnedItems = new List<GameObject>();
        private object selectedDeckObject = null;

        private void Start()
        {
            if (confirmBtn != null) confirmBtn.SetActive(false);
            if (selectedDeckLabel != null) selectedDeckLabel.text = "";

            if (contentParent == null || deckItemPrefab == null)
            {
                Debug.LogError("DeckListUI: assign contentParent and deckItemPrefab in inspector.");
                return;
            }

            // Example: If you already have DeckService, call it and forward result:
            // DeckService.Instance.FetchUserDecks(decks => PopulateDecksFromEnumerable(decks), err => ShowEmptyState(true, "Failed"));
            // But don't hard-wire DeckService here in case you call Populate... yourself.
        }

        // Public: feed any enumerable of deck objects (List<DecksDto>, List<DeckDTO>, etc.)
        public void PopulateDecksFromEnumerable(System.Collections.IEnumerable deckEnumerable)
        {
            ClearSpawnedItems();

            if (deckEnumerable == null)
            {
                ShowEmptyState(true, "No decks");
                return;
            }

            bool any = false;
            foreach (var d in deckEnumerable)
            {
                any = true;
                var go = Instantiate(deckItemPrefab, contentParent, false);
                var itemUI = go.GetComponent<DeckItemUI>();
                if (itemUI == null)
                {
                    Debug.LogError("DeckListUI: deckItemPrefab must have DeckItemUI component.");
                    Destroy(go);
                    continue;
                }

                itemUI.Initialize(d, OnDeckClicked);
                spawnedItems.Add(go);
            }

            if (!any)
            {
                ShowEmptyState(true, "No decks found");
            }
            else
            {
                ShowEmptyState(false, "");
            }
        }

        // Optional strongly-typed helper for List<Assets.Scripts.CreateDeck.DecksDto>
        public void PopulateDecks(List<Assets.Scripts.CreateDeck.DecksDto> decks)
        {
            PopulateDecksFromEnumerable(decks);
        }

        private void ClearSpawnedItems()
        {
            foreach (var g in spawnedItems) if (g != null) Destroy(g);
            spawnedItems.Clear();
            selectedDeckObject = null;
            if (selectedDeckLabel != null) selectedDeckLabel.text = "";
            if (confirmBtn != null) confirmBtn.SetActive(false);
        }

        private void ShowEmptyState(bool show, string message)
        {
            if (emptyStateObject != null) emptyStateObject.SetActive(show);
            if (emptyStateObject != null && show)
            {
                var txt = emptyStateObject.GetComponentInChildren<Text>();
                if (txt != null) txt.text = message;
            }
            if (confirmBtn != null) confirmBtn.SetActive(false);
        }

        private void OnDeckClicked(object deck, DeckItemUI clickedItem)
        {
            selectedDeckObject = deck;

            // Update label
            string name = GetDeckName(deck);
            if (selectedDeckLabel != null) selectedDeckLabel.text = $"Selected: {name}";

            // Show confirm
            if (confirmBtn != null) confirmBtn.SetActive(true);

            // Visual selection
            foreach (var g in spawnedItems)
            {
                var ui = g.GetComponent<DeckItemUI>();
                if (ui != null) ui.SetSelected(ui == clickedItem);
            }
        }

        // Called by ConfirmBtn's OnClick()
        public void OnConfirmSelection()
        {
            if (selectedDeckObject == null)
            {
                Debug.LogWarning("No deck selected.");
                return;
            }

            long id = GetDeckId(selectedDeckObject);
            string name = GetDeckName(selectedDeckObject);
            Debug.Log($"Confirmed deck id={id} name={name}");

            // TODO: store globally or proceed to next scene
            // e.g. GameSession.SelectedDeckId = id; GameSession.SelectedDeckName = name;
        }

        // -----------------------
        // Reflection helpers (similar logic as in DeckItemUI)
        // -----------------------
        private object GetMemberValue(object obj, string memberName)
        {
            if (obj == null) return null;
            var t = obj.GetType();

            var prop = t.GetProperty(memberName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (prop != null) return prop.GetValue(obj);
            var field = t.GetField(memberName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (field != null) return field.GetValue(obj);

            // case-insensitive fallback
            prop = t.GetProperty(memberName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
            if (prop != null) return prop.GetValue(obj);
            field = t.GetField(memberName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
            if (field != null) return field.GetValue(obj);

            return null;
        }

        private string GetDeckName(object deck)
        {
            var v = GetMemberValue(deck, "name") ?? GetMemberValue(deck, "Name");
            return v?.ToString() ?? "Deck";
        }

        private int GetCardCount(object deck)
        {
            var cards = GetMemberValue(deck, "cards") ?? GetMemberValue(deck, "Cards");
            if (cards == null) return 0;
            var coll = cards as System.Collections.ICollection;
            if (coll != null) return coll.Count;
            var countObj = GetMemberValue(cards, "Count") ?? GetMemberValue(cards, "count");
            if (countObj != null && int.TryParse(countObj.ToString(), out var n)) return n;
            return 0;
        }

        private long GetDeckId(object deck)
        {
            var v = GetMemberValue(deck, "id") ?? GetMemberValue(deck, "Id") ?? GetMemberValue(deck, "ID");
            if (v == null) return 0;
            if (v is long) return (long)v;
            if (v is int) return (int)v;
            if (long.TryParse(v.ToString(), out var l)) return l;
            return 0;
        }

        private void OnDestroy()
        {
            ClearSpawnedItems();
        }
    }
}
