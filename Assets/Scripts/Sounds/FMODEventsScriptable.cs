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
    public EventReference UIHover;
    public EventReference UIClick;
    [Header("Hero Sounds")]
    public EventReference LevelUp;
    [Header("Lycandruid")]
    public EventReference ClawAttack;
    public EventReference Swipe;
    public EventReference Uppercut;
    public EventReference Roar;
    public EventReference WildRage;
    public EventReference PawAttack;
    public EventReference Bite;
    public EventReference PounceJump; 
    public EventReference PounceImpact;
    public EventReference Howl;
    public EventReference CallOfTheWild;
    [Header("Forest Protector")]
    public EventReference GreenDust;
    public EventReference Rejuvenation;
    public EventReference EntanglingRoots;
    public EventReference FlowerPowerCast;
    public EventReference FlowerPowerGrow;
    public EventReference FlowerPowerHeal;
}
