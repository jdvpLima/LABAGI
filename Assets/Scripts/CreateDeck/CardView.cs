using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class CardView : MonoBehaviour
{
    [SerializeField] private Image cardImage;

    public long cardId;
    private Transform initialParent;
    private Transform targetParent;
    private bool isAtTarget = false;

    public TextMeshProUGUI titleText;
    public TextMeshProUGUI suit;
    public TextMeshProUGUI rarity;
    public TextMeshProUGUI points;
    public TextMeshProUGUI subText;
    public TextMeshProUGUI flavourText;
    public TextMeshProUGUI actionsText;
    public VideoPlayer videoPlayer;

    [SerializeField] private RawImage suitImage;
    

    public Button button;

    public Card card;

    [Header("Suit videos")]
    [SerializeField] private List<VideoClip> suitClips = new();
    private Dictionary<string, VideoClip> _clipBySuit;

    void Awake()
    {
        //button = GetComponent<Button>();
        _clipBySuit = suitClips
                .Where(c => c != null)
                .GroupBy(c => c.name, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
    }
    

    public void Init(Card card, Transform initial, Transform target)
    {
        this.card = card;

        cardId = card.CardId;
        initialParent = initial;
        targetParent = target;
        isAtTarget = false;
        //LoadImage();
        transform.SetParent(initialParent);
        transform.localPosition = Vector3.zero;

        titleText.text = card.Name;

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


        button.onClick.AddListener(OnClick);


    }

    public void setVideo()
    {
        RenderTexture rt = new RenderTexture(300, 300, 24);
        rt.Create();

        videoPlayer.targetTexture = rt;
        suitImage.texture = rt;

        videoPlayer.clip = _clipBySuit.ContainsKey(card.Suit.ToLower()) ? _clipBySuit[card.Suit.ToLower()] : null;
    }

    


    // Chame pelo botão ou clique
    public void OnClick()
    {
        if (isAtTarget)
        {
            // volta para o inicial
            MoveTo(initialParent);
            //transform.localPosition = Vector3.zero;
           
            isAtTarget = false;
        }
        else
        {
            // vai para o target
           MoveTo(targetParent);
            
            //transform.localPosition = Vector3.zero;
            isAtTarget = true;
        }
    }


    public void MoveTo(Transform newParent)
    {
        
        transform.SetParent(newParent, false);
        StartCoroutine(RefreshLater(newParent));
    }

    IEnumerator RefreshLater(Transform newParent)
    {
        yield return null; // espera 1 frame
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(
            newParent.GetComponent<RectTransform>()
        );
    }


    public void Initialize(long id, Transform initial, Transform target)
    {
        cardId = id;
        initialParent = initial;
        targetParent = target;
        isAtTarget = false;
        //LoadImage();
        transform.SetParent(initialParent);
        transform.localPosition = Vector3.zero;
    }

    /*
    private void LoadImage()
    {
        // Caminho dentro de Resources
        //string path = $"Cards/{cardId}";
        string path = $"Cards/sample_card";

        Texture2D texture = Resources.Load<Texture2D>(path);

        if (texture != null)
        {
            // Converte Texture2D em Sprite
            Sprite sprite = Sprite.Create(texture,
                                          new Rect(0, 0, texture.width, texture.height),
                                          new Vector2(0.5f, 0.5f));
            cardImage.sprite = sprite;
        }
        else
        {
            Debug.LogWarning($"Imagem com ID {cardId} não encontrada em Resources/Images");
        }
    }*/

}
