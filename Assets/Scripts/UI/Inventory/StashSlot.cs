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
            if (inventoryItem.parentAfterDrag.TryGetComponent(out CharacterGearSlot slot))
            {
                return;
            }
            StartCoroutine(DelayedCmd(inventoryItem.item.name, inventoryItem.stacks));
            inventoryItem.DestroyItem();
        }
    }
    private IEnumerator DelayedCmd(string itemName, int stacks)
    {
        float time = 1;
        while (time > 0)
        {
            time -= Time.deltaTime;
            yield return null;
        }
        CmdSpawnItemOnThisSlot(itemName, stacks);
    }
    [Command(requiresAuthority = false)]
    private void CmdSpawnItemOnThisSlot(string itemName, int stacks)
    {
        var newItem = Instantiate(inventoryItemPrefab, transform);
        Debug.Log(newItem.name);
        NetworkServer.Spawn(newItem.gameObject);
        RpcUpdateNewItem(newItem.GetComponent<NetworkIdentity>(), itemName, stacks);
    }
    [ClientRpc]
    private void RpcUpdateNewItem(NetworkIdentity item, string itemName, int stacks)
    {
        item.GetComponent<InventoryItem>().InitializeItem(itemDatabase.GetItemByName(itemName), stacks);
    }
}
