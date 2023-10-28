using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Event Scriptable")]
public class EventScriptable : ScriptableObject
{
    public UnityEvent voiiddEEvveenntt = new();

    public UnityEvent<PlayerCharacter> playerEvent = new();
}