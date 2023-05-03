using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EscapableUI : MonoBehaviour
{
    public GameObject window;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            window.SetActive(false);
    }
}
