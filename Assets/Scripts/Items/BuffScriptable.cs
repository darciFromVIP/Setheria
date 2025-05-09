using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum BuffType
{
    Bleed, Slow, Stun, Regen, Fear,
    MaxHealth, MaxMana, ManaRegen, InventorySlots, Power, 
    CriticalChance, CriticalDamage, AttackSpeedMultiplier, Armor, CooldownReduction, AttackRange, Speed, BaseAttackSpeed, 
    PowerScaling, CorruptionResistance, Corruption, Sleep, Invulnerability,
    PowerMultiplier
}
[CreateAssetMenu(menuName = "Buff")]
public class BuffScriptable : ScriptableObject
{
    public string buffName;
    public Sprite sprite;
    [TextArea(3, 3)]
    public string description;
    public BuffType buffType;
    public bool positiveBuff;
    public float value;
    public float duration;
    public bool stackable;
    public int maxStacks = 1;
    public EventReference sound;
    private void OnEnable()
    {
        buffName = name;
    }
}
