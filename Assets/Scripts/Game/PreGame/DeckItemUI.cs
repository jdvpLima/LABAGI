using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Assets.Scripts.CreateDeck; // for DecksDto

[RequireComponent(typeof(Image))] // ensures there is a Graphic to receive pointer events
public class DeckItemUI : MonoBehaviour, IPointerClickHandler
{
    [Header("UI refs")]
    public Image artworkImage;                  // child image for deck artwork (optional)
    public TextMeshProUGUI titleText;           // display-only text (assign your TMP text object)
    public Image backgroundImage;               // root background image (for selection highlight)

    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color selectedColor = new Color(0.8f, 0.9f, 1f);

    private DecksDto deck;
    private Action<DecksDto, DeckItemUI> onClick;

    // Bind called by the list manager after instantiation/clone
    public void Bind(DecksDto d, Action<DecksDto, DeckItemUI> onClickCallback)
    {
        deck = d;
        onClick = onClickCallback;

        titleText.text = string.IsNullOrEmpty(d.name) ? $"Deck {d.id}" : d.name;

        // artworkImage left empty unless you populate it via a sprite field or URL loader
        // e.g., artworkImage.sprite = someSprite;
        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        if (backgroundImage != null)
            backgroundImage.color = selected ? selectedColor : normalColor;
    }

    // IPointerClickHandler receives clicks anywhere on the item's Image (so ensure RaycastTarget = true)
    public void OnPointerClick(PointerEventData eventData)
{
    Debug.Log("Item Clicked!"); // <--- Add this
    onClick?.Invoke(deck, this);
}
}
