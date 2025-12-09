using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARPlaceUI : MonoBehaviour
{
    [Header("Componentes AR")]
    public ARRaycastManager raycastManager;
    public ARPlaneManager planeManager;

    [Header("Referências UI")]
    public ARManipulateUI manipul;
    public GameObject uiCanvasPrefab;

    [Header("Configuração de Deteção")]
    public float timeToWait = 2.0f;
    public Vector2 minWallSize = new Vector2(0.2f, 0.2f);

    // Nome da Tag para procurar o Canvas (garante que o teu Canvas tem esta Tag no Unity)
    private string canvasTag = "Canva";

    // Estado Interno
    private GameObject currentCanvas; // Guarda o canvas que encontrámos
    private GameObject spawnedUI;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private float detectionTimer = 0f;
    private bool isScanning = false;

    public GameObject loadingPanel;
    public TextMeshProUGUI loadingText;


    private void OnEnable() { EnhancedTouchSupport.Enable(); }
    private void OnDisable() { EnhancedTouchSupport.Disable(); }

    /// <summary>
    /// LIGA ISTO AO BOTÃO "MUDAR PARA AR"
    /// </summary>
    public void IniciarProcuraAR()
    {
        StartCoroutine(RotinaDePontos());
        loadingPanel.SetActive(true);
        // 1. Procurar o Canvas ativo na cena pela TAG
        currentCanvas = GameObject.FindGameObjectWithTag(canvasTag);

        /*
        // Fallback: Se não encontrar pela tag, tenta encontrar qualquer Canvas Overlay
        if (currentCanvas == null)
        {
            Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (Canvas c in allCanvases)
            {
                if (c.renderMode == RenderMode.ScreenSpaceOverlay && c.gameObject.activeInHierarchy)
                {
                    currentCanvas = c.gameObject;
                    break;
                }
            }
        }*/

        if (currentCanvas != null)
        {
            // 2. Esconde o Canvas encontrado
            currentCanvas.SetActive(false);
            Debug.Log($"Canvas '{currentCanvas.name}' encontrado e escondido. A iniciar procura de parede...");
        }
        else
        {
            Debug.LogWarning("Nenhum Canvas encontrado! Verifica se a Tag 'CanvaPref' está atribuída.");
        }

        // 3. Inicia o Scan

        if(spawnedUI == null)
        {
            detectionTimer = 0f;
            //spawnedUI = null;
            isScanning = true;
        }
        else { spawnedUI.SetActive(true);}
        
    }

    void Update()
    {
        if (!isScanning) {
            loadingPanel.SetActive(false);
            SetPlanesVisible(false);
    
        return; }
        if (spawnedUI != null) return;

        CheckForWallInFront();
    }

    void CheckForWallInFront()
    {
        if (raycastManager == null || planeManager == null) return;

        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

        if (raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            if (hits.Count > 0)
            {
                Pose hitPose = hits[0].pose;
                ARPlane plane = planeManager.GetPlane(hits[0].trackableId);

                if (plane != null &&
                    plane.alignment == PlaneAlignment.Vertical &&
                    (plane.size.x >= minWallSize.x && plane.size.y >= minWallSize.y))
                {
                    detectionTimer += Time.deltaTime;

                    if (detectionTimer >= timeToWait)
                    {
                        SpawnNewUI(hitPose);
                    }
                }
                else
                {
                    detectionTimer = 0f;
                }
            }
        }
        else
        {
            detectionTimer = 0f;
        }
    }

    void SpawnNewUI(Pose wallPose)
    {
        isScanning = false;

        // 1. Reativar o Canvas que escondemos anteriormente
        if (currentCanvas != null)
        {
            currentCanvas.SetActive(true);

            // Opcional: Garantir que ele é tratado como a 'spawnedUI' para parar lógicas futuras
            spawnedUI = currentCanvas;
        }

        // 2. Chamar o ARSceneLoader para converter esse objeto específico
        var loader = FindAnyObjectByType<ARSceneLoader>();
        if (loader != null && currentCanvas != null)
        {
            // Envia especificamente o objeto que encontrámos
            loader.turnToAR(currentCanvas, wallPose.position, wallPose.rotation);
            
            
        }

        // Se preferires converter TODOS os roots da cena (como tinhas antes):
        /*
        foreach (GameObject rootObj in SceneManager.GetActiveScene().GetRootGameObjects())
        {
             if (loader != null) loader.turnToAR(rootObj, wallPose.position, wallPose.rotation);
        }
        */

        Debug.Log("UI reativada e colocada na parede!");
    }


    
    void SpawnUI(Pose wallPose)
    {
        // Instancia na posição da parede e com a rotação da parede
        spawnedUI = Instantiate(uiCanvasPrefab, wallPose.position, wallPose.rotation);

        // --- ROTAÇÃO ---
        // IMPORTANTE: Não uses LookAt na parede. 
        // Se a UI aparecer virada para dentro da parede, descomenta a linha abaixo:
        spawnedUI.transform.LookAt(Camera.main.transform);
        spawnedUI.transform.Rotate(0, 180f, 0);

        // Atribui ao script de manipulação
        if (manipul != null)
        {
            manipul.scene = spawnedUI;
        }

        Debug.Log("UI criada na parede com sucesso!");
    }



    

    void verifyTouch2()
    {
        if (!raycastManager) return;
        if(spawnedUI != null) return;

        bool pressed = false;
        Vector2 screenPosition = default;

        if (Touchscreen.current != null)
        {
            var primary = Touchscreen.current.primaryTouch;
            if (primary.press.wasPressedThisFrame) { 
                pressed = true;
                screenPosition = primary.position.ReadValue();
            }
        }
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) {
            pressed = true;
            screenPosition = Mouse.current.position.ReadValue();

        }

        if (pressed == true)
        {
            Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            if (raycastManager.Raycast(screenCenter, hits, TrackableType.Planes))
            {
                Pose hitPose = hits[0].pose;

                if (spawnedUI == null)
                {
                    spawnedUI = Instantiate(uiCanvasPrefab, hitPose.position, hitPose.rotation);
                    spawnedUI.transform.LookAt(Camera.main.transform); // Virar para a câmera
                    spawnedUI.transform.Rotate(0, 180f, 0); // Corrigir rotação

                    manipul.scene = spawnedUI;
                    
                }
            }
        }

    }

    void CheckForPlaneInFront()
    {
        if (raycastManager == null /*|| arCamera == null*/) return;

        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

        if (raycastManager.Raycast(screenCenter, hits, TrackableType.Planes))
        {
            Pose hitPose = hits[0].pose;

            if (spawnedUI == null)
            {
                spawnedUI = Instantiate(uiCanvasPrefab, hitPose.position, hitPose.rotation);
                spawnedUI.transform.LookAt(Camera.main.transform); // Virar para a câmera
                spawnedUI.transform.Rotate(0, 180f, 0); // Corrigir rotação

                manipul.scene = spawnedUI;
            }
            else
            {
                /*
                // Permitir mover UI existente
                spawnedUI.transform.position = hitPose.position;
                spawnedUI.transform.rotation = hitPose.rotation;
                spawnedUI.transform.LookAt(Camera.main.transform);
                spawnedUI.transform.Rotate(0, 180f, 0);*/
            }
        }
    }
    /*
        void AlignCanvasToPlane(Transform canvas, ARRaycastHit hit)
        {
            // Obtém a normal do plano
            var plane = hit.trackable as ARPlane;
            if (plane == null) return;

            // Rotaciona o canvas para ficar paralelo ao plano
            canvas.rotation = Quaternion.LookRotation(plane.normal, Vector3.up);
        }*/

    
    public string textoBase = "Looking for suitable planes for AR";
    public float tempoEntrePontos = 0.5f;

    IEnumerator RotinaDePontos()
    {
        int numeroDePontos = 0;

        while (true) 
        {

            string pontos = new string('.', numeroDePontos);


            loadingText.text = textoBase + pontos;

            yield return new WaitForSeconds(tempoEntrePontos);

            numeroDePontos++;

            if (numeroDePontos > 3)
            {
                numeroDePontos = 0;
            }
        }
    }

    // Chama esta função com 'false' para esconder, 'true' para mostrar
    public void SetPlanesVisible(bool visible)
    {
        // Percorre todos os planos que o AR Foundation já detetou
        foreach (var plane in planeManager.trackables)
        {
            SetPlaneVisibility(plane, visible);
        }

        // Se quiseres que os NOVOS planos detetados já nasçam invisíveis,
        // tens de subscrever eventos ou correr isto no Update. 
        // Mas geralmente, basta esconder os existentes.

        // Dica: Para parar de mostrar novos planos visualmente mas continuar a trackear,
        // podes desligar o planeManager.enabled = false? NÃO!
        // Isso para o tracking. Tens de manter o manager ligado.
    }

    private void SetPlaneVisibility(ARPlane plane, bool visible)
    {
        // Controla o Mesh Renderer (o preenchimento amarelo)
        ARPlaneMeshVisualizer meshRenderer = plane.GetComponent<ARPlaneMeshVisualizer>();
        if (meshRenderer != null) meshRenderer.enabled = visible;

    }


}
    

