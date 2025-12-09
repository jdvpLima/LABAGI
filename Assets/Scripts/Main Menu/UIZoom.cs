using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Main_Menu
{
    public class UIZoom : MonoBehaviour
    {
        [Header("Zoom Settings")]
        [SerializeField] private float zoomStep = 0.5f;      // quanto muda a cada zoom
        [SerializeField] private float minScale = 1f;       // escala mínima
        [SerializeField] private float maxScale = 2.0f;       // escala máxima
        [SerializeField] private float doubleTapMaxDelay = 0.3f; // tempo máx. entre taps (Android/Quest)

        private Canvas canvas;                   // canvas onde está o ZoomRoot

        private RectTransform rectTransform;
        private float currentScale = 1f;
        private float lastTapTime = -1f;

        private Vector2 initialAnchoredPosition;
        private float initialScale;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                Debug.LogError("UIZoom precisa de um RectTransform no mesmo GameObject.");
                enabled = false;
                return;
            }

            canvas = GetComponentInParent<Canvas>();

            if (canvas == null)
            {
                canvas = GetComponentInParent<Canvas>();
            }

            currentScale = rectTransform.localScale.x;

            initialAnchoredPosition = rectTransform.anchoredPosition;
            initialScale = currentScale;
        }

        private void Update()
        {
            HandleDesktopRightClick_NewInput();
            HandleTouch_NewInput();
        }

        // PC / Editor: right click
        private void HandleDesktopRightClick_NewInput()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            if (Mouse.current == null) return;

            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                Vector2 pos = Mouse.current.position.ReadValue();
                StepZoomAtPosition(pos);
            }
#endif
        }

        // Android / Quest (touch screen): double tap
        private void HandleTouch_NewInput()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            var ts = Touchscreen.current;
            if (ts == null) return;

            var touch = ts.primaryTouch;

            if (!touch.press.wasReleasedThisFrame) return;

            float now = Time.time;
            Vector2 pos = touch.position.ReadValue();

            if (lastTapTime > 0f && (now - lastTapTime) <= doubleTapMaxDelay)
            {
                // Double tap detectado
                StepZoomAtPosition(pos);
                lastTapTime = -1f;
            }
            else
            {
                lastTapTime = now;
            }
#endif
        }

        // Para Quest com controladores
        public void TriggerZoomFromXR()
        {
            Vector2 center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            StepZoomAtPosition(center);
        }

        // Zoom centrado num ponto de ecrã (screenPos)
        private void StepZoomAtPosition(Vector2 screenPos)
        {
            // calcula nova escala
            float newScale = currentScale + zoomStep;
            if (newScale > maxScale)
                newScale = minScale;

            newScale = Mathf.Clamp(newScale, minScale, maxScale);

            // ponto do ponteiro em coordenadas do pai do ZoomRoot
            RectTransform parentRect = rectTransform.parent as RectTransform;
            if (parentRect == null)
            {
                // fallback: só muda a escala
                ApplyScale(newScale);
                return;
            }

            Camera cam = null;
            if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                cam = canvas.worldCamera;
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                screenPos,
                cam,
                out Vector2 p // ponto local no parent
            );

            // fórmula para manter o ponto p fixo:
            // a' = p - (p - a) * (s' / s)
            Vector2 a = rectTransform.anchoredPosition;
            float s = currentScale;
            float sPrime = newScale;

            if (Mathf.Approximately(s, 0f))
            {
                s = 0.0001f;
            }

            Vector2 aPrime = p - (p - a) * (sPrime / s);

            // aplica
            rectTransform.anchoredPosition = aPrime;
            ApplyScale(newScale);
        }

        private void ApplyScale(float newScale)
        {
            currentScale = newScale;
            rectTransform.localScale = new Vector3(currentScale, currentScale, 1f);
            Debug.Log($"[UIZoom] scale = {currentScale}");
        }
        public void ResetZoom()
        {
            currentScale = initialScale;
            rectTransform.localScale = new Vector3(initialScale, initialScale, 1f);
            rectTransform.anchoredPosition = initialAnchoredPosition;
        }
    }
}

