using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class ARSceneLoader : MonoBehaviour
{
    [Header("Configuração AR")]
    //private Transform arCanvasParent; // Canvas ou empty object AR
    public Camera arCamera;          // Câmera AR do XR Origin
    public float worldScale = 0.00000000005f; // Escala para reduzir o Canvas


    public ARManipulateUI manipulateUI;

    private GameObject sceneBefore;

    private GameObject pref;

    public GameObject dragBarPref;
    public GameObject scalePref;

    [SerializeField]
    private List<GameObject> activeCanvasStack = new List<GameObject>();


    private void Start()
    {
    }

    private void OnDestroy()
    {
        // Boa prática limpar eventos ao destruir o objeto
       
    }

    void Update()
    {
        if (sceneBefore != null && manipulateUI.scene == null)
            manipulateUI.scene = sceneBefore;

        if (pref == null)
        {
            pref = GameObject.FindWithTag("CanvaPref");
        }
        /*
        if (pref != null)
        {
            if (
#if UNITY_EDITOR
                CountActiveAdditiveScenes() > 1
#else
                CountActiveAdditiveScenes() > 0
#endif
                )
            {
                pref.SetActive(false);
            }
            else
            {
                pref.SetActive(true);
                if(FindFirstObjectByType<ARManipulateUI>().scene != pref)
                {
                    FindFirstObjectByType<ARManipulateUI>().scene = pref;
                    Debug.Log("Atualizou Scene no ARManipulate");
                }
            }
        }*/
    }

    /// <summary>
    /// Carrega uma cena additivamente na posição do prefab clicado
    /// </summary>
    /// <param name="sceneName">Nome da cena a carregar</param>
    /// <param name="spawnPosition">Posição do prefab clicado</param>
    /// <param name="spawnRotation">Rotação do prefab clicado</param>
    public void LoadSceneAtPosition(string sceneName)
    {
        sceneBefore = manipulateUI.scene;
        SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive).completed += (op) =>
        {
            Scene loadedScene = SceneManager.GetSceneByName(sceneName);
            Debug.Log("Cena carregada: " + loadedScene.name);
            transformIntoAR(loadedScene.GetRootGameObjects());
            
        };
    }

    public void LoadNetworkScenes(Scene loadedScene)
    {
        sceneBefore = manipulateUI.scene;
        Debug.Log("Cena carregada: " + loadedScene.name);
        transformIntoAR(loadedScene.GetRootGameObjects());
    }



    public void transformIntoAR(GameObject[] loadedSceneObjects)
    {
        Vector3 spawnPosition = manipulateUI.scene.transform.position;
        Quaternion spawnRotation = manipulateUI.scene.transform.rotation;

        foreach (GameObject rootObj in loadedSceneObjects)
        {
            turnToAR(rootObj, spawnPosition, spawnRotation);
        }
    }

    public void turnToAR(GameObject rootObj, Vector3 spawnPosition, Quaternion spawnRotation)
    {
        Canvas[] canvases = rootObj.GetComponentsInChildren<Canvas>(true);

        // Variável para guardar a largura da cena anterior (se existir)
        float baseWidth = 0f;
        bool hasBaseScene = false;
        if (manipulateUI.scene != null)
        {
            CanvasScaler baseScaler = manipulateUI.scene.GetComponent<CanvasScaler>();
            if (baseScaler != null)
            {
                baseWidth = baseScaler.referenceResolution.x;
                hasBaseScene = true;
            }
            else
            {
                RectTransform baseRect = manipulateUI.scene.GetComponent<RectTransform>();
                if (baseRect != null)
                {
                    baseWidth = baseRect.rect.width;
                    hasBaseScene = true;
                }
            }
        }

        foreach (Canvas canvas in canvases)
        {
#if UNITY_EDITOR
            if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(canvas)) continue;
#endif

            if (canvas.sortingOrder > 32700) continue;


            // --- CÁLCULO DO FATOR DE ESCALA ---
            float correctionFactor = 1f;

            // Só calculamos correção se tivermos uma cena base para comparar
            if (hasBaseScene)
            {
                CanvasScaler currentScaler = canvas.GetComponent<CanvasScaler>();
                if (currentScaler != null && currentScaler.referenceResolution.x > 0)
                {
                    // Fórmula: Largura da Cena 1 / Largura da Cena 2
                    // Exemplo: Se Cena 1 (1920) e Cena 2 (3840) -> 1920/3840 = 0.5 (reduz para metade)
                    correctionFactor = baseWidth / currentScaler.referenceResolution.x;
                }
                else
                {
                    // Fallback para RectTransform se o novo objeto não tiver Scaler
                    RectTransform currentRect = canvas.GetComponent<RectTransform>();
                    if (currentRect != null && currentRect.rect.width > 0)
                    {
                        correctionFactor = baseWidth / currentRect.rect.width;
                    }
                }
            }
            // ----------------------------------


            // Converte para World Space
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = arCamera;


            // Define posição/rotação exata do prefab
            canvas.transform.position = spawnPosition;
            canvas.transform.rotation = spawnRotation;

            // Ajusta escala para AR
            canvas.transform.localScale = Vector3.one * worldScale * correctionFactor;

            if (manipulateUI.scene == null)
            {
                // Rotaciona levemente para o usuário
                canvas.transform.LookAt(arCamera.transform);
                canvas.transform.Rotate(0, 180f, 0);
            }


            manipulateUI.scene = canvas.gameObject;

            activeCanvasStack.Add(canvas.gameObject);
            AddDragBarBelowCanvas(canvas.gameObject, dragBarPref);
            //manipulateScript.dragHandle = canvas.transform.Find("dragger")?.GetComponent<DragHandle>();
            //manipulateScript.scaleHandle = canvas.transform.Find("scaleBtn")?.transform;

        }
    }


    public int CountActiveAdditiveScenes()
    {
        int count = 0;

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);

            // Somente cenas carregadas e que não são a cena principal
            if (scene.isLoaded && scene.name != SceneManager.GetActiveScene().name)
            {
                count++;
            }
        }

        return count;
    }


    public void AddDragBarBelowCanvas(GameObject newCanvas, GameObject dragBarPrefab)
    {
        if (newCanvas == null || dragBarPrefab == null) return;

        RectTransform canvasRect = newCanvas.GetComponent<RectTransform>();
        if (canvasRect == null) return;

        // Instancia a DragBar
        GameObject dragBarCopy = Instantiate(dragBarPrefab, newCanvas.transform);

        // Posiciona ligeiramente abaixo do Canvas
        Vector3 dragBarPosition = new Vector3(0, -canvasRect.rect.height / 2 - 30f, 0);
        dragBarCopy.transform.localPosition = dragBarPosition;
        dragBarCopy.transform.localRotation = Quaternion.identity;
        dragBarCopy.transform.localScale = Vector3.one * 1.5f;

        /*
        // Atualiza ARManipulateUI
        ARManipulateUI manipulateScript = newCanvas.GetComponent<ARManipulateUI>();
        if (manipulateScript != null)
        {
            manipulateScript.dragHandle = dragBarCopy.GetComponent<DragHandle>();
        }*/
    }


    public void generateFirstAR(GameObject[] loadedSceneObjects)
    {

    }


    /// <summary>
    /// Descarrega uma cena additiva
    /// </summary>
    /// <param name="sceneName"></param>
    public void UnloadScene(string sceneName)
    {
        if (SceneManager.GetSceneByName(sceneName).isLoaded)
        {
            SceneManager.UnloadSceneAsync(sceneName);
        }
    }

    public void OnSceneLoadedAndReady(Scene scene, LoadSceneMode mode)
    {
        // Verifica se a cena carregada é uma das cenas de jogo AR permitidas
        if (scene.name == "Game" || scene.name == "GameResults")
        {
            if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer)
            {
                Debug.Log($"Cena de Jogo detetada: {scene.name}. Transformando em AR...");
                LoadNetworkScenes(scene);
            }
            if (SceneManager.GetSceneByName("PreGame").isLoaded && scene.name != "CardViewer2")
            {
                SceneManager.UnloadSceneAsync("PreGame");
            }
        }
        else
        {
            Debug.Log($"Cena {scene.name} carregada.");
        }
    }

    public void OnSceneUnload(Scene scene)
    {
        Debug.Log("WHYYYYY");
        if (activeCanvasStack.Contains(manipulateUI.scene))
        {
            if (activeCanvasStack.Remove(manipulateUI.scene))
                sceneBefore = GetTopScene();
        }
      
    }

    public GameObject GetTopScene()
    {
        if (activeCanvasStack.Count > 0)
        {
            return activeCanvasStack[activeCanvasStack.Count - 1];
        }
        return null;
    }


}