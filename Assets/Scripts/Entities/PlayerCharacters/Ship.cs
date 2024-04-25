using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Ship : Character, IInteractable, ISaveable
{
    public int crewCapacity = 1;
    public List<PlayerCharacter> crew = new();
    [Tooltip("This has to match the prefab's name! For saving purposes")]
    public string nameOfShip;

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
        {
            if (isOwned)
                netIdentity.RemoveClientAuthority();
            netIdentity.AssignClientAuthority(conn);
        }
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

    public SaveDataWorldObject SaveState()
    {
        return new SaveDataWorldObject
        {
            positionX = transform.position.x,
            positionY = transform.position.y,
            positionZ = transform.position.z,
            rotationW = transform.rotation.w,
            rotationX = transform.rotation.x,
            rotationY = transform.rotation.y,  
            rotationZ = transform.rotation.z,
            name = nameOfShip,
        };
    }

    public void LoadState(SaveDataWorldObject state)
    {
        var moveComp = GetComponent<CanMove>();
        moveComp.agent.enabled = false;
        GetComponent<NetworkTransformUnreliable>().enabled = false;
        if (state.positionX == 0 && state.positionY == 0 && state.positionZ == 0)
            transform.position = FindObjectOfType<Shipyard>().GetComponent<Structure>().unitSpawnPoint.position;
        else
            transform.position = new Vector3(state.positionX, state.positionY, state.positionZ);
        transform.rotation = new Quaternion(state.rotationX, state.rotationY, state.rotationZ, state.rotationW);
        GetComponent<NetworkTransformUnreliable>().enabled = true;
        moveComp.agent.enabled = true;
    }
}
