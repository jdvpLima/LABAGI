using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class ARPlaceUI : MonoBehaviour
{
    public ARRaycastManager raycastManager;
    public GameObject uiCanvasPrefab;

    //[HideInInspector]
    public GameObject spawnedUI;

    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Update()
    {
        // Ignorar toque sobre UI existente
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(0))
            return;

#if UNITY_EDITOR
        if (!Input.GetMouseButtonDown(0))
            return;
        Vector2 touchPos = Input.mousePosition;
#else
        if (Input.touchCount == 0)
            return;
        Vector2 touchPos = Input.GetTouch(0).position;
#endif

        if (raycastManager.Raycast(touchPos, hits, TrackableType.Planes))
        {
            Pose hitPose = hits[0].pose;

            if (spawnedUI == null)
            {
                spawnedUI = Instantiate(uiCanvasPrefab, hitPose.position, hitPose.rotation);
                spawnedUI.transform.LookAt(Camera.main.transform); // Virar para a câmera
                spawnedUI.transform.Rotate(0, 180f, 0); // Corrigir rotação
                var manip = spawnedUI.GetComponent<ARManipulateUI>();
                if (manip != null)
                {
                    manip.placeUI = this;
                }
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

            // Opcional: ajustar a UI para sempre ficar vertical (normal da parede)
            //AlignCanvasToPlane(spawnedUI.transform, hits[0]);
        }
    }

    void AlignCanvasToPlane(Transform canvas, ARRaycastHit hit)
    {
        // Obtém a normal do plano
        var plane = hit.trackable as ARPlane;
        if (plane == null) return;

        // Rotaciona o canvas para ficar paralelo ao plano
        canvas.rotation = Quaternion.LookRotation(plane.normal, Vector3.up);
    }
}
