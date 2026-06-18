using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CameraMovement : MonoBehaviour
{
    [FormerlySerializedAs("_cam")]
    [SerializeField]
    private Camera cam;

    [SerializeField] private float zoomStep, minCamSize, maxCamSize;
    [SerializeField] private Image vignetteOverlay;

    [Header("Bounds")]
    public float boundX = 18f;
    public float boundY = 10f;

    private Vector3 dragOrigin;

    private void Start()
    {
        if (cam != null)
            cam.orthographicSize = maxCamSize;
    }

    private void Update()
    {
        PanCamera();

        float scrollValue = Mouse.current.scroll.ReadValue().y;
        if (scrollValue > 0f) ZoomIn();
        else if (scrollValue < 0f) ZoomOut();

        ClampCamera();
        UpdateVignetteAlpha();
    }

    private void PanCamera()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            dragOrigin = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        }

        if (!Mouse.current.leftButton.isPressed) return;

        Vector3 currentPos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        cam.transform.position += dragOrigin - currentPos;
        dragOrigin = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        ClampCamera();
    }

    private void ClampCamera()
    {
        float camHeight = cam.orthographicSize;
        float camWidth = cam.orthographicSize * cam.aspect;

        float clampedX = camWidth >= boundX ? 0f :
            Mathf.Clamp(cam.transform.position.x, -boundX + camWidth, boundX - camWidth);

        float clampedY = camHeight >= boundY ? 0f :
            Mathf.Clamp(cam.transform.position.y, -boundY + camHeight, boundY - camHeight);

        cam.transform.position = new Vector3(clampedX, clampedY, cam.transform.position.z);
    }

    public void ZoomIn()
    {
        float newSize = cam.orthographicSize - zoomStep;
        cam.orthographicSize = Mathf.Clamp(newSize, minCamSize, maxCamSize);
        dragOrigin = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    }

    public void ZoomOut()
    {
        float newSize = cam.orthographicSize + zoomStep;
        cam.orthographicSize = Mathf.Clamp(newSize, minCamSize, maxCamSize);
        dragOrigin = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
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