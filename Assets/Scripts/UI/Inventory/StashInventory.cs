using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StashInventory : MonoBehaviour
{
    public List<InventorySlot> inventorySlots = new();

    public InventoryItem inventoryItemPrefab;

    public ItemScriptableDatabase itemDatabase;
    public void InitializeInventory()
    {
        foreach (var item in GetComponentsInChildren<InventorySlot>(true))
        {
            inventorySlots.Add(item);
        }
    }
    public bool AddItem(Item item)
    {
        return AddItem(item.itemData, item.stacks);
    }
    public bool AddItem(ItemScriptable item, int stacks)
    {
        if (item.stackable)
        {
            foreach (var slot in inventorySlots)
            {
                if (slot.transform.childCount > 0)
                {
                    if (slot.GetComponentInChildren<InventoryItem>().item == item)
                    {
                        slot.GetComponentInChildren<InventoryItem>().ChangeStacks(stacks);
                        return true;
                    }
                }
            }
        }
        foreach (var slot in inventorySlots)
        {
            if (slot.transform.childCount == 0 && slot.isFree)
            {
                SpawnNewItem(item, stacks, slot);
                item.Item_Acquired.Invoke();
                return true;
            }
        }
        FindObjectOfType<SystemMessages>().AddMessage("Inventory is full!");
        return false;
    }
    public bool AddItem(ItemRecipeInfo itemData)
    {
        return AddItem(itemData.itemData, itemData.stacks);
    }
    public bool AddItem(SaveDataItem itemData)
    {
        return AddItem(itemDatabase.GetItemByName(itemData.name), itemData.stacks);
    }
    public void RemoveItem(ItemRecipeInfo itemToDestroy)
    {
        foreach (var item in inventorySlots)
        {
            if (item.transform.childCount > 0)
            {
                var temp = item.GetComponentInChildren<InventoryItem>();
                if (temp.item == itemToDestroy.itemData)
                {
                    if (temp.item.stackable)
                    {
                        temp.ChangeStacks(-itemToDestroy.stacks);
                    }
                    else
                    {
                        temp.transform.SetParent(null);
                        Destroy(temp.gameObject);
                    }
                }
            }
        }
    }

    private void SpawnNewItem(Item item, InventorySlot slot)
    {
        var newItem = Instantiate(inventoryItemPrefab, slot.transform);
        newItem.InitializeItem(item);
    }
    private void SpawnNewItem(ItemScriptable item, int stacks, InventorySlot slot)
    {
        var newItem = Instantiate(inventoryItemPrefab, slot.transform);
        newItem.InitializeItem(item, stacks);
    }
    public List<InventoryItem> GetAllItems()
    {
        List<InventoryItem> result = new();
        foreach (var item in inventorySlots)
        {
            if (item.transform.childCount > 0)
                result.Add(item.GetComponentInChildren<InventoryItem>());
        }
        return result;
    }
    public void ExtendInventory(int value)
    {
        foreach (var item in inventorySlots)
        {
            if (!item.isFree)
            {
                item.ToggleSlotAvailability(true);
                value--;
                if (value <= 0)
                    return;
            }
        }
    }
    public bool TestReduceInventory(int value)
    {
        for (int i = inventorySlots.Count - 1; i >= 0; i--)
        {
            if (inventorySlots[i].isFree && inventorySlots[i].transform.childCount == 0)
                value--;
        }
        if (value <= 0)
            return true;
        else 
            return false;
    }
    public void ReduceInventory(int value)
    {
        for (int i = inventorySlots.Count - 1; i >= 0; i--)
        {
            if (inventorySlots[i].isFree && inventorySlots[i].transform.childCount == 0)
            {
                inventorySlots[i].ToggleSlotAvailability(false);
                value--;
                if (value <= 0)
                    break;
            }
        }
    }
    public Transform GetFreeSlot()
    {
        foreach (var slot in inventorySlots)
        {
            if (slot.transform.childCount == 0 && slot.isFree)
            {
                return slot.transform;
            }
        }
        FindObjectOfType<SystemMessages>().AddMessage("Inventory is full!");
        return null;
    }
}
