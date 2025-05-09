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
    public EventReference UnlockRecipe;
    public EventReference BaseUnderAttack;
    public EventReference PlayerDeath;
    [Header("Hero Sounds")]
    public EventReference LevelUp;
    [Header("World Sounds")]
    public EventReference BuildingFinished;
    public EventReference BuildingDestroyed;
    public EventReference ItemPickUp;
    public EventReference ResourcesCollected;
    public EventReference ItemCrafted;
    public EventReference EatFood;
    public EventReference DrinkWater;
    public EventReference Repair;
}
