using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.UI;
using UnityEngine;

public class CharacterStatsWindow : MonoBehaviour
{
    public EventScriptable Character_Screen_Toggled;

    private void OnEnable()
    {
        Character_Screen_Toggled.boolEvent.Invoke(true);
    }
    private void OnDisable()
    {
        Character_Screen_Toggled.boolEvent.Invoke(false);
    }
}
