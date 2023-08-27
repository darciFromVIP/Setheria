using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowWithCategories : MonoBehaviour
{
    public GameObject currentOpenedWindow;

    public virtual void OpenAnotherWindow(GameObject window)
    {
        if (currentOpenedWindow)
            currentOpenedWindow.SetActive(false);
        window.SetActive(true);
        currentOpenedWindow = window;
    }
}
