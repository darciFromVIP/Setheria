using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class InventorySlot : MonoBehaviour, IDropHandler
{
    public Sprite freeSprite, lockSprite;
    public bool isFree = false;
    public void ToggleSlotAvailability(bool value)
    {
        isFree = value;
        if (isFree)
            GetComponent<Image>().sprite = freeSprite;
        else
            GetComponent<Image>().sprite = lockSprite;
    }
    public void OnDrop(PointerEventData eventData)
    {
        if (transform.childCount == 0 && isFree)
        {
            var inventoryItem = eventData.pointerDrag.GetComponent<InventoryItem>();
            if (inventoryItem.parentAfterDrag.TryGetComponent(out CharacterGearSlot slot))
            {
                var player = FindObjectOfType<GameManager>().localPlayerCharacter;
                foreach (var item in inventoryItem.item.passiveBuffs)
                {
                    if (item.buffType == BuffType.InventorySlots)
                    {
                        if (!FindObjectOfType<InventoryManager>(true).TestReduceInventory((int)item.value))
                            return;
                    }
                }
                foreach (var item in inventoryItem.item.passiveBuffs)
                {
                    player.CmdRemoveBuff(item);
                }
            }
            if (inventoryItem.parentAfterDrag.TryGetComponent(out StashSlot stashSlot))
            {
                stashSlot.CmdDeleteItemOnClients();
            }
            inventoryItem.parentAfterDrag = transform;
        }
    }
}
