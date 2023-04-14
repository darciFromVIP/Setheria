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
                    SpawnItem(new SaveDataItem { name = item2.item.itemData.name, stacks = item2.item.stacks }, transform.position);
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
                }
            }
        }
    }
    [Command(requiresAuthority = false)]
    public void SpawnItem(SaveDataItem item, Vector3 position)
    {
        RaycastHit hit;
        Ray ray = new Ray(position, Vector3.down);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Default", "Water")))
        {
            var newItem = Instantiate(itemDatabase.GetItemByName(item.name), hit.point, Quaternion.identity);
            NetworkServer.Spawn(newItem.gameObject);
            RpcUpdateCreatedItem(newItem.GetComponent<NetworkIdentity>(), item.stacks);
        }
    }
    [ClientRpc]
    private void RpcUpdateCreatedItem(NetworkIdentity item, int stacks)
    {
        item.GetComponent<Item>().stacks = stacks;
    }
}
