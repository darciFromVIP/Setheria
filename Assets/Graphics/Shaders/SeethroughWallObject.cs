using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeethroughWallObject : MonoBehaviour
{
    public static int PosID = Shader.PropertyToID("_Player_Position");
    public static int SizeID = Shader.PropertyToID("_Size");

    public Material WallMaterial;
    public Camera cam;
    public LayerMask mask;

    void Update()
    {
        var dir = cam.transform.position - transform.position;
        var ray = new Ray(transform.position, dir.normalized);

        if (Physics.Raycast(ray, 99999, mask))
            WallMaterial.SetFloat(SizeID, 1);
        else
            WallMaterial.SetFloat(SizeID, 0);

        var view = cam.WorldToViewportPoint(transform.position);
        WallMaterial.SetVector(PosID, view);
    }
}
