using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryScreen : MonoBehaviour
{
    public GameObject window;
    private SettingsManager settingsManager;

    private void Start()
    {
        GetComponentInChildren<InventoryManager>(true).InitializeInventory();
        settingsManager = FindObjectOfType<SettingsManager>();
    }
    void Update()
    {
        if (Input.GetKeyDown(settingsManager.settings.inventory))
        {
            ToggleWindow();
        }
    }
    public void ToggleWindow()
    {
        window.SetActive(!window.activeSelf);
        if (window.activeSelf)
            FindObjectOfType<AudioManager>().InventoryOpen();
        else
            FindObjectOfType<AudioManager>().InventoryClose();
        FindObjectOfType<Tooltip>(true).Hide();
    }
}
