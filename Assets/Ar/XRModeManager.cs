using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Importante para UI

public class XRModeManager : MonoBehaviour
{

    [Header("Rigs")]
    public GameObject mobileRig;
    public GameObject vrRig;

    [Header("UI Helper")]
    // Arrastar o Prefab ou o Canvas da cena se ele já existir
    public Canvas canvasUI;
    public ARManipulateUI arManipulateScript; // O teu script de toque


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        // Verifica se há um dispositivo XR (Headset) ativo e carregado
        bool isVR = XRSettings.isDeviceActive;

        if (isVR)
        {
            Debug.Log("Modo VR (Quest) Detetado");
            //SetupVR();
        }
        else
        {
            Debug.Log("Modo Mobile AR Detetado");
            //SetupMobile();
        }

    }

    
}
