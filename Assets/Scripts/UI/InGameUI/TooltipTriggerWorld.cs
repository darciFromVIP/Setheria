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
    private void OnMouseOver()
    {
        if (FogOfWarTeam.GetTeam(0).GetFogValue(transform.position) >= 255 / 1.2f)
            return;
        var eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> results = new();
        EventSystem.current.RaycastAll(eventData, results);
        if (results.Count > 0)
        {
            FindObjectOfType<TooltipWorld>(true).Hide();
            return;
        }
        FindObjectOfType<TooltipWorld>(true).Show(objectName, keybindType, keybindLabel);
    }
    private void OnMouseExit()
    {
        FindObjectOfType<TooltipWorld>(true).Hide();
    }
}
