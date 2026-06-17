using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CameraMovement : MonoBehaviour
{
    [FormerlySerializedAs("_cam")] [SerializeField]
    private Camera cam;

    private Vector3 dragOrigin;

    [SerializeField] private float zoomStep, minCamSize, maxCamSize;

    [SerializeField] private Image vignetteOverlay;


    private void Start()
    {

        if (cam != null)
        {
            cam.orthographicSize = maxCamSize;
        }
    }


    // Update is called once per frame
    private void Update()
    {
        PanCamera();

        float scrollValue = Mouse.current.scroll.ReadValue().y;
        if (scrollValue > 0f) // forward
        {
            ZoomIn();
        }
        else if (scrollValue < 0f) // backwards
        {
            ZoomOut();
        }
        
        UpdateVignetteAlpha();
        

    }

    private void PanCamera()
    {

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            dragOrigin = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());


        }

        if (!Mouse.current.leftButton.isPressed) return;
        Vector3 difference = dragOrigin - cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        cam.transform.position += difference;
    }


    public void ZoomIn()
    {
        float newSize = cam.orthographicSize - zoomStep;
        cam.orthographicSize = Mathf.Clamp(newSize, minCamSize, maxCamSize);
    }

    public void ZoomOut()
    {
        float newSize = cam.orthographicSize + zoomStep;
        cam.orthographicSize = Mathf.Clamp(newSize, minCamSize, maxCamSize);
    }

    private void UpdateVignetteAlpha()
    {
        if (vignetteOverlay == null) return;
        
        if (cam.orthographicSize >= maxCamSize)
        {
            Color clearColor = vignetteOverlay.color;
            clearColor.a = 0f;
            vignetteOverlay.color = clearColor;
            return;
        }
        
        float t = Mathf.InverseLerp(minCamSize, maxCamSize, cam.orthographicSize);
        float targetAlpha = 1f - t;

        Color color = vignetteOverlay.color;
        color.a = Mathf.Clamp01(targetAlpha);
        vignetteOverlay.color = color;
    }


}