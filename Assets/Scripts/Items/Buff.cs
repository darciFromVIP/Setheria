using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public abstract class Buff
{
    public string name;
    public BuffType buffType;
    public float value;
    public float durationTimer;
    public Character targetEntity;
    public GameObject effect;

    public UnityEvent Buff_Expired = new();

    public abstract void BuffExpired();
    public virtual IEnumerator TimedBuff(float duration)
    {
        durationTimer = duration;
        while (durationTimer > 0)
        {
            durationTimer -= Time.deltaTime;
            yield return null;
        }
        Buff_Expired.Invoke();
        targetEntity.BuffExpired(effect);
        BuffExpired();
        targetEntity.buffs.Remove(this);
    }
}
public class BMaxHealth : Buff
{
    public BMaxHealth(float value, Character targetEntity)
    {
        buffType = BuffType.MaxHealth;
        this.value = value;
        this.targetEntity = targetEntity;
        targetEntity.GetComponent<HasHealth>().ChangeBonusMaxHealth(value);
    }
    public override void BuffExpired()
    {
        targetEntity.GetComponent<HasHealth>().ChangeBonusMaxHealth(-value);
    }
}
public class BMaxMana : Buff
{
    public BMaxMana(float value, Character targetEntity)
    {
        buffType = BuffType.MaxMana;
        this.value = value;
        this.targetEntity = targetEntity;
        targetEntity.GetComponent<HasMana>().ChangeBonusMaxMana(value);
    }
    public override void BuffExpired()
    {
        targetEntity.GetComponent<HasMana>().ChangeBonusMaxMana(-value);
    }
}
public class BManaRegen : Buff
{
    public BManaRegen(float value, Character targetEntity)
    {
        buffType = BuffType.ManaRegen;
        this.value = value;
        this.targetEntity = targetEntity;
        targetEntity.GetComponent<HasMana>().ChangeBaseManaRegen(value);
    }
    public override void BuffExpired()
    {
        targetEntity.GetComponent<HasMana>().ChangeBaseManaRegen(-value);
    }
}
public class BBleed : Buff
{
    public BBleed(float value, Character targetEntity)
    {
        buffType = BuffType.Bleed;
        this.value = value;
        this.targetEntity = targetEntity;
        targetEntity.GetComponent<HasHealth>().ChangeBonusHealthRegen(-value);
    }
    public override void BuffExpired()
    {
        targetEntity.GetComponent<HasHealth>().ChangeBonusHealthRegen(value);
    }
}
public class BRegen : Buff
{
    public BRegen(float value, Character targetEntity)
    {
        buffType = BuffType.Regen;
        this.value = value;
        this.targetEntity = targetEntity;
        targetEntity.GetComponent<HasHealth>().ChangeBonusHealthRegen(value);
    }
    public override void BuffExpired()
    {
        targetEntity.GetComponent<HasHealth>().ChangeBonusHealthRegen(-value);
    }
}
public class BInventorySlots : Buff
{
    public InventoryManager targetInventory;
    public BInventorySlots(float value, InventoryManager targetInventory)
    {
        buffType = BuffType.InventorySlots;
        this.value = value;
        this.targetInventory = targetInventory;
        targetInventory.ExtendInventory((int)value);
    }
    public bool TestBuffExpired()
    {
        return targetInventory.TestReduceInventory((int)value);
    }
    public override void BuffExpired()
    {
        targetInventory.ReduceInventory((int)value);
    }
}
public class BPower : Buff
{
    public BPower(float value, Character targetEntity)
    {
        buffType = BuffType.Power;
        this.value = value;
        this.targetEntity = targetEntity;
        targetEntity.GetComponent<CanAttack>().ChangePower(value);
    }
    public override void BuffExpired()
    {
        targetEntity.GetComponent<CanAttack>().ChangePower(-value);
    }
}
public class BCriticalChance : Buff
{
    public BCriticalChance(float value, Character targetEntity)
    {
        buffType = BuffType.CriticalChance;
        this.value = value;
        this.targetEntity = targetEntity;
        targetEntity.GetComponent<CanAttack>().ChangeCriticalChance(value);
    }
    public override void BuffExpired()
    {
        targetEntity.GetComponent<CanAttack>().ChangeCriticalChance(-value);
    }
}
public class BCriticalDamage : Buff
{
    public BCriticalDamage(float value, Character targetEntity)
    {
        buffType = BuffType.CriticalDamage;
        this.value = value;
        this.targetEntity = targetEntity;
        targetEntity.GetComponent<CanAttack>().ChangeCriticalDamage(value);
    }
    public override void BuffExpired()
    {
        targetEntity.GetComponent<CanAttack>().ChangeCriticalDamage(-value);
    }
}
public class BAttackSpeed : Buff
{
    public BAttackSpeed(float value, Character targetEntity)
    {
        buffType = BuffType.AttackSpeed;
        this.value = value;
        this.targetEntity = targetEntity;
        targetEntity.GetComponent<CanAttack>().ChangeAttackSpeed(value);
    }
    public override void BuffExpired()
    {
        targetEntity.GetComponent<CanAttack>().ChangeAttackSpeed(-value);
    }
}
public class BAttackRange : Buff
{
    public BAttackRange(float value, Character targetEntity)
    {
        buffType = BuffType.AttackRange;
        this.value = value;
        this.targetEntity = targetEntity;
        targetEntity.GetComponent<CanAttack>().ChangeAttackRange(value);
    }
    public override void BuffExpired()
    {
        targetEntity.GetComponent<CanAttack>().ChangeAttackRange(-value);
    }
}
public class BArmor : Buff
{
    public BArmor(float value, Character targetEntity)
    {
        buffType = BuffType.Armor;
        this.value = value;
        this.targetEntity = targetEntity;
        targetEntity.GetComponent<HasHealth>().ChangeArmor(value);
    }
    public override void BuffExpired()
    {
        targetEntity.GetComponent<HasHealth>().ChangeArmor(-value);
    }
}
public class BStun : Buff
{
    public BStun(Character targetEntity)
    {
        buffType = BuffType.Stun;
        this.targetEntity = targetEntity;
        targetEntity.CmdStunCharacter();
    }
    public override void BuffExpired()
    {
        targetEntity.CmdUnstunCharacter();
    }
}
public class BSpeed : Buff
{
    public BSpeed(float value, Character targetEntity)
    {
        buffType = BuffType.MovementSpeed;
        this.value = value;
        this.targetEntity = targetEntity;
        targetEntity.GetComponent<CanMove>().ChangeBonusMovementSpeed(value);
    }
    public override void BuffExpired()
    {
        targetEntity.GetComponent<CanMove>().ChangeBonusMovementSpeed(-value);
    }
}