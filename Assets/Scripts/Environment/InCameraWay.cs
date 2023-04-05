using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InCameraWay : MonoBehaviour
{
    public void TurnOff()
    {
        foreach (var item in GetComponentsInChildren<MeshRenderer>())
        {
            item.enabled = false;
        }
    }
    public void TurnOn()
    {
        foreach (var item in GetComponentsInChildren<MeshRenderer>())
        {
            item.enabled = true;
        }
    }
}
