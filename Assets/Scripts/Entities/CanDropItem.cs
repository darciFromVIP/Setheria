using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
[System.Serializable]
public struct ItemDropChances
{
    public List<ItemDropChance> itemDropChances;
}
[System.Serializable]
public struct ItemDropChance
{
    public ItemRecipeInfo item;
    public int chance;
}
[System.Serializable]
public class CanDropItem : NetworkBehaviour
{
    public ItemPrefabDatabase itemDatabase;
    public List<ItemDropChances> itemDropsTable;
    [Tooltip("If this object drops any Fish, you have to assign this event. Otherwise, leave blank.")]
    public EventScriptable fishingQuestEvent;
    private HasHealth hpComp;
    private void Start()
    {
        hpComp = GetComponent<HasHealth>(); 
        if (isServer && hpComp)
            hpComp.On_Death.AddListener(DropItem);
    }
    private void DropItem()
    {
        int random;
        int temp;
        foreach (var item in itemDropsTable)
        {
            random = Random.Range(0, 100);
            temp = 0;
            foreach (var item2 in item.itemDropChances)
            {
                temp += item2.chance;
                if (random >= temp - item2.chance && random < temp)
                {
                    Vector3 finalPos = transform.position;
                    while (true)
                    {
                        Vector3 pos = transform.position + new Vector3(Random.Range(-2, 2), 0, Random.Range(-2, 2));
                        RaycastHit hit;
                        if (Physics.Raycast(new Ray(pos + Vector3.up, Vector3.down), out hit, 3, LayerMask.GetMask("Default", "Water")))
                        {
                            pos = hit.point;
                        }
                        if (Mathf.Abs(finalPos.y - pos.y) < 1)
                        {
                            finalPos = pos;
                            break;
                        }
                    }
                    SpawnItem(new SaveDataItem { name = item2.item.itemData.name, stacks = item2.item.stacks }, finalPos);
                }
            }
        }
    }
    public void SpawnItemsInInventory(InventoryManager inventoryManager)
    {
        int random;
        int temp;
        foreach (var item in itemDropsTable)
        {
            random = Random.Range(0, 100);
            temp = 0;
            foreach (var item2 in item.itemDropChances)
            {
                temp += item2.chance;
                if (random >= temp - item2.chance && random < temp)
                {
                    inventoryManager.AddItem(item2.item);
                    if (item2.item.itemData.itemType == ItemType.Fish)
                        fishingQuestEvent.theEvent.Invoke();
                }
            }
        }
    }
    [Command(requiresAuthority = false)]
    public void SpawnItem(SaveDataItem item, Vector3 position)
    {
        var newItem = Instantiate(itemDatabase.GetItemByName(item.name), position, Quaternion.identity);
        NetworkServer.Spawn(newItem.gameObject);
        RpcUpdateCreatedItem(newItem.GetComponent<NetworkIdentity>(), item.stacks);
    }
    [ClientRpc]
    private void RpcUpdateCreatedItem(NetworkIdentity item, int stacks)
    {
        item.GetComponent<Item>().stacks = stacks;
    }
}
