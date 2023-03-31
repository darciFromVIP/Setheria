using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class Item : NetworkBehaviour, ISaveable
{
    public ItemScriptable itemData;
    public int stacks;
    [Command(requiresAuthority = false)]
    public void DestroyItem()
    {
        NetworkServer.Destroy(gameObject);
    }
    public SaveDataWorldObject SaveState()
    {
        return new SaveDataWorldObject
        {
            name = itemData.name,
            positionX = transform.position.x,
            positionY = transform.position.y,
            positionZ = transform.position.z,
            intData1 = stacks
        };
    }
    public void LoadState(SaveDataWorldObject state)
    {
        transform.position = new Vector3(state.positionX, state.positionY, state.positionZ);
        stacks = state.intData1;
    }
    
}
