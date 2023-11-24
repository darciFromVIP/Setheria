using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class TutorialArea : MonoBehaviour
{
    public TutorialDataScriptable tutData;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerCharacter character))
        {
            if (character.isOwned)
            {
                FindObjectOfType<Tutorial>().QueueNewTutorial(tutData);
                Destroy(gameObject);
            }
        }
    }
}
