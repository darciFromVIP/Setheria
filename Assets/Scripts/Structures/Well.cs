using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Well : NetworkBehaviour
{
    public float waterCooldown;
    private float waterTimer;
    public ItemScriptable waterItem;
    private void Update()
    {
        if (waterTimer > 0)
        {
            waterTimer -= Time.deltaTime;
        }
    }
    [Command(requiresAuthority = false)]
    public void CmdStartWaterCooldown()
    {
        RpcStartWaterCooldown();
    }
    [ClientRpc]
    private void RpcStartWaterCooldown()
    {
        waterTimer = waterCooldown;
    }
    public float GetWaterTimer()
    {
        return waterTimer;
    }
}
