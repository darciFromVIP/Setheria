using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveItemsBar : MonoBehaviour
{
    private List<ActiveItemSlot> activeItemSlots = new();
    private void Start()
    {
        foreach (var item in GetComponentsInChildren<ActiveItemSlot>())
        {
            activeItemSlots.Add(item);
        }
    }
    public bool UnlockSlot()
    {
        foreach (var item in activeItemSlots)
        {
            if (!item.isFree)
            {
                item.ToggleSlotAvailability(true);
                return true;
            }
        }
        return false;
    }
}
