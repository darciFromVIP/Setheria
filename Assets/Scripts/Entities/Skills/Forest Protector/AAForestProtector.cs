using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Skills/Forest Protector/Auto Attack")]
public class AAForestProtector : Skill
{
    public override void Execute(Character self)
    {
    }

    public override void ExecuteOnStart(Character self)
    {
    }

    public override void StopExecute()
    {
    }

    public override void UpdateDescription()
    {
        var attackComp = castingEntity.GetComponent<CanAttack>();
        description = GetTextIconByStat(PlayerStat.AttackSpeed) + attackComp.GetAttackCooldown().ToString("F2") + "\nYour basic attack deals <color=orange>" 
            + attackComp.GetPower() + "</color> (" + "100%" + GetTextIconByStat(PlayerStat.Power) + ") damage.";
        base.UpdateDescription();
    }
}
