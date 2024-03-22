using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class AuraEventReceiver : MonoBehaviour
{
    public UnityEvent<Collider> On_Trigger_Stay = new();
    public UnityEvent<Collider> On_Trigger_Exit = new();

    private List<Collider> enteredEntities = new List<Collider>();

    private void OnTriggerStay(Collider other)
    {
        On_Trigger_Stay.Invoke(other);
        if (!enteredEntities.Contains(other))
        {
            enteredEntities.Add(other);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        On_Trigger_Exit.Invoke(other);
        if (enteredEntities.Contains(other))
        {
            enteredEntities.Remove(other);
        }
    }
    private void OnDestroy()
    {
        foreach (var item in enteredEntities)
        {
            On_Trigger_Exit.Invoke(item);
        }
    }
}
