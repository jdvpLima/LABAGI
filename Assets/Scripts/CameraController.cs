using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class CameraController : MonoBehaviour
    {

        private static bool retainCamera;

        // Start is called before the first frame update
        void Start()
        {
            DontDestroyOnLoad(transform.gameObject);

            //Retain on Load of New Scene
            if (!retainCamera)
            {
                retainCamera = true;
                DontDestroyOnLoad(transform.gameObject);
                Debug.Log("Camera Loaded");
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
