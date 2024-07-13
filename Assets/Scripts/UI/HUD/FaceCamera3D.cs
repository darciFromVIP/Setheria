using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera3D : MonoBehaviour
{
    private Transform toFaceCamera;
    private void Awake()
    {
        toFaceCamera = GetComponent<Transform>();
    }
    public void Update()
    {
        Camera camera = Camera.main;
        toFaceCamera.transform.LookAt(camera.transform.position, camera.transform.up);
    }
}
