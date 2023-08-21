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
            if (!inventoryItem)
                return;
            else if (inventoryItem.parentAfterDrag.TryGetComponent(out CharacterGearSlot slot))
                return;
            CmdSpawnItemOnThisSlot(inventoryItem.item.name, inventoryItem.stacks);
            inventoryItem.DestroyItem();
        }
    }
    [Command(requiresAuthority = false)]
    public void CmdSpawnItemOnThisSlot(string itemName, int stacks)
    {
        RpcUpdateNewItem(itemName, stacks);
    }
    [ClientRpc]
    private void RpcUpdateNewItem(string itemName, int stacks)
    {
        var newItem = Instantiate(inventoryItemPrefab, transform);
        newItem.GetComponent<InventoryItem>().InitializeItem(itemDatabase.GetItemByName(itemName), stacks);
    }
    [Command(requiresAuthority = false)]
    public void CmdDeleteItemOnClients()
    {
        RpcDeleteItemOnClients();
    }
    [ClientRpc]
    private void RpcDeleteItemOnClients()
    {
        var item = GetComponentInChildren<InventoryItem>();
        if (item)
            Destroy(item.gameObject);
    }
    [Command(requiresAuthority = false)]
    public void CmdChangeStacks(int stacks)
    {
        RpcChangeStacks(stacks);
    }
    [ClientRpc]
    private void RpcChangeStacks(int stacks)
    {
        var item = GetComponentInChildren<InventoryItem>();
        if (item)
            item.ChangeStacks(stacks, false);
    }
}
