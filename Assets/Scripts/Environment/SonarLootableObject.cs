using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SonarLootableObject : LootableObject
{
    protected override void Start()
    {
        base.Start();
        GetComponent<ObjectMapIcon>().Icon_Loaded.AddListener(TurnOffObject);
    }
    private void TurnOffObject()
    {
        gameObject.SetActive(false);
    }
    [Command(requiresAuthority = false)]
    public void CmdEnableObject()
    {
        RpcEnableObject();
    }
    [ClientRpc]
    public void RpcEnableObject()
    {
        gameObject.SetActive(true);
        UpdateLootability(true);
        GetComponent<ObjectMapIcon>().ToggleIconOnMap(true);
    }
    protected override void GiveLoot()
    {
        base.GiveLoot();
        UpdateLootability(false);
        TurnOffObject();
    }
}
