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
        if (Input.GetKeyDown(settingsManager.settings.characterScreen))
        {
            ShowWindow();
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
    public void HideWindow()
    {
        window.SetActive(false);
        FindObjectOfType<AudioManager>().InventoryClose();
        FindObjectOfType<Tooltip>(true).Hide();
    }
    public void ShowWindow()
    {
        window.SetActive(true);
        FindObjectOfType<AudioManager>().InventoryOpen();
        FindObjectOfType<Tooltip>(true).Hide();
    }
}
