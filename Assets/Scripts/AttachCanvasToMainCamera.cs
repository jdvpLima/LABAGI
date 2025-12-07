using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    [RequireComponent(typeof(Canvas))]
    public class AttachCanvasToMainCamera : MonoBehaviour
    {
        void OnEnable()
        {
            var canvas = GetComponent<Canvas>();
            var cam = getCorrectCameraOnAR() != null ?  getCorrectCameraOnAR() : Camera.main;

            if (canvas != null && cam != null)
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = cam;
            }
            else
            {
                Debug.LogWarning("[AttachCanvasToMainCamera] Canvas ou Main Camera não encontrados.");
            }
        }


         Camera getCorrectCameraOnAR()
        {
            CameraController scriptEncontrado = FindAnyObjectByType<CameraController>();

            if (scriptEncontrado != null)
            {
                // 2. Acede ao componente Camera que está no mesmo GameObject
                Camera cam = scriptEncontrado.GetComponent<Camera>();

                // Agora podes usar a "cam"
                Debug.Log("Câmera encontrada: " + cam.name);
                return cam;
            }
            else
            {
                return null;
            }
        }
    }

}
