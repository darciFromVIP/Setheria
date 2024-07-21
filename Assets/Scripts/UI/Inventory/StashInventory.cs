using System.Collections.Generic;
using UnityEngine;

public class StashInventory : MonoBehaviour, WindowedUI
{
    public List<StashSlot> stashSlots = new();
    public GameObject window;

    public InventoryItem inventoryItemPrefab;

    public ItemScriptableDatabase itemDatabase;
    private void Start()
    {
        window.SetActive(true);
        InitializeInventory();
    }
    public void LoadStash()
    {
        foreach (var item in FindObjectOfType<WorldGenerator>().lastLoadedWorldState.stash)
        {
            AddItemOnClient(item);
        }
        window.SetActive(false);
        GetComponentInParent<StructureScreen>(true).HideWindow();
    }
    public void ShowWindow()
    {
        window.SetActive(true);
        FindObjectOfType<InventoryScreen>(true).ShowWindow();
    }
    public void HideWindow()
    {
        window.SetActive(false);
    }
    public void InitializeInventory()
    {
        foreach (var item in GetComponentsInChildren<StashSlot>(true))
        {
            stashSlots.Add(item);
        }
    }
    public bool AddItem(Item item)
    {
        return CmdAddItem(item.itemData, item.stacks);
    }
    public bool CmdAddItem(ItemScriptable item, int stacks)
    {
        if (item.stackable)
        {
            foreach (var slot in stashSlots)
            {
                if (slot.transform.childCount > 0)
                {
                    var inventoryItem = slot.GetComponentInChildren<InventoryItem>(true);
                    if (inventoryItem != null)
                    {
                        if (inventoryItem.item == item)
                        {
                            slot.CmdChangeStacks(stacks);
                            return true;
                        }
                    }
                }
            }
        }
        foreach (var slot in stashSlots)
        {
            if (slot.isFree && slot.isUnlocked)
            {
                slot.CmdSpawnItemOnThisSlot(item.name, stacks);
                slot.isFree = false;
                return true;
            }
        }
        FindObjectOfType<SystemMessages>().AddMessage("Inventory is full.");
        return false;
    }
    public bool AddItemOnClient(ItemScriptable item, int stacks, bool isLoading = false)
    {
        foreach (var slot in stashSlots)
        {
            if ((slot.isFree && slot.isUnlocked) || (isLoading && slot.isFree))
            {
                slot.SpawnNewItem(item.name, stacks);
                slot.isFree = false;
                return true;
            }
        }
        return false;
    }
    public bool AddItem(ItemRecipeInfo itemData)
    {
        return CmdAddItem(itemData.itemData, itemData.stacks);
    }
    public bool AddItemOnClient(SaveDataItem itemData)
    {
        return AddItemOnClient(itemDatabase.GetItemByName(itemData.name), itemData.stacks, true);
    }
    public void RemoveItem(ItemRecipeInfo itemToDestroy)
    {
        foreach (var item in stashSlots)
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
                            temp.transform.SetParent(null);
                            Destroy(temp.gameObject);
                            item.isFree = true;
                        }
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
        foreach (var item in stashSlots)
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
    public void ExtendInventoryUpTo(int finalSlots)
    {
        int currentSlots = 0;
        foreach (var item in stashSlots)
        {
            if (item.isUnlocked)
            {
                currentSlots++;
            }
        }
        ExtendInventory(finalSlots - currentSlots);
    }
    public void ExtendInventory(int value)
    {
        foreach (var item in stashSlots)
        {
            if (!item.isUnlocked)
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
        for (int i = stashSlots.Count - 1; i >= 0; i--)
        {
            if (stashSlots[i].isUnlocked && stashSlots[i].transform.childCount == 0)
                value--;
        }
        if (value <= 0)
            return true;
        else 
            return false;
    }
    public void ReduceInventory(int value)
    {
        for (int i = stashSlots.Count - 1; i >= 0; i--)
        {
            if (stashSlots[i].isUnlocked && stashSlots[i].transform.childCount == 0)
            {
                stashSlots[i].ToggleSlotAvailability(false);
                value--;
                if (value <= 0)
                    break;
            }
        }
    }
    public Transform GetFreeSlot()
    {
        foreach (var slot in stashSlots)
        {
            if (slot.isFree && slot.isUnlocked)
            {
                return slot.transform;
            }
        }
        FindObjectOfType<SystemMessages>().AddMessage("Inventory is full.");
        return null;
    }
    public InventoryItem GetItemOfName(string name)
    {
        InventoryItem result = null;
        foreach (var item in stashSlots)
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
    public bool IsActive()
    {
        return window.activeSelf;
    }
}
