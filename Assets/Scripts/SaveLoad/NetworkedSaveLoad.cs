using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkedSaveLoad : NetworkBehaviour
{
    [SerializeField] private SaveLoadSystem saveLoadSystem;
    private void Start()
    {
        saveLoadSystem = FindObjectOfType<SaveLoadSystem>();
    }
    [Command(requiresAuthority = false)]
    public void CmdSave()
    {
        saveLoadSystem.Save();
    }
    [Command(requiresAuthority = false)]
    public void CmdSavePlayerState(SaveDataPlayer saveData)
    {
        saveLoadSystem.SavePlayerState(saveData);
    }
    [Command(requiresAuthority = false)]    
    public void CmdLoad()                   // not functional enough
    {
        RpcLoad(saveLoadSystem.currentWorldDataServer.worldSaveData);
        foreach (var item in FindObjectsOfType<PlayerCharacter>())
        {
            item.LoadCharacterFromServer();
        }
    }
    [ClientRpc]
    private void RpcLoad(SaveDataWorld state)
    {
        StartCoroutine(FindObjectOfType<WorldGenerator>().LoadWorldState(state));
    }
}
