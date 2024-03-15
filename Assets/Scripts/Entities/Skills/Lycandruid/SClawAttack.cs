using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Skills/Lycandruid/Auto Attack")]
public class SClawAttack : Skill
{
    public override void UpdateDescription()
    {
        var attackComp = castingEntity.GetComponent<CanAttack>();
        description = GetTextIconByStat(PlayerStat.AttackSpeed) + attackComp.GetAttackCooldown().ToString("F2") + "\nYour basic attack deals <color=orange>"
            + attackComp.GetFinalPower() * attackComp.GetPowerScaling() + "</color> (" + attackComp.GetPowerScaling() * 100 + "%" + GetTextIconByStat(PlayerStat.Power) + ") damage.";
        base.UpdateDescription();
    }
}
