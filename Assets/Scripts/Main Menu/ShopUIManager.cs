using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform contentRoot;   // Content do Scroll View
    [SerializeField] private DlcItemUI dlcItemPrefab;
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject playScenePanel;

    [Header("DLCs disponíveis")]
    [SerializeField] private List<DlcData> dlcs = new List<DlcData>();

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseShop);
    }

    private void OnEnable()
    {
        PopulateDlcList();
    }

    private void OnDisable()
    {
        ClearDlcList();
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
        // Se a shop for um painel, podes simplesmente ocultá-lo
        gameObject.SetActive(false);
        playScenePanel.SetActive(true);

    }

    public void OnDownloadClicked(DlcData dlc)
    {
        Debug.Log("Download DLC: " + dlc.id + " - " + dlc.displayName);

        // lógica real de download / compra / instalação.
        // Exemplo pseudo-código:
        // StartCoroutine(DlcDownloader.Instance.DownloadAndInstall(dlc.downloadUrl));
    }
}
