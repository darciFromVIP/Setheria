using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyList : NetworkBehaviour
{
    public Transform content;
    public PartyMember prefab;
    [Command(requiresAuthority = false)]
    public void CmdAddPartyMember(NetworkIdentity member)
    {
        RpcAddPartyMember(member);
    }
    [ClientRpc]
    private void RpcAddPartyMember(NetworkIdentity member)
    {
        var instance = Instantiate(prefab, content);
        instance.Initialize(member.GetComponent<PlayerCharacter>());
    }
    public void TogglePartyList()
    {
        content.gameObject.SetActive(!content.gameObject.activeSelf);
    }
}
