using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DlcItemUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image dlcImage;
    [SerializeField] private TMP_Text dlcNameText;
    [SerializeField] private Button downloadButton;

    private DlcData _data;
    private Action<DlcData> _onDownloadClicked;

    public void Initialize(DlcData data, System.Action<DlcData> onDownloadClicked)
    {
        _data = data;
        _onDownloadClicked = onDownloadClicked;

        if (dlcImage != null)
            dlcImage.sprite = data.image;

        if (dlcNameText != null)
            dlcNameText.text = data.displayName;

        if (downloadButton != null)
        {
            downloadButton.onClick.RemoveAllListeners();
            downloadButton.onClick.AddListener(HandleDownloadClicked);
        }
    }

    private void HandleDownloadClicked()
    {
        if (_onDownloadClicked != null)
            _onDownloadClicked.Invoke(_data);
    }
}
