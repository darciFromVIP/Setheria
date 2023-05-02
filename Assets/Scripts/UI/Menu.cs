using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
    public GameObject window;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            window.SetActive(!window.activeSelf);
    }
}
