using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class CameraMovement : MonoBehaviour
{
    [SerializeField] private Camera _cam;
    private Vector3 dragOrigin;


    // Update is called once per frame
    private void Update()
    {
        PanCamera();
    }

    private void PanCamera()
    {

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            dragOrigin = _cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());


        }

        if (!Mouse.current.leftButton.isPressed) return;
        Vector3 difference = dragOrigin - _cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        _cam.transform.position += difference;


    }
}