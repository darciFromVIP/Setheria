using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanPickup : NetworkBehaviour
{
    private Item itemToPickup;
    private CanMove canMoveComp;
    private PlayerController playerController;
    private void Start()
    {
        canMoveComp = GetComponent<CanMove>();
        playerController = GetComponent<PlayerController>();
    }
    [Command(requiresAuthority = false)]
    public void CmdGoToPickup(NetworkIdentity item)
    {
        StartCoroutine(GoToPickup(item));
    }
    public IEnumerator GoToPickup(NetworkIdentity item)
    {
        if (itemToPickup == item)
            yield break;
        itemToPickup = item.GetComponent<Item>();
        canMoveComp.MoveTo(item.transform.position);
        var originDest = canMoveComp.agent.destination;
        while (true)
        {
            if (originDest != canMoveComp.agent.destination)
            {
                itemToPickup = null;
                yield break;
            }
            if (playerController.ContainsCollider(item.GetComponent<Collider>()))
                break;
            yield return null;
        }
        canMoveComp.Stop();
        //canMoveComp.RpcStop();
        if (itemToPickup == null)
            yield break;
        FindObjectOfType<AudioManager>().ItemPickUp(transform.position);
        FindObjectOfType<InventoryManager>(true).AddItem(itemToPickup);
        FindObjectOfType<TooltipWorld>(true).Hide();
        itemToPickup.DestroyItem();
        itemToPickup = null;
    }
}
