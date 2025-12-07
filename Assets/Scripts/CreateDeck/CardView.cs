using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{
    [SerializeField] private Image cardImage;

    public long cardId;
    private Transform initialParent;
    private Transform targetParent;
    private bool isAtTarget = false;


    public TextMeshProUGUI titleText;
    public TextMeshProUGUI subText;
    public TextMeshProUGUI flavourText;
    public TextMeshProUGUI actionsText;
    public Button button;

    public Card card;

    void Awake()
    {
        //button = GetComponent<Button>();
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
        subText.text = card.Suit;
        flavourText.text = card.FlavourText;

        
        button.onClick.AddListener(OnClick);


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
        //StartCoroutine(RefreshLater(newParent));
    }

    IEnumerator RefreshLater(Transform newParent)
    {
        yield return null; // espera 1 frame
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(
            newParent.GetComponent<RectTransform>()
        );
    }
}
