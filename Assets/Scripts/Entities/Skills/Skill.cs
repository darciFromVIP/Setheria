using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Skill : ScriptableObject
{
    protected Character castingEntity;
    [Header("General Settings")]
    public Sprite icon;
    public float cooldown;
    public int manaCost;
    public EventReference sound;
    [Header("Specific Settings")]
    [HideInInspector] public string description;
    public TalentScriptable requiredTalent;
    public byte requiredTalentLevel;
    [HideInInspector] public bool unlocked = true;
    public virtual void ExecuteOnStart(Character self)
    {

    }
    public virtual void ExecuteOnStart(Structure self)
    {

    }
    public virtual void Execute(Character self)
    {
        castingEntity = self;
        PlayerController player = castingEntity.GetComponent<PlayerController>();
        if (player)
        {
            player.SetCurrentSkill(this);
            player.ChangeState(PlayerState.Casting);
            player.Ground_Left_Clicked.RemoveAllListeners();
        }
    }
    public virtual void StopExecute()
    {

    }
    public virtual void UpdateDescription()
    {
        if (requiredTalent != null)
            unlocked = (castingEntity as PlayerCharacter).talentTrees.IsTalentUnlocked(requiredTalent, requiredTalentLevel);
        (castingEntity as PlayerCharacter).Skills_Changed.Invoke(castingEntity.skills);
    }
    public void SetCastingEntity(Character self)
    {
        castingEntity = self;
        UpdateDescription();
    }
    protected float GetScalingStatValue(PlayerStat stat)
    {
        if (!castingEntity)
            return 0;
        switch (stat)
        {
            case PlayerStat.Health:
                return castingEntity.GetComponent<HasHealth>().GetHealth();
            case PlayerStat.MaxHealth:
                return castingEntity.GetComponent<HasHealth>().GetFinalMaxHealth();
            case PlayerStat.HealthRegen:
                return castingEntity.GetComponent<HasHealth>().GetFinalHealthRegen();
            case PlayerStat.Mana:
                return castingEntity.GetComponent<HasMana>().GetMana();
            case PlayerStat.MaxMana:
                return castingEntity.GetComponent<HasMana>().GetFinalMaxMana();
            case PlayerStat.ManaRegen:
                return castingEntity.GetComponent<HasMana>().GetFinalManaRegen();
            case PlayerStat.Power:
                return castingEntity.GetComponent<CanAttack>().GetPower();
            case PlayerStat.AttackSpeed:
                return castingEntity.GetComponent<CanAttack>().GetAttackSpeed();
            case PlayerStat.CriticalChance:
                return castingEntity.GetComponent<CanAttack>().GetCritChance();
            case PlayerStat.CriticalDamage:
                return castingEntity.GetComponent<CanAttack>().GetCritDamage();
            case PlayerStat.Armor:
                return castingEntity.GetComponent<HasHealth>().GetArmor();
            case PlayerStat.CooldownReduction:
                return castingEntity.GetComponent<CanAttack>().GetCooldownReduction();
            case PlayerStat.MovementSpeed:
                return castingEntity.GetComponent<CanMove>().GetFinalMovementSpeed();
            case PlayerStat.Level:
                return castingEntity.level;
            default:
                return 0;
        }
    }
    protected string GetTextIconByStat(PlayerStat stat)
    {
        if (!castingEntity)
            return "";
        switch (stat)
        {
            case PlayerStat.MaxHealth:
                return "<sprite=5>";
            case PlayerStat.HealthRegen:
                return "<sprite=1>";
            case PlayerStat.MaxMana:
                return "<sprite=4>";
            case PlayerStat.ManaRegen:
                return "<sprite=6>";
            case PlayerStat.Power:
                return "<sprite=3>";
            case PlayerStat.AttackSpeed:
                return "<sprite=9>";
            case PlayerStat.CriticalChance:
                return "<sprite=7>";
            case PlayerStat.CriticalDamage:
                return "<sprite=8>";
            case PlayerStat.Armor:
                return "<sprite=0>";
            case PlayerStat.CooldownReduction:
                return "<sprite=10>";
            case PlayerStat.Hunger:
                return "<sprite=12>";
            case PlayerStat.MaxHunger:
                return "<sprite=12>";
            case PlayerStat.Resources:
                return "<sprite=15>";
            case PlayerStat.Knowledge:
                return "<sprite=11>";
            case PlayerStat.AttributePoint:
                return "<sprite=2>";
            case PlayerStat.MovementSpeed:
                return "<sprite=16>";
            case PlayerStat.Level:
                return "<sprite=13>";
            default:
                return "";
        }
    }
}