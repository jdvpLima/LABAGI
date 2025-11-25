using UnityEngine;

public class CardFullscreenToggle : MonoBehaviour
{
    private RectTransform rect;

    private RectTransform originalParent;
    private RectTransform fullscreenParent;

    private Vector2 originalAnchoredPos;
    private Vector2 originalSizeDelta;
    private Vector2 originalAnchorMin;
    private Vector2 originalAnchorMax;
    private Vector2 originalPivot;

    private bool isFullscreen = false;

    private float originalAspect = 1f;

    public void Init(RectTransform originalParent, RectTransform fullscreenParent)
    {
        this.originalParent = originalParent;
        this.fullscreenParent = fullscreenParent;
        rect = GetComponent<RectTransform>();
    }

    public void ToggleFullscreen()
    {
        if (rect == null || fullscreenParent == null || originalParent == null)
            return;

        if (!isFullscreen)
        {
            EnterFullscreen();
        }
        else
        {
            ExitFullscreen();
        }
    }

    private void EnterFullscreen()
    {
        // išsaugom pradinę info
        originalAnchorMin = rect.anchorMin;
        originalAnchorMax = rect.anchorMax;
        originalPivot = rect.pivot;
        originalAnchoredPos = rect.anchoredPosition;
        originalSizeDelta = rect.sizeDelta;

        // aspect ratio pagal dabartinį rect
        if (rect.rect.height > 0.01f)
            originalAspect = rect.rect.width / rect.rect.height;
        else
            originalAspect = 0.7f; // fallback

        // perkeliam į fullscreen parent
        rect.SetParent(fullscreenParent, worldPositionStays: false);

        // pilnas vertikalus – centras ekrane
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);

        float parentHeight = fullscreenParent.rect.height;
        float margin = 50f; // šiek tiek paliekam kraštams

        float targetHeight = parentHeight - margin;
        float targetWidth = targetHeight * originalAspect;

        rect.sizeDelta = new Vector2(targetWidth, targetHeight);
        rect.anchoredPosition = Vector2.zero;

        isFullscreen = true;
    }

    private void ExitFullscreen()
    {
        // grąžinam į grid parent
        rect.SetParent(originalParent, worldPositionStays: false);

        // grąžinam anchor/pivot/position/size
        rect.anchorMin = originalAnchorMin;
        rect.anchorMax = originalAnchorMax;
        rect.pivot = originalPivot;
        rect.sizeDelta = originalSizeDelta;
        rect.anchoredPosition = originalAnchoredPos;

        isFullscreen = false;
    }
}
