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
        if (outline)
            outline.enabled = true;
    }
    private void OnMouseExit()
    {
        if (outline)
            outline.enabled = false;
    }

}
