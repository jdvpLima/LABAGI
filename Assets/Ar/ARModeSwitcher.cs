using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
// using UnityEngine.XR.ARFoundation; // Descomenta se necessário para tipos específicos

public class ARModeSwitcher : MonoBehaviour
{

    // --- SINGLETON SETUP (Para não destruir e não duplicar) ---
    public static ARModeSwitcher Instance { get; private set; }

    [Header("Estado AR Guardado")]
    public Vector3 lastPosition;
    public Quaternion lastRotation;
    public Vector3 lastScale;
    public bool hasSavedState = false; // Para sabermos se já temos algo guardado


    private void Awake()
    {
        // Se já existir uma instância e não for esta...
        if (Instance != null && Instance != this)
        {
            // ...destruímos este objeto (o duplicado)
            Destroy(gameObject);
            return;
        }

        // Se não existir, esta passa a ser a instância oficial
        Instance = this;

        // Torna este objeto imortal entre cenas
        DontDestroyOnLoad(gameObject);
    }

    // -----------------------------------------------------------
    [Header("Referências Obrigatórias")]
    //public GameObject arSessionOrigin; // O objeto pai que tem a AR Camera
    public GameObject arSession;       // O objeto "AR Session" (que gere o tracking)
    public Camera standardCamera;      // Uma câmara normal (não-AR) que deve estar na cena

    [Header("Scripts Externos")]
    public ARManipulateUI manipulateUI; // O teu script de controlo
    public GameObject dragBarPref;      // Referência para limpar a dragbar
    public GameObject ARPlaceUI;

    public static bool ar_active = false;

    
    public void ResetCanvasToScreenSpace(GameObject canvasObj)
    {
        if (canvasObj == null) return;

        Canvas canvas = canvasObj.GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = standardCamera;
            //canvas.transform.localScale = Vector3.one;
            // Opcional: Resetar posição se necessário, mas Overlay ignora isto
            canvas.planeDistance = 1.0f;

            canvas.transform.localScale = Vector3.one;
            canvas.transform.localPosition = Vector3.zero;
            canvas.transform.localRotation = Quaternion.identity;
        }

        // Limpar DragBar
        if (dragBarPref != null)
        {
            string dragBarName = dragBarPref.name + "(Clone)";
            Transform dragBarRef = canvasObj.transform.Find(dragBarName);
            if (dragBarRef != null) Destroy(dragBarRef.gameObject);
        }
        /*
        // Limpar Referência
        if (manipulateUI != null && manipulateUI.scene == canvasObj)
        {
            manipulateUI.scene = null;
        }*/
    }

    /// <summary>
    /// Tenta encontrar a câmara automaticamente (mesmo se estiver desativada)
    /// </summary>
    private void FindStandardCamera()
    {
        // Esta função encontra TUDO o que é câmara, incluindo as desativadas
        Camera[] allCameras = Resources.FindObjectsOfTypeAll<Camera>();

        foreach (Camera cam in allCameras)
        {
            // Verifica se o objeto está na cena (e não nos Assets) e se tem o nome correto
            if (cam.gameObject.scene.IsValid() && cam.name == "StandardCamera")
            {
                standardCamera = cam;
                Debug.Log("Standard Camera encontrada automaticamente!");
                return;
            }
        }
    }

    public void DisableARMode()
    {
        if (!ar_active) return;

        ar_active = false;

        // 1. Reverter UI
        if (manipulateUI != null && manipulateUI.scene != null)
        {
            Transform targetInfo = manipulateUI.scene.transform;

            lastPosition = targetInfo.position;
            lastRotation = targetInfo.rotation;
            lastScale = targetInfo.localScale;

            hasSavedState = true;
            ResetCanvasToScreenSpace(manipulateUI.scene);
        }

        // 2. Garantir que temos a câmara normal
        if (standardCamera == null)
        {
            FindStandardCamera();
        }

        // 3. Trocar as Câmaras
        //if (arSessionOrigin != null) arSessionOrigin.SetActive(false);
        if (arSession != null) arSession.SetActive(false);

        if (standardCamera != null)
        {
            standardCamera.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("ERRO CRÍTICO: Não foi encontrada nenhuma câmara com o nome 'StandardCamera'.");
        }

        Debug.Log("Modo AR Desativado.");

        SceneManager.sceneLoaded -= FindAnyObjectByType<ARSceneLoader>().OnSceneLoadedAndReady;
        SceneManager.sceneUnloaded -= FindAnyObjectByType<ARSceneLoader>().OnSceneUnload;

    }


    public void EnableARMode()
    {
        if (ar_active) return;

        ar_active = true;

        // Se a câmara não estiver atribuída, tenta encontrar antes de desativar
        if (standardCamera == null) FindStandardCamera();

        if (standardCamera != null) standardCamera.gameObject.SetActive(false);
        if (arSession != null) arSession.SetActive(true);
        //if (arSessionOrigin != null) arSessionOrigin.SetActive(true);

        if(ARPlaceUI != null) ARPlaceUI.gameObject.SetActive(true);

        FindAnyObjectByType<ARPlaceUI>().IniciarProcuraAR();

        /*

        FindAnyObjectByType<ARSceneLoader>().transformIntoAR(SceneManager.GetActiveScene().GetRootGameObjects());
*/
        if (hasSavedState)
        {
            RestoreLastPosition(manipulateUI.scene);
        }

        
        SceneManager.sceneLoaded += FindAnyObjectByType<ARSceneLoader>().OnSceneLoadedAndReady;
        SceneManager.sceneUnloaded += FindAnyObjectByType<ARSceneLoader>().OnSceneUnload;


    }


    public void RestoreLastPosition(GameObject targetObj)
    {
        if (hasSavedState && targetObj != null)
        {
            targetObj.transform.position = lastPosition;
            targetObj.transform.rotation = lastRotation;
            targetObj.transform.localScale = lastScale;

            // Voltar a mudar para World Space
            Canvas c = targetObj.GetComponent<Canvas>();
            if (c != null)
            {
                c.renderMode = RenderMode.WorldSpace;
                // Aqui terias de reatribuir a worldCamera se necessário
            }
        }
    }
}