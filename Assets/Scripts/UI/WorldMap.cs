using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMap : MonoBehaviour
{
    public GameObject mapWindow;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            mapWindow.SetActive(!mapWindow.activeSelf);
        }

    }
}
