using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableOutlineOnMouseOver : MonoBehaviour
{
    private void OnMouseEnter()
    {
        foreach (var item in GetComponentsInChildren<MeshRenderer>())
        {
            item.materials[1].SetFloat("_Enabled", 1);
        }
    }
    private void OnMouseExit()
    {
        foreach (var item in GetComponentsInChildren<MeshRenderer>())
        {
            item.materials[1].SetFloat("_Enabled", 0);
        }
    }

}
