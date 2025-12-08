using UnityEngine;
using UnityEngine.EventSystems;

public class ScaleHandle : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool isScalingWithHandle { get; private set; }

    public void OnPointerDown(PointerEventData eventData)
    {
        isScalingWithHandle = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isScalingWithHandle = false;
    }
}
