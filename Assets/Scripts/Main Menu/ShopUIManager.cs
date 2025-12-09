using Assets.Scripts.CreateDeck;
using Assets.Scripts.Main_Menu;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ShopUIManager : MonoBehaviour
{
    [Header("API")]
    [SerializeField] private string baseApiUrl = "https://lagabi-group2-backend.onrender.com/api";

    [Header("UI References")]
    [SerializeField] private Transform contentRoot;
    [SerializeField] private DlcItemUI dlcItemPrefab;
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject playScenePanel;

    [Header("Sprites por Expansion Code")]
    [SerializeField] private List<ExpansionSpriteBinding> expansionSprites;
    
    private long userId;
    private readonly List<DlcData> dlcs = new();

    [System.Serializable]
    public class ExpansionSpriteBinding
    {
        public string code;   // ex: "exp_adaptive_minds"
        public Sprite sprite; // ícone da expansão
    }

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseShop);
    }

    private void OnEnable()
    {
        // ir buscar userId ao AuthBootstrapper
        userId = AuthBootstrapper.CurrentUserId;

        if (userId <= 0)
        {
            Debug.LogError("[ShopUIManager] userId inválido. Certifica-te que o login foi feito antes de abrir a shop.");
            return;
        }
        StartCoroutine(LoadExpansionsAndPopulate());
    }

    private void OnDisable()
    {
        ClearDlcList();
    }

    private IEnumerator LoadExpansionsAndPopulate()
    {
        ClearDlcList();
        dlcs.Clear();

        var url = $"{baseApiUrl}/Expansions/available?userId={userId}";

        using (var req = UnityWebRequest.Get(url))
        {
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Erro ao carregar expansões: {req.responseCode} - {req.error}");
                yield break;
            }

            var json = req.downloadHandler.text;
            var expansions = JsonConvert.DeserializeObject<List<ExpansionApiDTO>>(json);

            foreach (var e in expansions)
            {
                // decidir não mostrar Core ou já owned:
                if (e.isCore) continue;       // Core não é DLC
                // não mostrar já compradas:
                if (e.owned) continue;

                var sprite = GetSpriteForCode(e.code);

                var dlc = new DlcData
                {
                    id = e.code,
                    displayName = e.name,
                    image = sprite,
                    downloadUrl = null
                };

                dlcs.Add(dlc);
            }
        }

        PopulateDlcList();
    }

    private Sprite GetSpriteForCode(string code)
    {
        foreach (var binding in expansionSprites)
        {
            if (binding.code == code)
                return binding.sprite;
        }
        return null;
    }

    private void PopulateDlcList()
    {
        ClearDlcList();

        if (contentRoot == null || dlcItemPrefab == null)
            return;

        foreach (var dlc in dlcs)
        {
            var item = Instantiate(dlcItemPrefab, contentRoot);
            item.Initialize(dlc, OnDownloadClicked);
        }
    }

    private void ClearDlcList()
    {
        if (contentRoot == null)
            return;

        for (int i = contentRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(contentRoot.GetChild(i).gameObject);
        }
    }

    public void CloseShop()
    {
        if (MainMenuManager.Instance.uiZoom != null)
            MainMenuManager.Instance.uiZoom.ResetZoom();

        gameObject.SetActive(false);
        if (playScenePanel != null)
            playScenePanel.SetActive(true);
    }

    public void OnDownloadClicked(DlcData dlc)
    {
        Debug.Log("Pedir compra DLC: " + dlc.id + " - " + dlc.displayName);
        StartCoroutine(PurchaseDlcCoroutine(dlc));
    }

    private IEnumerator PurchaseDlcCoroutine(DlcData dlc)
    {
        var url = $"{baseApiUrl}/Expansions/{dlc.id}/purchase?userId={userId}";

        using (var req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
        {
            req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(string.Empty));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Erro ao comprar DLC {dlc.id}: {req.responseCode} - {req.error}");
                Debug.LogError(req.downloadHandler.text);
            }
            else
            {
                Debug.Log($"DLC comprada com sucesso: {dlc.displayName}");
                // Opcional: recarregar lista para esconder o que passou a owned
                StartCoroutine(LoadExpansionsAndPopulate());
            }
        }
    }
}
