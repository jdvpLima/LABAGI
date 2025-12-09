using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class KeepManager : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
