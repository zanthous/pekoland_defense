using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class CameraZoomChangeZone : MonoBehaviour
{
    //What should the camera's zoom level change to after hitting this collider?
    [SerializeField] private CameraZoomLevel zoomLevel;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.transform.CompareTag("Player"))
        {
            Camera.ChangeCameraZoom?.Invoke(zoomLevel);
        }
    }
}
