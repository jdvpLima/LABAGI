using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem; 
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class ARManipulateUI : MonoBehaviour
{
    //public ARPlaceUI placeUI;

    public GameObject scene;

    public ScaleHandle scaleHandle;
    public DragHandle dragHandle;


    private Quaternion baseRotation;     // rotação paralela à parede
    private float tiltAmount = 0.5f;    // intensidade do “virar” (0.0 a 1.0)
    private float tiltSpeed = 10f;        // suavidade da inclinação


    [Header("Scale")]
    public float scaleSpeed = 0.0005f;
    public float minScale = 0.0005f;
    public float maxScale = 3f;

    private Vector3 initialScale;
    private float lastMouseY;



    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    void Update()
    {
        //Debug.Log("Update em ARManipulateUI");


        if (scene != null)
        {
            scaleHandle = scene.GetComponentInChildren<ScaleHandle>();
            dragHandle = scene.GetComponentInChildren<DragHandle>();
            baseRotation = scene.transform.rotation;

        }
        else {
            
            return;
        }

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
        if (scene == null) return;

        // 1. Só permite se a handle (barra inferior) estiver a ser arrastada
        if (dragHandle == null || !dragHandle.IsDragging) return;

        // 2. Verifica se há pelo menos um toque
        if (Touch.activeTouches.Count == 0) return;

        var touch = Touch.activeTouches[0];

        // 3. Verifica UI (Mantendo a mesma lógica do rato)
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.touchId))
            return;

        // 4. Raycast e Movimento (igual ao HandleMouseDrag)
        Ray ray = Camera.main.ScreenPointToRay(touch.screenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 targetPos = hit.point;

            // Movimento suavizado (Lerp)
            float smoothSpeed = 10f;
            scene.transform.position =
                Vector3.Lerp(
                    scene.transform.position,
                    targetPos,
                    Time.deltaTime * smoothSpeed
                );
        }

        // 5. Aplica a inclinação suave
        ApplyTiltTowardsCamera();
    }

    void HandlePinchScale()
    {
        if (Touch.activeTouches.Count == 2)
        {
            var t0 = Touch.activeTouches[0];
            var t1 = Touch.activeTouches[1];

            // delta substitui deltaPosition
            float prevDist =
                (t0.screenPosition - t0.delta - (t1.screenPosition - t1.delta)).magnitude;
            float currDist =
                (t0.screenPosition - t1.screenPosition).magnitude;

            float diff = currDist - prevDist;

            scene.transform.localScale += Vector3.one * diff * 0.001f;
        }
    }

    void HandleHandleScale()
    {
        if (scaleHandle.isScalingWithHandle && Touch.activeTouches.Count > 0)
        {
            var t = Touch.activeTouches[0];
            Ray ray = Camera.main.ScreenPointToRay(t.screenPosition);

            Plane plane = new Plane(
                -scene.transform.forward,
                scene.transform.position);

            if (plane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                float dist = Vector3.Distance(scene.transform.position, hitPoint);
                scene.transform.localScale = Vector3.one * dist;
            }
        }
    }

    /* ------------------ MOUSE ------------------ */

    void HandleMouseDrag()
    {
       
        if (scene == null)
            return;

        if (Mouse.current == null) return;

        // Só arrasta se clicou na barra inferior
        if (dragHandle == null || !dragHandle.IsDragging)
            return;

        // Não mover se clicar em UI que não seja a barra
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
            return;

        // Substitui Input.GetMouseButton(0)
        if (!Mouse.current.leftButton.isPressed)
            return;

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 targetPos = hit.point;

            // Movimento mais suave
            float smoothSpeed = 10f;
            scene.transform.position =
                Vector3.Lerp(
                    scene.transform.position,
                    targetPos,
                    Time.deltaTime * smoothSpeed
                );
        }

        ApplyTiltTowardsCamera();
    }
    
    void HandleMouseScale()
    {
        if (Mouse.current == null) return;

        float scroll = Mouse.current.scroll.ReadValue().y;
        // Debug.Log("Scroll: " + scroll);

        if (Mathf.Abs(scroll) > 0.01f)
        {

            float currentScale = scene.transform.localScale.x;


            float nextScale = currentScale + (scroll * scaleSpeed);

            
            if (nextScale < 0.0020)
            {
                scene.transform.localScale = Vector3.one * 0.0020f;

            } else
            if (nextScale > 0.004)
            {
                scene.transform.localScale = Vector3.one * 0.004f;

            }
            else
            {
                //nextScale = Mathf.Clamp(nextScale, minScale, maxScale);
                scene.transform.localScale = Vector3.one * nextScale;
            }  
        }
    }

    void ApplyTiltTowardsCamera()
    {
        Transform ui = scene.transform;
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