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
        foreach (var item in inventoryItem.item.passiveBuffs)
        {
            player.CmdAddBuff(item.name);
        }
    }
}
