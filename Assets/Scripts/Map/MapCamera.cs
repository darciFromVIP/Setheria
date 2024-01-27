using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCamera : MonoBehaviour
{
    private Camera cam;
    private Vector3 mouseWorldPosStart;
    private float zoomScale = 100;
    private float zoomMin = 50;
    private float zoomMax = 540;
    private bool dragPanModeActive;
    private Vector2 lastMousePosition;

    public float mapMinX, mapMaxX, mapMinY, mapMaxY;
    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        if (Input.GetMouseButton(1))
            Pan();
        Zoom(Input.GetAxis("Mouse ScrollWheel"));
        ClampCamera();
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
            transform.position += moveDir * 0.3f;
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
    private void ClampCamera()
    {
        float camHeight = cam.orthographicSize;
        float camWidth = cam.orthographicSize * cam.aspect;

        float minX = mapMinX + camWidth;
        float maxX = mapMaxX - camWidth;
        float minY = mapMinY + camHeight;
        float maxY = mapMaxY - camHeight;

        float newX = Mathf.Clamp(transform.position.x, minX, maxX);
        float newY = Mathf.Clamp(transform.position.y, minY, maxY);

        transform.position = new Vector3(newX, newY, transform.position.z);
    }
}