using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableOutlineOnMouseOver : MonoBehaviour
{
    private Outline outline;
    private void Start()
    {
        outline = GetComponentInChildren<Outline>();
    }
    private void OnMouseEnter()
    {
        outline.enabled = true;
    }
    private void OnMouseExit()
    {
        outline.enabled = false;
    }

}
