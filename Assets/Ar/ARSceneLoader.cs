using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;

public class ARSceneLoader : MonoBehaviour
{
    [Header("Configuração AR")]
    //private Transform arCanvasParent; // Canvas ou empty object AR
    public Camera arCamera;          // Câmera AR do XR Origin
    public float worldScale = 0.0025f; // Escala para reduzir o Canvas


    public ARManipulateUI manipulateUI;

    private GameObject sceneBefore;

    private GameObject pref;

    public GameObject dragBarPref;
    public GameObject scalePref;

    void Update()
    {
        if (sceneBefore != null && manipulateUI.scene == null)
            manipulateUI.scene = sceneBefore;

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
                if(FindFirstObjectByType<ARManipulateUI>().scene != pref)
                {
                    FindFirstObjectByType<ARManipulateUI>().scene = pref;
                    Debug.Log("Atualizou Scene no ARManipulate");
                }
            }
        }
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
        foreach (Canvas canvas in canvases)
        {
#if UNITY_EDITOR
            if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(canvas)) continue;
#endif

            if (canvas.sortingOrder > 32700) continue;
            // Converte para World Space
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = arCamera;


            // Define posição/rotação exata do prefab
            canvas.transform.position = spawnPosition;
            canvas.transform.rotation = spawnRotation;

            // Ajusta escala para AR
            canvas.transform.localScale = Vector3.one * worldScale;

            // Rotaciona levemente para o usuário
            canvas.transform.LookAt(arCamera.transform);
            canvas.transform.Rotate(0, 180f, 0);


            manipulateUI.scene = canvas.gameObject;


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


}