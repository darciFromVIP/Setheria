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
        StartCoroutine(WaitForStashToLoad());
    }
    private IEnumerator WaitForStashToLoad()
    {
        var stash = FindObjectOfType<StashInventory>(true);
        while (stash == null)
        {
            stash = FindObjectOfType<StashInventory>(true);
            yield return null;
        }
        stash.ExtendInventoryUpTo(stashSlots);
    }
}
