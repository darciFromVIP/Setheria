using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyList : NetworkBehaviour
{
    public Transform content;
    public PartyMember prefab;
    private List<PartyMember> partyMembers = new();
    [Command(requiresAuthority = false)]
    public void CmdAddPartyMember(NetworkIdentity member)
    {
        RpcAddPartyMember(member);
    }
    [ClientRpc]
    private void RpcAddPartyMember(NetworkIdentity member)
    {
        foreach (var item in partyMembers)
        {
            if (item.nameTag.text == member.GetComponent<PlayerCharacter>().nameTag.text)
                return;
        }
        var instance = Instantiate(prefab, content);
        instance.Initialize(member.GetComponent<PlayerCharacter>());
        partyMembers.Add(instance);
    }
    public void TogglePartyList()
    {
        content.gameObject.SetActive(!content.gameObject.activeSelf);
    }
}
