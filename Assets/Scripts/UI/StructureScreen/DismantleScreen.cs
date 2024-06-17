using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DismantleScreen : MonoBehaviour, WindowedUI
{
    public GameObject window;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
            ToggleWindow(false);
    }
    public void ToggleWindow(bool value)
    {
        if (value)
        {
            window.SetActive(true);
        }
        else
        {
            window.SetActive(false);
        }
    }
    public void ShowWindow()
    {
        ToggleWindow(true);
    }

    public void HideWindow()
    {
        ToggleWindow(false);
    }

    public bool IsActive()
    {
        return window.activeSelf;
    }

}
