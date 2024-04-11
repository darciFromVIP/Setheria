using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanPickup : NetworkBehaviour
{
    private Item itemToPickup;
    private CanMove canMoveComp;
    private PlayerController playerController;
    private ShipController shipController;
    private void Start()
    {
        canMoveComp = GetComponent<CanMove>();
        playerController = GetComponent<PlayerController>();
        shipController = GetComponent<ShipController>();
    }
    public IEnumerator GoToPickup(Item item)
    {
        if (itemToPickup == item || item == null)
            yield break;
        itemToPickup = item;
        canMoveComp.MoveTo(item.transform.position);
        var originDest = canMoveComp.agent.destination;
        while (true)
        {
            if (item == null)
                yield break;
            if (originDest != canMoveComp.agent.destination)
            {
                itemToPickup = null;
                yield break;
            }
            if (shipController)
                if (shipController.ContainsCollider(item.GetComponent<Collider>()))
                    break;
            if (playerController)
                if (playerController.ContainsCollider(item.GetComponent<Collider>()))
                    break;
            yield return null;
        }
        canMoveComp.Stop();
        FindObjectOfType<AudioManager>().ItemPickUp(transform.position);
        if (itemToPickup == null)
            yield break;
        if (itemToPickup)
            FindObjectOfType<InventoryManager>(true).AddItem(itemToPickup);
        FindObjectOfType<TooltipWorld>(true).Hide();
        if (itemToPickup)
            itemToPickup.DestroyItem();
        itemToPickup = null;
    }
}
