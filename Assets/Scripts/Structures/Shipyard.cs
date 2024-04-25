using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class Shipyard : NetworkBehaviour
{
    public float callShipsCooldown = 30;
    [HideInInspector] public float callShipsTimer = 0;
    private void Update()
    {
        if (callShipsTimer > 0)
            callShipsTimer -= Time.deltaTime;
    }
    public void CallShips()
    {
        StartCoroutine(WaitUntilCallShips(GetComponent<Structure>().unitSpawnPoint.position));
        callShipsTimer = callShipsCooldown;
        FindObjectOfType<SystemMessages>().AddMessage("All ships will return to this Shipyard in 30 seconds.", MsgType.Notice);
    }
    private IEnumerator WaitUntilCallShips(Vector3 point)
    {
        yield return new WaitForSeconds(callShipsCooldown);
        CmdCallShips(point);
    }
    [Command(requiresAuthority = false)]
    private void CmdCallShips(Vector3 point)
    {
        RpcCallShips(point);
    }
    [ClientRpc]
    private void RpcCallShips(Vector3 point)
    {
        foreach (var item in FindObjectsOfType<Ship>())
        {
            item.GetComponent<CanMove>().agent.enabled = false;
            item.transform.position = point;
            item.GetComponent<CanMove>().agent.enabled = true;
        }
    }
}
