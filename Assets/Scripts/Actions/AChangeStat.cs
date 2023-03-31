using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum PlayerStat
{
    Health, MaxHealth, HealthRegen, Mana, MaxMana, ManaRegen, Hunger, MaxHunger, Resources, Knowledge, ActiveItemSlot, AttributePoint, Power, AttackSpeed, CriticalChance, CriticalDamage, Armor, CooldownReduction
}
[CreateAssetMenu(menuName = "Actions/Change Stat")]
public class AChangeStat : ActionTemplate
{
    public PlayerStat stat;
    public float amount;
    public override void ActionFinished()
    {
        
    }

    public override void Execute()
    {
        if (TestExecute())
        {
            FindObjectOfType<GameManager>().localPlayerCharacter.ChangeStat(stat, amount);
            Action_Finished.Invoke();
        }
        else
            Debug.Log("Insufficient Action Execution");
    }

    public override bool TestExecute()
    {
        return FindObjectOfType<GameManager>().localPlayerCharacter.TestChangeStat(stat, amount);
    }
}
