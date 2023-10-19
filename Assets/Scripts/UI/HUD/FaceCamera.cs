using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    private Transform toFaceCamera;
    private void Awake()
    {
        toFaceCamera = GetComponent<Transform>();
    }
    public void LateUpdate()
    {
        Camera camera = Camera.main;
        toFaceCamera.transform.LookAt(transform.position + camera.transform.rotation * Vector3.forward, camera.transform.rotation * Vector3.up);
    }
}
