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
}
