using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using FoW;
public class TooltipTriggerWorld : MonoBehaviour
{
    public string objectName;
    public string keybindLabel;
    public KeybindType keybindType;
    public void Show(TooltipWorld tooltip)
    {
        if (FogOfWarTeam.GetTeam(0).GetFogValue(transform.position) >= 255 / 1.2f)
            return;
        tooltip.Show(objectName, keybindType, keybindLabel);
    }
}
