using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;
public class Cottage : Tent
{
    protected override void Start()
    {
        base.Start();
        if (isServer)
            RpcUpgradeStash();
    }
    [ClientRpc]
    private void RpcUpgradeStash()
    {
        FindObjectOfType<StashInventory>(true).ExtendInventoryUpTo(stashSlots);
    }
}
