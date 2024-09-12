using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum PlayerStat
{
    Health, MaxHealth, HealthRegen, Mana, MaxMana, ManaRegen, Hunger, MaxHunger, Resources, Knowledge, 
    ActiveItemSlot, AttributePoint, Power, AttackSpeed, CriticalChance, CriticalDamage, Armor, CooldownReduction, MovementSpeed, Level, CorruptedHealthMana, Hydration
}
[CreateAssetMenu(menuName = "Actions/Change Stat")]
public class AChangeStat : ActionTemplate
{
    public PlayerStat stat;
    public float amount;
    public GameObject vfx;
    public override void ActionFinished()
    {
        Action_Finished.Invoke();
    }

    public override void Execute()
    {
        if (TestExecute())
        {
            var player = FindObjectOfType<GameManager>().localPlayerCharacter;
            player.ChangeStat(stat, amount);
            if (vfx)
                player.CmdSpawnVfx(vfx.name);
            ActionFinished();
        }
        else
            Debug.Log("Insufficient Action Execution");
    }

    public override bool TestExecute()
    {
        return FindObjectOfType<GameManager>().localPlayerCharacter.TestChangeStat(stat, amount);
    }
}
