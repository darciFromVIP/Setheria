using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestTrigger : MonoBehaviour
{
    public QuestlineScriptable questlineTriggered;
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerCharacter player))
        {
            FindObjectOfType<QuestManager>().NewQuestline(questlineTriggered);
            Destroy(gameObject);
        }
    }
}
