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
        GetComponent<CanDropItem>().SpawnItemsInInventory(FindObjectOfType<InventoryManager>());
        turnInComp.enabled = false;
        ChestLooted();
    }
    [Command(requiresAuthority = false)]
    private void ChestLooted()
    {
        looted = true;
        GetComponent<TooltipTriggerWorld>().objectName = "Treasure Chest (Looted)";
        turnInComp.enabled = false;
    }
}
