using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlsWindow : MonoBehaviour
{
    public List<Keybind> keybinds = new();

    private void Awake()
    {
        foreach (var item in GetComponentsInChildren<Keybind>(true))
        {
            keybinds.Add(item);
        }
    }

    private void OnEnable()
    {
        UpdateKeybinds();        
    }
    public void UpdateKeybinds()
    {
        foreach (var item in keybinds)
        {
            item.UpdateKeybind();
        }
    }
}
