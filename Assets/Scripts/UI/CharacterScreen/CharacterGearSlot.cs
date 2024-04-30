using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class CharacterGearSlot : MonoBehaviour, IDropHandler
{
    public ItemType itemType;

    public void OnDrop(PointerEventData eventData)
    {
        var inventoryItem = eventData.pointerDrag.GetComponent<InventoryItem>();
        if (inventoryItem)
        {
            inventoryItem.DestroyTempObject();
            if (inventoryItem.parentAfterDrag.TryGetComponent(out CharacterGearSlot slot))
                return;
            if (inventoryItem.item.itemType == itemType)
            {
                EquipItem(inventoryItem);
            }
        }
    }
    public void EquipItem(InventoryItem inventoryItem)
    {
        FindObjectOfType<Tooltip>(true).Hide();
        var player = FindObjectOfType<GameManager>().localPlayerCharacter;
        if (transform.childCount == 0)
            inventoryItem.parentAfterDrag = transform;
        else
        {
            foreach (var item in inventoryItem.item.passiveBuffs)
            {
                if (item.buffType == BuffType.InventorySlots)
                {
                    if (!FindObjectOfType<InventoryManager>(true).TestReduceInventory((int)item.value))
                        return;
                }
            }
            var unequippedItem = GetComponentInChildren<InventoryItem>(true);
            unequippedItem.transform.SetParent(inventoryItem.parentAfterDrag);
            foreach (var item in unequippedItem.item.passiveBuffs)
            {
                player.CmdRemoveBuff(item.name);
            }
            inventoryItem.parentAfterDrag = transform;
        }
        EquipItemWithoutMoving(inventoryItem);
        GetComponent<TooltipTrigger>().enabled = false;
    }
    public void EquipItemWithoutMoving(InventoryItem inventoryItem)
    {
        var player = FindObjectOfType<GameManager>().localPlayerCharacter;
        foreach (var item in inventoryItem.item.passiveBuffs)
        {
            if (item.buffType == BuffType.InventorySlots)
                player.AddBuff(item.name);
            else
                player.CmdAddBuff(item.name);
        }
    }
    public void UnequipItem()
    {
        GetComponent<TooltipTrigger>().enabled = true;
    }
}
