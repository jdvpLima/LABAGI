using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace Assets.Scripts.Settings
{
    public class HapticsManager : MonoBehaviour
    {
        public static HapticsManager Instance { get; private set; }

        [Header("XR Settings")]
        [Range(0f, 1f)]
        [SerializeField] private float defaultXRAmplitude = 0.5f;
        [SerializeField] private float defaultXRDuration = 0.1f;

        [Header("Global Toggle")]
        public bool hapticsEnabled = true; // sincronizado pelo PersistentSettingsManager

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // Chama isto para feedback háptico genérico (click, submit, etc.).
        // Decide sozinho se usa XR ou vibração de telemóvel.
        public void PlayClickHaptic()
        {
            if (!hapticsEnabled)
                return;

#if UNITY_ANDROID && !UNITY_EDITOR
        // Em Android (inclui Quest): tenta XR primeiro, se não houver, vibração de telemóvel.
        bool xrOk = TryPlayXRBothHands(defaultXRAmplitude, defaultXRDuration);
        if (!xrOk)
        {
            // Telefone Android sem XR / fallback
            Handheld.Vibrate();
        }
#else
            // Outras plataformas (PC VR, etc): tenta apenas XR
            TryPlayXRBothHands(defaultXRAmplitude, defaultXRDuration);
#endif
        }

        // Vibra ambas as mãos se possível. Devolve true se conseguiu enviar impulsos.
        private bool TryPlayXRBothHands(float amplitude, float duration)
        {
            bool sent = false;

            if (TryPlayXR(XRNode.LeftHand, amplitude, duration))
                sent = true;

            if (TryPlayXR(XRNode.RightHand, amplitude, duration))
                sent = true;

            return sent;
        }

        // Vibra uma mão específica
        public bool TryPlayXR(XRNode hand, float amplitude, float duration)
        {
            if (!hapticsEnabled)
                return false;

            var device = InputDevices.GetDeviceAtXRNode(hand);
            if (!device.isValid)
                return false;

            if (device.TryGetHapticCapabilities(out HapticCapabilities caps) && caps.supportsImpulse)
            {
                // canal 0 é o padrão na maioria dos controladores
                device.SendHapticImpulse(0u, amplitude, duration);
                return true;
            }

            return false;
        }
    }
}
