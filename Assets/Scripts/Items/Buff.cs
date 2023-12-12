using FMOD;
using FMOD.Studio;
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
    public EventInstance sound;

    public UnityEvent Buff_Expired = new();
    public virtual void SetSound(EventInstance soundInstance)
    {
        sound = soundInstance;
        sound.start();
    }
    public virtual void BuffExpired()
    {
        Buff_Expired.Invoke();
        if (targetEntity)
        {
            targetEntity.BuffExpired(effect);
            targetEntity.buffs.Remove(this);
        }
    }
    public virtual IEnumerator TimedBuff(float duration)
    {
        durationTimer = duration;
        while (durationTimer > 0)
        {
            sound.set3DAttributes(new ATTRIBUTES_3D
            {
                position = new VECTOR { x = targetEntity.transform.position.x, y = targetEntity.transform.position.y, z = targetEntity.transform.position.z },
                forward = new VECTOR { x = targetEntity.transform.forward.x, y = targetEntity.transform.forward.y, z = targetEntity.transform.forward.z },
                up = new VECTOR { x = targetEntity.transform.up.x, y = targetEntity.transform.up.y, z = targetEntity.transform.up.z },
                velocity = new VECTOR { x = 0, y = 0, z = 0 }
            });
            durationTimer -= Time.deltaTime;
            yield return null;
        }
        BuffExpired();
    }
}
public class BMaxHealth : Buff
{
    public BMaxHealth(float value, Character targetEntity)
    {
        buffType = BuffType.MaxHealth;
        this.value = value;
        this.targetEntity = targetEntity;
        targetEntity.GetComponent<HasHealth>().ChangeGearMaxHealth(value);
    }
    public override void BuffExpired()
    {
        base.BuffExpired();
        targetEntity.GetComponent<HasHealth>().ChangeGearMaxHealth(-value);
    }
}
public class BMaxMana : Buff
{
    public BMaxMana(float value, Character targetEntity)
    {
        buffType = BuffType.MaxMana;
        this.value = value;
        this.targetEntity = targetEntity;
        targetEntity.GetComponent<HasMana>().ChangeGearMaxMana(value);
    }
    public override void BuffExpired()
    {
        base.BuffExpired();
        targetEntity.GetComponent<HasMana>().ChangeGearMaxMana(-value);
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
        base.BuffExpired();
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
        targetEntity.GetComponent<HasHealth>().ChangeGearHealthRegen(-value);
    }
    public override void BuffExpired()
    {
        base.BuffExpired();
        targetEntity.GetComponent<HasHealth>().ChangeGearHealthRegen(value);
    }
}
public class BRegen : Buff
{
    public BRegen(float value, Character targetEntity)
    {
        buffType = BuffType.Regen;
        this.value = value;
        this.targetEntity = targetEntity;
        targetEntity.GetComponent<HasHealth>().ChangeGearHealthRegen(value);
    }
    public override void BuffExpired()
    {
        base.BuffExpired();
        if (sound.isValid())
            sound.setParameterByNameWithLabel("Rejuvenation", "End");
        targetEntity.GetComponent<HasHealth>().ChangeGearHealthRegen(-value);
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
        base.BuffExpired();
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
        targetEntity.GetComponent<CanAttack>().ChangeGearPower(value);
    }
    public override void BuffExpired()
    {
        base.BuffExpired();
        targetEntity.GetComponent<CanAttack>().ChangeGearPower(-value);
    }
}
public class BCriticalChance : Buff
{
    public BCriticalChance(float value, Character targetEntity)
    {
        buffType = BuffType.CriticalChance;
        this.value = value;
        this.targetEntity = targetEntity;
        targetEntity.GetComponent<CanAttack>().ChangeGearCriticalChance(value);
    }
    public override void BuffExpired()
    {
        base.BuffExpired();
        targetEntity.GetComponent<CanAttack>().ChangeGearCriticalChance(-value);
    }
}
public class BCriticalDamage : Buff
{
    public BCriticalDamage(float value, Character targetEntity)
    {
        buffType = BuffType.CriticalDamage;
        this.value = value;
        this.targetEntity = targetEntity;
        targetEntity.GetComponent<CanAttack>().ChangeGearCriticalDamage(value);
    }
    public override void BuffExpired()
    {
        base.BuffExpired();
        targetEntity.GetComponent<CanAttack>().ChangeGearCriticalDamage(-value);
    }
}
public class BAttackSpeed : Buff
{
    public BAttackSpeed(float value, Character targetEntity)
    {
        buffType = BuffType.AttackSpeed;
        this.value = value;
        this.targetEntity = targetEntity;
        targetEntity.GetComponent<CanAttack>().ChangeGearAttackSpeed(value);
    }
    public override void BuffExpired()
    {
        base.BuffExpired();
        targetEntity.GetComponent<CanAttack>().ChangeGearAttackSpeed(-value);
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
        base.BuffExpired();
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
        targetEntity.GetComponent<HasHealth>().ChangeGearArmor(value);
    }
    public override void BuffExpired()
    {
        base.BuffExpired();
        targetEntity.GetComponent<HasHealth>().ChangeGearArmor(-value);
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
        base.BuffExpired();
        targetEntity.CmdUnstunCharacter();
    }
}
public class BSpeed : Buff
{
    public BSpeed(float value, Character targetEntity)
    {
        buffType = BuffType.Speed;
        this.value = value;
        this.targetEntity = targetEntity;
        targetEntity.GetComponent<CanMove>().ChangeBonusMovementSpeed(value);
    }
    public override void BuffExpired()
    {
        base.BuffExpired();
        targetEntity.GetComponent<CanMove>().ChangeBonusMovementSpeed(-value);
    }
}
public class BSlow : Buff
{
    public BSlow(float value, Character targetEntity)
    {
        buffType = BuffType.Speed;
        this.value = value;
        this.targetEntity = targetEntity;
        targetEntity.GetComponent<CanMove>().ChangeBonusMovementSpeed(-value);
    }
    public override void BuffExpired()
    {
        base.BuffExpired();
        targetEntity.GetComponent<CanMove>().ChangeBonusMovementSpeed(value);
    }
}
public class BCooldownReduction : Buff
{
    public BCooldownReduction(float value, Character targetEntity)
    {
        buffType = BuffType.CooldownReduction;
        this.value = value;
        this.targetEntity = targetEntity;
        targetEntity.GetComponent<CanAttack>().ChangeGearCooldownReduction(value);
    }
    public override void BuffExpired()
    {
        base.BuffExpired();
        targetEntity.GetComponent<CanAttack>().ChangeGearCooldownReduction(-value);
    }
}