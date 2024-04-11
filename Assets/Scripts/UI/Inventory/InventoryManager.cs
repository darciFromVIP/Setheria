using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class InventoryManager : MonoBehaviour, NeedsLocalPlayerCharacter
{
    public List<InventorySlot> inventorySlots = new();

    public InventoryItem inventoryItemPrefab;
    private PlayerCharacter localPlayerCharacter;

    public ItemScriptableDatabase itemDatabase;
    private bool equipableItemTutorial = true;
    public void InitializeInventory()
    {
        foreach (var item in GetComponentsInChildren<InventorySlot>(true))
        {
            inventorySlots.Add(item);
        }
    }
    public InventoryItem AddItem(Item item, bool stackable = true)
    {
        return AddItem(item.itemData, item.stacks, stackable);
    }
    public InventoryItem AddItem(ItemScriptable item, int stacks, bool stackable = true)
    {
        InventoryItem result = null;
        if (item == null)
            return result;
        if (item.stackable && stackable)
        {
            foreach (var slot in inventorySlots)
            {
                if (slot.transform.childCount > 0)
                {
                    var inventoryItem = slot.GetComponentInChildren<InventoryItem>(true);
                    if (inventoryItem != null)
                    { if (inventoryItem.item == item)
                        {
                            result = slot.GetComponentInChildren<InventoryItem>(true);
                            result.ChangeStacks(stacks);
                            item.Item_Stacks_Acquired.Invoke(item, stacks);
                            FindObjectOfType<AcquiredItems>().ItemAcquired(new ItemRecipeInfo { itemData = item, stacks = stacks });
                            return result;
                        }
                    }
                }
            }
        }
        foreach (var slot in inventorySlots)
        {
            if (slot.transform.childCount == 0 && slot.isFree)
            {
                result = SpawnNewItem(item, stacks, slot);
                item.Item_Acquired.Invoke(item);
                item.Item_Stacks_Acquired.Invoke(item, stacks);
                FindObjectOfType<AcquiredItems>().ItemAcquired(new ItemRecipeInfo { itemData = item, stacks = stacks });
                return result;
            }
        }
        localPlayerCharacter.CreateItem(new SaveDataItem() { name = item.name, stacks = stacks }, localPlayerCharacter.transform.position);
        if (item.itemType == ItemType.GatheringTool && equipableItemTutorial)
        {
            FindObjectOfType<SystemMessages>().AddMessage("Equip your new pickaxe by double clicking it!", MsgType.Notice);
            equipableItemTutorial = false;
        }
        FindObjectOfType<SystemMessages>().AddMessage("Inventory is full!");
        return result;
    }
    public InventoryItem AddItem(ItemRecipeInfo itemData, bool stackable = true)
    {
        return AddItem(itemData.itemData, itemData.stacks, stackable);
    }
    public InventoryItem AddItem(SaveDataItem itemData, bool stackable = true)
    {
        return AddItem(itemDatabase.GetItemByName(itemData.name), itemData.stacks, stackable);
    }
    public void RemoveItem(ItemRecipeInfo itemToDestroy)
    {
        foreach (var item in inventorySlots)
        {
            if (item.transform.childCount > 0)
            {
                var temp = item.GetComponentInChildren<InventoryItem>(true);
                if (temp != null)
                {
                    if (temp.item == itemToDestroy.itemData)
                    {
                        if (temp.item.stackable)
                        {
                            temp.ChangeStacks(-itemToDestroy.stacks);
                        }
                        else
                        {
                            itemToDestroy.itemData.Item_Stacks_Lost.Invoke(itemToDestroy.itemData, 1);
                            temp.transform.SetParent(null);
                            Destroy(temp.gameObject);
                        }
                        break;
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
    private InventoryItem SpawnNewItem(ItemScriptable item, int stacks, InventorySlot slot)
    {
        var newItem = Instantiate(inventoryItemPrefab, slot.transform);
        newItem.InitializeItem(item, stacks);
        return newItem;
    }
    public List<InventoryItem> GetAllItems()
    {
        List<InventoryItem> result = new();
        foreach (var item in inventorySlots)
        {
            if (item.transform.childCount > 0)
            {
                var inventoryItem = item.GetComponentInChildren<InventoryItem>(true);
                if (inventoryItem != null)
                    result.Add(inventoryItem);
            }
        }
        return result;
    }
    public InventoryItem GetItemOfName(string name)
    {
        InventoryItem result = null;
        foreach (var item in inventorySlots)
        {
            if (item.transform.childCount > 0)
            {
                var temp = item.transform.GetChild(0).GetComponent<InventoryItem>();
                if (temp != null)
                {
                    if (temp.item.name == name)
                        result = temp;
                }
            }
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
    public void DestroyAllItems()
    {
        foreach (var item in GetAllItems())
        {
            item.DestroyItem();
        }
    }
    public void SetLocalPlayerCharacter(PlayerCharacter player)
    {
        localPlayerCharacter = player;
    }
}
