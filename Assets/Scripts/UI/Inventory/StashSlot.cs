using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class StashSlot : NetworkBehaviour, IDropHandler
{
    public ItemScriptableDatabase itemDatabase;
    public InventoryItem inventoryItemPrefab;
    public Sprite freeSprite, lockSprite;
    public bool isUnlocked = false;
    public bool isFree = true;
    public void ToggleSlotAvailability(bool value)
    {
        isUnlocked = value;
        if (isUnlocked)
            GetComponent<Image>().sprite = freeSprite;
        else
            GetComponent<Image>().sprite = lockSprite;
    }
    public void OnDrop(PointerEventData eventData)
    {
        if (isFree && isUnlocked)
        {
            var inventoryItem = eventData.pointerDrag.GetComponent<InventoryItem>();
            if (!inventoryItem)
                return;
            inventoryItem.DestroyTempObject();
            if (inventoryItem.parentAfterDrag.TryGetComponent(out CharacterGearSlot slot))
                return;
            else if (inventoryItem.parentAfterDrag.TryGetComponent(out StashSlot stashSlot))
                stashSlot.CmdDeleteItemOnClients();
            else
                inventoryItem.DestroyItem();
            CmdSpawnItemOnThisSlot(inventoryItem.item.name, inventoryItem.stacks);
        }
    }
    [Command(requiresAuthority = false)]
    public void CmdSpawnItemOnThisSlot(string itemName, int stacks)
    {
        RpcSpawnNewItem(itemName, stacks);
    }
    [ClientRpc]
    private void RpcSpawnNewItem(string itemName, int stacks)
    {
        SpawnNewItem(itemName, stacks);
    }
    public void SpawnNewItem(string itemName, int stacks)
    {
        var newItem = Instantiate(inventoryItemPrefab, transform);
        newItem.GetComponent<InventoryItem>().InitializeItem(itemDatabase.GetItemByName(itemName), stacks);
        isFree = false;
    }
    [Command(requiresAuthority = false)]
    public void CmdDeleteItemOnClients()
    {
        RpcDeleteItemOnClients();
    }
    [ClientRpc]
    private void RpcDeleteItemOnClients()
    {
        var item = GetComponentInChildren<InventoryItem>(true);
        if (item)
            Destroy(item.gameObject);
        isFree = true;
    }
    [Command(requiresAuthority = false)]
    public void CmdChangeStacks(int stacks)
    {
        RpcChangeStacks(stacks);
    }
    [ClientRpc]
    private void RpcChangeStacks(int stacks)
    {
        var item = GetComponentInChildren<InventoryItem>(true);
        if (item)
            item.ChangeStacks(stacks, false);
    }
}
