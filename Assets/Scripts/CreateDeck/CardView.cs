using Assets.Scripts.Service;
using Assets.Scripts.Workshop;
using NUnit.Framework.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using TMPro;
using UnityEditor.Experimental.GraphView;
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

    [Serializable]
    private class ParsedCardActions
    {
        // C# field names must match the JSON keys exactly.

        public int amount;
        public string effect;
        public string target;
        public string trigger;

        // Note: JsonUtility is case-sensitive! 
        // If the JSON key is "oncePerGame", the C# field must also be "oncePerGame".
        public bool oncePerGame;
    }

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
            // Join all of the strings from actions to form a valid JSON object
            // And use the AbilityTextBuilder from Workshop to parse it
            var actionsJsonString = string.Join(", ", card.Actions);

            var parsedJsonActions = JsonUtility.FromJson<ParsedCardActions>(actionsJsonString);
            // Construir um CardDto temporário só para alimentar os builders
            var cardForAbility = new WorkshopCardDTO
            {
                id = 0,
                name = "",
                suit = "",
                rarity = "",
                points = 2,
                ability = null, // vai ser preenchido pelos builders
                trigger = parsedJsonActions.trigger,
                effect = parsedJsonActions.effect,
                amount = parsedJsonActions.amount,
                target = parsedJsonActions.target,
                oncePerGame = parsedJsonActions.oncePerGame,
                abilityJson = null,
                expansionCode = "wks",
                flavorText = "",
                status = "",
            };

            string humanReadableAbility = AbilityTextBuilder.Build(cardForAbility);
            actionsText.text = humanReadableAbility;
            //actionsText.text = string.Join("\n", card.Actions);
        }
        else
        {
            actionsText.text = ""; // Empty string if null
        }


        button.onClick.AddListener(OnClick);


    }

    public void setVideo()
    {
        // Criar RenderTexture (podes otimizar isto no futuro para não criar sempre uma nova)
        RenderTexture rt = new RenderTexture(300, 300, 24);
        rt.Create();

        videoPlayer.targetTexture = rt;
        suitImage.texture = rt;

        if (!_clipBySuit.TryGetValue(card.Suit.ToLower(), out var clip))
        {
            videoPlayer.clip = null;
            return;
        }

        videoPlayer.clip = clip;

        var settings = PersistentSettingsManager.Instance;
        bool lowSensory = settings != null && settings.lowSensoryModeEnabled;

        if (lowSensory)
        {
            // só primeira frame, sem loop
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = false;
            StartCoroutine(ShowFirstFrameStatic());
        }
        else
        {
            // modo normal
            videoPlayer.isLooping = true;
            videoPlayer.Play();
        }
    }

    private IEnumerator ShowFirstFrameStatic()
    {
        if (videoPlayer.clip == null)
            yield break;

        videoPlayer.Prepare();
        while (!videoPlayer.isPrepared)
            yield return null;

        // toca 1 frame
        videoPlayer.Play();
        yield return null; // 1 frame
        videoPlayer.Pause();

        // opcional: tenta garantir frame 0
        try
        {
            videoPlayer.frame = 0;
        }
        catch { }
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
