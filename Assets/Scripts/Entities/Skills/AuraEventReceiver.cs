using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class AuraEventReceiver : MonoBehaviour
{
    public UnityEvent<Collider> On_Trigger_Stay = new();
    public UnityEvent<Collider> On_Trigger_Exit = new();

    private void OnTriggerStay(Collider other)
    {
        On_Trigger_Stay.Invoke(other);
    }
    private void OnTriggerExit(Collider other)
    {
        On_Trigger_Exit.Invoke(other);
    }
}
