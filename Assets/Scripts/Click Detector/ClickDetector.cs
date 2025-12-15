using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickDetector : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
       
        RaycastResult raycast = eventData.pointerPressRaycast;

        GameObject clickedObject = raycast.gameObject;

        int layerIndex = clickedObject.layer;
        string layerName = LayerMask.LayerToName(layerIndex);

        Debug.Log("Objek yang Diklik: " + clickedObject.name);

        if (layerName == "UI")
        {
            Debug.Log("UI Clicked");
        }
        else if (layerName == "Panel")
        {
            Debug.Log("panel Clicked");
        }
        else if (layerName == "Bar Menu: Description")
        {
            Debug.Log("Bar Menu: Description  Clicked");
        }
        else if (layerName == "Bar Menu: button Close")
        {
            Debug.Log("Bar Menu: button Close Clicked");
        }
        else if (layerName == "Jadwal")
        {
            Debug.Log("Jadwal Clicked");
        }
        else if (layerName == "Bar Menu: Bar Navigation ")
        {
            Debug.Log("Bar Menu: Bar Navigation Clicked");
        }
        else if (layerName == "Bar Menu: Auto Rotate")
        {
            Debug.Log("Bar Menu: Auto Rotate Clicked");
        }

    }
}