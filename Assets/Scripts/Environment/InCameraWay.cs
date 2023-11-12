using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class InCameraWay : MonoBehaviour
{
    public GameObject objectToHide;
    private void Start()
    {
        if (objectToHide == null)
        {
            objectToHide = gameObject;
        }
    }
    public void TurnOff()
    {
        foreach (var item in objectToHide.GetComponentsInChildren<MeshRenderer>())
        {
            item.enabled = false;
        }
    }
    public void TurnOn()
    {
        foreach (var item in objectToHide.GetComponentsInChildren<MeshRenderer>())
        {
            item.enabled = true;
        }
    }
}
