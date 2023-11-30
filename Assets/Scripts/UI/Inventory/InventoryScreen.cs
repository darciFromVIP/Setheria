using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryScreen : MonoBehaviour
{
    public GameObject window;
    public EventScriptable Character_Screen_Toggled;
    private SettingsManager settingsManager;
    private Vector3 defaultWindowPosition;
    private Vector3 customWindowPosition;

    private void Start()
    {
        GetComponentInChildren<InventoryManager>(true).InitializeInventory();
        settingsManager = FindObjectOfType<SettingsManager>();
        defaultWindowPosition = window.transform.position;
        customWindowPosition = defaultWindowPosition;
        Character_Screen_Toggled.boolEvent.AddListener(CharacterScreenToggled);
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
    public void HideWindow()
    {
        window.SetActive(false);
        FindObjectOfType<AudioManager>().InventoryClose();
        FindObjectOfType<Tooltip>(true).Hide();
    }
    public void HideWindowWithoutSound()
    {
        window.SetActive(false);
        FindObjectOfType<Tooltip>(true).Hide();
    }
    public void ShowWindow()
    {
        window.SetActive(true);
        FindObjectOfType<AudioManager>().InventoryOpen();
        FindObjectOfType<Tooltip>(true).Hide();
    }
    public void ShowWindowWithoutSound()
    {
        window.SetActive(true);
        FindObjectOfType<Tooltip>(true).Hide();
    }
    private void CharacterScreenToggled(bool value)
    {
        if (value)
        {
            customWindowPosition = window.transform.position;
            if (!window.activeSelf)
                ShowWindowWithoutSound();
            window.transform.position = defaultWindowPosition;
        }
        else
        {
            window.transform.position = customWindowPosition;
            HideWindowWithoutSound();
        }
    }
}
