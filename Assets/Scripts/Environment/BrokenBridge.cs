using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BrokenBridge : NetworkBehaviour, ISaveable
{
    public GameObject repairedModel;

    private void Start()
    {
        GetComponent<TurnInItemsInteractable>().Items_Turned_In.AddListener(CmdRepairBridge);
    }
    [Command(requiresAuthority = false)]
    private void CmdRepairBridge()
    {
        RpcRepairBridge();
    }
    [ClientRpc]
    private void RpcRepairBridge()
    {
        repairedModel.SetActive(true);
        GetComponent<NavMeshObstacle>().enabled = false;
        Destroy(GetComponent<TurnInItemsInteractable>());
    }

    public SaveDataWorldObject SaveState()
    {
        return new SaveDataWorldObject()
        {
            boolData1 = repairedModel.activeSelf
        };
    }

    public void LoadState(SaveDataWorldObject state)
    {
        if (state.boolData1)
            CmdRepairBridge();
    }
}
