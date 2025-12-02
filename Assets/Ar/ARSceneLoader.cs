using UnityEngine;
using UnityEngine.SceneManagement;

public class ARSceneLoader : MonoBehaviour
{
    [Header("Configuração AR")]
    private Transform arCanvasParent; // Canvas ou empty object AR
    public Camera arCamera;          // Câmera AR do XR Origin
    public float worldScale = 0.005f; // Escala para reduzir o Canvas

    private GameObject sceneBefore;

    private GameObject pref;

    private GameObject dragBarPref;

    void Update()
    {
        if (pref == null)
        {
            pref = GameObject.FindWithTag("CanvaPref");
        }
        
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
                if(FindFirstObjectByType<ARPlaceUI>().spawnedUI != pref)
                {
                   FindFirstObjectByType<ARPlaceUI>().spawnedUI = pref;
                    Debug.Log("Atualizou spawnedUI no ARPlaceUI");
                }
            }
        }

        if(dragBarPref == null && pref != null)
        {
            dragBarPref = pref.transform.Find("dragger").gameObject;
        }



    }

    /// <summary>
    /// Carrega uma cena additivamente na posição do prefab clicado
    /// </summary>
    /// <param name="sceneName">Nome da cena a carregar</param>
    /// <param name="spawnPosition">Posição do prefab clicado</param>
    /// <param name="spawnRotation">Rotação do prefab clicado</param>
    public void LoadSceneAtPosition(string sceneName, Vector3 spawnPosition, Quaternion spawnRotation)
    {
        arCanvasParent = GameObject.FindWithTag("CanvaPref").transform;
        Debug.Log("Parent " + arCanvasParent);


        SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive).completed += (op) =>
        {
            Scene loadedScene = SceneManager.GetSceneByName(sceneName);
            Debug.Log("Cena carregada: " + loadedScene.name);

            foreach (GameObject rootObj in loadedScene.GetRootGameObjects())
            {
                Canvas[] canvases = rootObj.GetComponentsInChildren<Canvas>(true);
                foreach (Canvas canvas in canvases)
                {
#if UNITY_EDITOR
                    if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(canvas)) continue;
#endif
                    // Converte para World Space
                    canvas.renderMode = RenderMode.WorldSpace;
                    canvas.worldCamera = arCamera;

                    // Faz filho do empty object AR
                    //canvas.transform.SetParent(arCanvasParent, true);

                    // Define posição/rotação exata do prefab
                    canvas.transform.position = spawnPosition;
                    canvas.transform.rotation = spawnRotation;

                    // Ajusta escala para AR
                    canvas.transform.localScale = Vector3.one * worldScale;

                    // Rotaciona levemente para o usuário
                    canvas.transform.LookAt(arCamera.transform);
                    canvas.transform.Rotate(0, 180f, 0);

                    // Atualiza scripts de manipulação, se existirem
                    ARManipulateUI manipulateScript = canvas.GetComponent<ARManipulateUI>();

                    if (manipulateScript == null)
                    {
                        manipulateScript = canvas.gameObject.AddComponent<ARManipulateUI>();
                    }

                    
                    manipulateScript.placeUI = FindFirstObjectByType<ARPlaceUI>(); ;
                    manipulateScript.placeUI.spawnedUI = canvas.gameObject;
                    //manipulateScript.arCamera = arCamera;
                    AddDragBarBelowCanvas(canvas.gameObject, dragBarPref);
                    //manipulateScript.dragHandle = canvas.transform.Find("dragger")?.GetComponent<DragHandle>();
                    manipulateScript.scaleHandle = canvas.transform.Find("scaleBtn")?.transform;

                }
            }
        };
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
        Vector3 dragBarPosition = new Vector3(0, -canvasRect.rect.height / 2 - 20f, 0);
        dragBarCopy.transform.localPosition = dragBarPosition;
        dragBarCopy.transform.localRotation = Quaternion.identity;
        dragBarCopy.transform.localScale = Vector3.one;

        // Atualiza ARManipulateUI
        ARManipulateUI manipulateScript = newCanvas.GetComponent<ARManipulateUI>();
        if (manipulateScript != null)
        {
            manipulateScript.dragHandle = dragBarCopy.GetComponent<DragHandle>();
        }
    }
}