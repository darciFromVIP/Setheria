using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DismantleSlot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        var inventoryItem = eventData.pointerDrag.GetComponent<InventoryItem>();
        if (inventoryItem == null)
            return;
        if (inventoryItem.parentAfterDrag == null)
            return;
        if (inventoryItem)
        {
            if (inventoryItem.item.requiredTalentForDismantle != null)
                if (FindObjectOfType<GameManager>().localPlayerCharacter.talentTrees.IsTalentUnlocked(inventoryItem.item.requiredTalentForDismantle, 1) >= 1)
                    DismantleItem(inventoryItem);
                else
                    FindObjectOfType<SystemMessages>().AddMessage("You are missing the talent " + inventoryItem.item.requiredTalentForDismantle.label + " to dismantle this item.", MsgType.Error);
            else
                DismantleItem(inventoryItem);
        }
    }
    public void DismantleItem(InventoryItem inventoryItem)
    {
        if (inventoryItem.item.itemsAfterDismantle.Count > 0)
        {
            int random;
            int temp;
            foreach (var item in inventoryItem.item.itemsAfterDismantle)
            {
                for (int i = 0; i < inventoryItem.stacks; i++)
                {
                    random = Random.Range(0, 100);
                    temp = 0;
                    foreach (var item2 in item.itemDropChances)
                    {
                        temp += item2.chance;
                        if (random >= temp - item2.chance && random < temp)
                        {
                            FindObjectOfType<InventoryManager>(true).AddItem(item2.item);
                        }
                    }
                }
            }
            inventoryItem.DestroyItem();
        }
        else
            FindObjectOfType<SystemMessages>().AddMessage("You cannot dismantle this item.");
    }
}
