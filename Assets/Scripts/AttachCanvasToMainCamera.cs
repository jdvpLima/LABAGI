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
            var cam = Camera.main;

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
    }

}
