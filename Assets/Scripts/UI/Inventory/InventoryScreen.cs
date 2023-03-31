using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryScreen : MonoBehaviour
{
    public GameObject window;

    private void Start()
    {
        GetComponentInChildren<InventoryManager>(true).InitializeInventory();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleWindow();
        }
    }
    public void ToggleWindow()
    {
        window.SetActive(!window.activeSelf);
        FindObjectOfType<Tooltip>(true).Hide();
    }
}
