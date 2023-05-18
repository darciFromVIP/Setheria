using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlsWindow : MonoBehaviour
{
    private List<Keybind> keybinds = new();
    private SettingsManager settingsManager;

    private void Awake()
    {
        foreach (var item in GetComponentsInChildren<Keybind>(true))
        {
            keybinds.Add(item);
        }
        settingsManager = FindObjectOfType<SettingsManager>(true);
    }

    private void OnEnable()
    {
        UpdateKeybinds();        
    }
    public void UpdateKeybinds()
    {
        foreach (var item in keybinds)
        {
            item.UpdateKeybind(settingsManager.GetDataByKeybindType(item.keybindType));
        }
    }
}
