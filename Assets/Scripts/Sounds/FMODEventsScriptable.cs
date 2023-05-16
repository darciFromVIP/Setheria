using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
[CreateAssetMenu(menuName = "Databases/FMOD Events Database")]
public class FMODEventsScriptable : ScriptableObject
{
    [Header("UI Sounds")]
    public EventReference UIHover;
    public EventReference UIClick;
}
