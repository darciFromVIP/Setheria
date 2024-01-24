using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    private Camera cam;
    private Vector3 mouseWorldPosStart;
    private float zoomScale = 100;
    private float zoomMin = 100f;
    private float zoomMax = 700f;
    private bool dragPanModeActive;
    private Vector2 lastMousePosition;
    private Vector3 defaultCameraPosition;
    private float defaultCameraSize;
    private void Awake()
    {
        cam = GetComponent<Camera>();
        defaultCameraPosition = transform.position;
        defaultCameraSize = cam.orthographicSize;
    }

    void Update()
    {
        if (Input.GetMouseButton(1))
            Pan();
        Zoom(Input.GetAxis("Mouse ScrollWheel"));
        if (Input.GetKeyDown(KeyCode.Space))
        {
            transform.position = defaultCameraPosition;
            cam.orthographicSize = defaultCameraSize;
        }
    }

    private void Pan()
    {
        if (Input.GetMouseButtonDown(1))
        {
            dragPanModeActive = true;
            lastMousePosition = Input.mousePosition;

        }
        if (Input.GetMouseButtonUp(1))
        {
            dragPanModeActive = false;
        }

        if (dragPanModeActive)
        {
            Vector2 mouseMovementDelta = (Vector2)Input.mousePosition - lastMousePosition;
            Vector3 moveDir = new Vector3(-mouseMovementDelta.x, -mouseMovementDelta.y, 0);
            transform.position += moveDir;
            lastMousePosition = Input.mousePosition;
        }
    }

    private void Zoom(float zoomDiff)
    {
        if (zoomDiff != 0)
        {
            mouseWorldPosStart = cam.ScreenToWorldPoint(Input.mousePosition);
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - zoomDiff * zoomScale, zoomMin, zoomMax);
            Vector3 mouseWorldPosDiff = mouseWorldPosStart - cam.ScreenToWorldPoint(Input.mousePosition);
            transform.position += mouseWorldPosDiff;
        }
    }
}