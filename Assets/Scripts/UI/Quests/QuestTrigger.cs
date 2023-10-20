using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestTrigger : NetworkBehaviour
{
    public QuestlineScriptable questlineTriggered;
    public bool giveQuestlineToAllPlayers = true;
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerCharacter player))
        {
            if (player.isClient && player.isOwned)
            {
                if (giveQuestlineToAllPlayers)
                    FindObjectOfType<QuestManager>().CmdNewQuestline(questlineTriggered.name);
                else
                    FindObjectOfType<QuestManager>().NewQuestline(questlineTriggered.name);
                DestroyObject();
            }
        }
    }
    [Command(requiresAuthority = false)]
    private void DestroyObject()
    {
        NetworkServer.Destroy(gameObject);
    }
}
