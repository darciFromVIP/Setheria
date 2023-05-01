using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public enum KeybindType
{
    Move, Target, Interact, CharacterScreen, Manual, Inventory, CameraLock
}
public class Keybind : MonoBehaviour
{
    public KeybindType keybindType;
    public Image img;
    public TextMeshProUGUI text;

    public void UpdateKeybind()
    {

    }
}
