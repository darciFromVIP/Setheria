using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class Ship : Character, IInteractable
{
    public int crewCapacity = 1;
    public List<PlayerCharacter> crew = new();

    private ShipController shipController;
    protected override void Start()
    {
        base.Start();
        shipController = GetComponent<ShipController>();
    }
    public void Interact(PlayerCharacter player)
    {
        CmdInteract(player);
    }
    [Command(requiresAuthority = false)]
    public void CmdInteract(PlayerCharacter player, NetworkConnectionToClient conn = null)
    {
        if (crew.Count == 0)
            netIdentity.AssignClientAuthority(conn);
        RpcInteract(player);
    }
    [ClientRpc]
    private void RpcInteract(PlayerCharacter player)
    {
        if (crew.Count == crewCapacity)
        {
            Debug.Log("Ship is full");
            return;
        }
        player.BoardShip();
        player.transform.SetParent(transform);
        player.transform.position = transform.position + Vector3.up;
        crew.Add(player);
        shipController.enabled = true;
    }
    [Command(requiresAuthority = false)]
    public void UnloadCrew(Vector3 position)
    {
        RpcUnloadCrew(position);
    }
    [ClientRpc]
    private void RpcUnloadCrew(Vector3 position)
    {
        for (int i = crew.Count - 1; i >= 0; i--)
        {
            crew[i].UnboardShip(position);
            crew[i].transform.SetParent(null);
            crew.Remove(crew[i]);
        } 
        shipController.enabled = false;
    }
}
