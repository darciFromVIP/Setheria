using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanPickup : MonoBehaviour
{
    private Item itemToPickup;
    private CanMove canMoveComp;
    private void Start()
    {
        canMoveComp = GetComponent<CanMove>();
    }
    public IEnumerator GoToPickup(Item item)
    {
        if (itemToPickup == item)
            yield break;
        itemToPickup = item;
        canMoveComp.MoveTo(item.transform.position);
        var originDest = canMoveComp.agent.destination;
        while (true)
        {
            if (originDest != canMoveComp.agent.destination)
            {
                itemToPickup = null;
                yield break;
            }
            if (canMoveComp.HasReachedDestination())
                break;
            yield return null;
        }
        canMoveComp.Stop();
        FindObjectOfType<AudioManager>().ItemPickUp(transform.position);
        FindObjectOfType<InventoryManager>(true).AddItem(itemToPickup);
        FindObjectOfType<TooltipWorld>(true).Hide();
        itemToPickup.DestroyItem();
        itemToPickup = null;
    }
}
