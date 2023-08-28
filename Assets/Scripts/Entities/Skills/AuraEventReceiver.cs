using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class AuraEventReceiver : MonoBehaviour
{
    public UnityEvent<Collider> On_Trigger_Enter = new();
    public UnityEvent<Collider> On_Trigger_Exit = new();

    private void OnTriggerEnter(Collider other)
    {
        On_Trigger_Enter.Invoke(other);
    }
    private void OnTriggerExit(Collider other)
    {
        On_Trigger_Exit.Invoke(other);
    }
}
