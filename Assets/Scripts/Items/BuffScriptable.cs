using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum BuffType
{
    Bleed, Slow, Stun, Regen, Fear,
    MaxHealth, MaxMana, ManaRegen, InventorySlots, Power, CriticalChance, CriticalDamage, AttackSpeed, Armor, CooldownReduction, AttackRange
}
[CreateAssetMenu(menuName = "Buff")]
public class BuffScriptable : ScriptableObject
{
    public string buffName;
    public Sprite sprite;
    public BuffType buffType;
    public float value;
    public float duration;
    private void OnEnable()
    {
        buffName = name;
    }
}
