using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapIconPrefab : MonoBehaviour
{
    public GameObject checkmarkImage;

    public void ToggleCheckmark()
    {
        checkmarkImage.SetActive(true);
    }
}
