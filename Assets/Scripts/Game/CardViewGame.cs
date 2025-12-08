using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class CardViewGame : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI suit;
    public TextMeshProUGUI rarity;
    public TextMeshProUGUI points;
    public TextMeshProUGUI subText;
    public TextMeshProUGUI flavourText;
    public TextMeshProUGUI actionsText;
    public Button button;
    public VideoPlayer videoPlayer;
    [SerializeField] private AspectRatioFitter suitRatioFitter;

    [Header("Suit videos")]
    [SerializeField] private List<VideoClip> suitClips = new();
    private Dictionary<string, VideoClip> _clipBySuit;
    [SerializeField] private RawImage suitImage;

    public Card card;
    private Player owner;

    public event Action<CardViewGame> OnCardClicked;

    private void Awake()
    {
        _clipBySuit = suitClips
                .Where(c => c != null)
                .GroupBy(c => c.name, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
    }

    public void Init(Card card, Player owner)
    {
        this.card = card;
        this.owner = owner;

        titleText.text = card.Name;
        //subText.text = card.Suit;
        suit.text = card.Suit;
        rarity.text = card.Rarity;
        points.text = card.Points.ToString();
        flavourText.text = card.FlavourText;

        setVideo();

        // --- FIX: Check for Null Actions ---
        if (card.Actions != null && card.Actions.Count > 0)
        {
            actionsText.text = string.Join("\n", card.Actions);
        }
        else
        {
            actionsText.text = ""; // Empty string if null
        }
        // -----------------------------------

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            OnCardClicked?.Invoke(this);
            owner.PickCard(this);
            Debug.Log("Selected card: " + card.Name);
        });
    }


    public void setVideo()
    {
        RenderTexture rt = new RenderTexture(300, 300, 24);
        rt.Create();

        videoPlayer.targetTexture = rt;
        suitImage.texture = rt;

        videoPlayer.clip = _clipBySuit.ContainsKey(card.Suit.ToLower()) ? _clipBySuit[card.Suit.ToLower()] : null;
    }

}