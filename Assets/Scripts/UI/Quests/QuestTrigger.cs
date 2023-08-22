using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestTrigger : MonoBehaviour
{
    public QuestlineScriptable questlineTriggered;
    public bool giveQuestlineToAllPlayers = true;
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerCharacter player))
        {
            if (giveQuestlineToAllPlayers)
                FindObjectOfType<QuestManager>().CmdNewQuestline(questlineTriggered.name);
            else
                FindObjectOfType<QuestManager>().NewQuestline(questlineTriggered.name);
            Destroy(gameObject);
        }
    }
}
