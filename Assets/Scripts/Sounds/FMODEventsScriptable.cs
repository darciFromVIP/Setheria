using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
[CreateAssetMenu(menuName = "Databases/FMOD Events Database")]
public class FMODEventsScriptable : ScriptableObject
{
    [Header("UI Sounds")]
    public EventReference DayStart;
    public EventReference NightStart;
    public EventReference ManualOpen;
    public EventReference ManualClose;
    public EventReference UIHover;
    public EventReference UIClick;
    public EventReference UIInvalid;
    public EventReference InventoryOpen;
    public EventReference InventoryClose;
    public EventReference QuestAccepted;
    public EventReference QuestComplete;
    public EventReference ToolBreak;
    [Header("Hero Sounds")]
    public EventReference LevelUp;
    [Header("World Sounds")]
    public EventReference BuildingFinished;
    public EventReference BuildingDestroyed;
    public EventReference ChestOpen;
    public EventReference ItemPickUp;
    public EventReference ResourcesCollected;
    public EventReference ItemCrafted;
}
