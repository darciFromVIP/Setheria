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
            switch (item.keybindType)
            {
                case KeybindType.Move:
                    item.UpdateKeybind(settingsManager.GetMoveData());
                    break;
                case KeybindType.Target:
                    item.UpdateKeybind(settingsManager.GetTargetData());
                    break;
                case KeybindType.Interact:
                    item.UpdateKeybind(settingsManager.GetInteractData());
                    break;
                case KeybindType.CharacterScreen:
                    item.UpdateKeybind(settingsManager.GetCharacterData());
                    break;
                case KeybindType.Manual:
                    item.UpdateKeybind(settingsManager.GetManualData());
                    break;
                case KeybindType.Inventory:
                    item.UpdateKeybind(settingsManager.GetInventoryData());
                    break;
                case KeybindType.CameraLock:
                    item.UpdateKeybind(settingsManager.GetCameraLockData());
                    break;
                default:
                    break;
            }
        }
    }
}
