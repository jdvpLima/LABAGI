using UnityEngine;
using UnityEngine.EventSystems;

public class ARManipulateUI : MonoBehaviour
{
    public ARPlaceUI placeUI;
    public Transform scaleHandle;
    public DragHandle dragHandle;

    private bool isScalingWithHandle = false;

    private Quaternion baseRotation;     // rotação paralela à parede
    private float tiltAmount = 0.5f;    // intensidade do “virar” (0.0 a 1.0)
    private float tiltSpeed = 10f;        // suavidade da inclinação

    public ARSceneLoader loader; 

    [Header("Scale")]
    public float scaleSpeed = 0.0005f;
    public float minScale = 0.1f;
    public float maxScale = 3f;

    private Vector3 initialScale;
    private float lastMouseY;



    private void Awake()
    {

        if (loader == null)
        {
            loader = FindFirstObjectByType<ARSceneLoader>();
        }
        if (placeUI == null)
        {
            placeUI = FindFirstObjectByType<ARPlaceUI>();
        }
        else
        {
            baseRotation = placeUI.spawnedUI.transform.rotation;
        }

    }

    void Update()
    {
        Debug.Log("Update em ARManipulateUI");

        if (placeUI.spawnedUI == null)
        {
            Debug.Log("spawnedUI é nulo, retornando");
            return;
        }
        else { Debug.Log("não é nullo"); }

#if UNITY_EDITOR || UNITY_STANDALONE
            HandleMouseDrag();
        HandleMouseScale();
#else
        HandleTouchDrag();
        HandlePinchScale();
        HandleHandleScale();
#endif
    }

    /* ------------------ TOUCH ------------------ */

    void HandleTouchDrag()
    {
        if (Input.touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            Vector2 mid = (t0.position + t1.position) / 2;

            if (EventSystem.current != null &&
               (EventSystem.current.IsPointerOverGameObject(t0.fingerId) ||
                EventSystem.current.IsPointerOverGameObject(t1.fingerId))) return;

            Ray ray = Camera.main.ScreenPointToRay(mid);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                placeUI.spawnedUI.transform.position = hit.point;
            }
        }
    }

    void HandlePinchScale()
    {
        if (Input.touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            float prevDist =
                (t0.position - t0.deltaPosition - (t1.position - t1.deltaPosition)).magnitude;
            float currDist =
                (t0.position - t1.position).magnitude;

            float diff = currDist - prevDist;

            placeUI.spawnedUI.transform.localScale += Vector3.one * diff * 0.001f;
        }
    }

    void HandleHandleScale()
    {
        if (isScalingWithHandle && Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            Ray ray = Camera.main.ScreenPointToRay(t.position);

            Plane plane = new Plane(
                -placeUI.spawnedUI.transform.forward,
                placeUI.spawnedUI.transform.position);

            if (plane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                float dist = Vector3.Distance(placeUI.spawnedUI.transform.position, hitPoint);
                placeUI.spawnedUI.transform.localScale = Vector3.one * dist;
            }
        }
    }

    /* ------------------ MOUSE ------------------ */

    void HandleMouseDrag()
    {
        /*
        if (Input.GetMouseButton(0))
        {
            if (EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject())
                return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                placeUI.spawnedUI.transform.position = hit.point;
            }
        }*/

        // Só permite movimento quando ALT + botão esquerdo estiverem pressionados
        /*
        if (!(Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
            return;

        if (!Input.GetMouseButton(0))
            return;

        if (placeUI.spawnedUI == null)
            return;

        // Não arrastar se estiver clicando em UI (botões, etc.)
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Posição alvo
            Vector3 targetPos = hit.point;

            // Movimento suavizado
            float smoothSpeed = 10f;
            placeUI.spawnedUI.transform.position =
                Vector3.Lerp(
                    placeUI.spawnedUI.transform.position,
                    targetPos,
                    Time.deltaTime * smoothSpeed
                );
        }*/

        if (placeUI.spawnedUI == null)
            return;

        // Só arrasta se clicou na barra inferior
        if (dragHandle == null || !dragHandle.IsDragging)
            return;

        // Não mover se clicar em UI que não seja a barra
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
            return;

        if (!Input.GetMouseButton(0))
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 targetPos = hit.point;

            // Movimento mais suave
            float smoothSpeed = 10f;
            placeUI.spawnedUI.transform.position =
                Vector3.Lerp(
                    placeUI.spawnedUI.transform.position,
                    targetPos,
                    Time.deltaTime * smoothSpeed
                );
        }

        ApplyTiltTowardsCamera();
    }
    
    void HandleMouseScale()
    {
        float scroll = Input.mouseScrollDelta.y;
        Debug.Log("Scroll: " + scroll);

        if (Mathf.Abs(scroll) > 0.01f)
        {
            placeUI.spawnedUI.transform.localScale += Vector3.one * scroll * 0.0005f;
        }
    }




    /* ------------------ SETA / HANDLE ------------------ */

    public void StartHandleScale()
    {
        Debug.Log("Iniciando escala com handle");
        isScalingWithHandle = true;
    }

    public void EndHandleScale()
    {
        Debug.Log("Terminando escala com handle");
        isScalingWithHandle = false;
    }

    public void Button()
    {
        Debug.Log("Botão pressionado na UI AR");
        loader.LoadSceneAtPosition("DeckCreation", placeUI.spawnedUI.transform.position, placeUI.spawnedUI.transform.rotation);
    }


    void ApplyTiltTowardsCamera()
    {
        Transform ui = placeUI.spawnedUI.transform;
        Transform cam = Camera.main.transform;

        // direção da UI para a câmara
        Vector3 dirToCam = cam.position - ui.position;
        dirToCam.y = 0; // impede torção vertical exagerada

        if (dirToCam.sqrMagnitude < 0.001f)
            return;

        // rotação que OLHARIA totalmente a câmara
        Quaternion fullLook = Quaternion.LookRotation(-dirToCam, Vector3.up);

        // mistura leve (inclinar ligeiramente)
        Quaternion slightTilt = Quaternion.Slerp(baseRotation, fullLook, tiltAmount);

        // aplica suavemente
        ui.rotation = Quaternion.Slerp(
            ui.rotation,
            slightTilt,
            Time.deltaTime * tiltSpeed
        );
    }
    


}