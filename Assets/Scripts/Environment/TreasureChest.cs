using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
[RequireComponent(typeof(TurnInItemsInteractable), typeof(CanDropItem))]
public class TreasureChest : NetworkBehaviour
{
    private TurnInItemsInteractable turnInComp;
    private bool looted = false;
    private void Start()
    {
        turnInComp = GetComponent<TurnInItemsInteractable>();
        turnInComp.Items_Turned_In.AddListener(GiveLoot);
    }
    private void GiveLoot()
    {
        if (looted == true)
        {
            FindObjectOfType<SystemMessages>().AddMessage("This Chest has already been looted!");
            return;
        }
        FindObjectOfType<AudioManager>().ChestOpen(transform.position);
        GetComponent<CanDropItem>().SpawnItemsInInventory(FindObjectOfType<InventoryManager>());
        CmdChestLooted();
    }
    [Command(requiresAuthority = false)]
    private void CmdChestLooted()
    {
        RpcChestLooted();
    }
    [ClientRpc]
    private void RpcChestLooted()
    {
        looted = true;
        GetComponent<TooltipTriggerWorld>().objectName = "Treasure Chest (Looted)";
        Destroy(turnInComp);
    }
}
