using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Skills/Lycandruid/Battle Cry")]
public class SBattleCry : Skill
{
    public float range;
    public float baseDuration;
    public float movementBase;
    private float movementFinal;
    public PlayerStat movementScalingStat;
    public float movementScalingValue;
    public float attackSpeedBase;
    private float attackSpeedFinal;
    public PlayerStat attackSpeedScalingStat;
    public float attackSpeedScalingValue;
    public BuffScriptable movementBuff;
    public BuffScriptable attackSpeedBuff;
    public Projectile projectile;
    public override void Execute(Character self)
    {
        base.Execute(self);
        if (castingEntity.isServer)
            castingEntity.GetComponent<Character>().CastSkill4();
        castingEntity.GetComponent<PlayerController>().ChangeState(PlayerState.Busy);
        FindObjectOfType<AudioManager>().PlayOneShot(sound, castingEntity.transform.position);
        movementBuff.duration = baseDuration;
        attackSpeedBuff.duration = baseDuration;
        movementBuff.value = movementFinal;
        attackSpeedBuff.value = attackSpeedFinal;
        self.GetComponentInChildren<AnimatorEventReceiver>().Skill4_Casted.AddListener(Cast);
        castingEntity.skillIndicator.ShowRadius(range, false, RPG_Indicator.RpgIndicator.IndicatorColor.Ally, 0);
        castingEntity.skillIndicator.Casting(1.26f);
    }
    protected override void Cast()
    {
        base.Cast();
        if (castingEntity.isServer)
            castingEntity.GetComponent<Lycandruid>().CastBattleCry();
        PlayerController player = castingEntity.GetComponent<PlayerController>();
        if (castingEntity.isServer)
            player.GetComponent<HasMana>().RpcSpendMana(manaCost);
        player.StartCooldown4();
        player.GetComponentInChildren<AnimatorEventReceiver>().Skill4_Casted.RemoveAllListeners();
        player.ChangeState(PlayerState.None);
    }

    public override void UpdateDescription()
    {
        movementFinal = movementBase + GetScalingStatValue(movementScalingStat) * movementScalingValue;
        attackSpeedFinal = attackSpeedBase + GetScalingStatValue(attackSpeedScalingStat) * attackSpeedScalingValue;
        movementBuff.duration = baseDuration;
        description = GetTextIconByStat(PlayerStat.CooldownReduction) + (cooldown * castingEntity.GetComponent<CanAttack>().GetCooldownReductionModifier()).ToString("F1")
            + " " + GetTextIconByStat(PlayerStat.MaxMana) + manaCost + "\nWolferius howls, granting himself and nearby allies Speed (increases "
            + GetTextIconByStat(PlayerStat.MovementSpeed) + " by <color=orange>" + (movementFinal * 100).ToString("F0") + "%</color> (" + (movementBase * 100).ToString("F0") + "% + "
            + (movementScalingValue * 100).ToString("F0") + "*" + GetTextIconByStat(movementScalingStat) + ")) and Energized (increases " 
            + GetTextIconByStat(PlayerStat.AttackSpeed) + " by <color=orange>" + (attackSpeedFinal * 100).ToString("F0") + "%</color> (" + (attackSpeedBase * 100).ToString("F0")
            + "% + " + (attackSpeedScalingValue * 100).ToString("F0") + "*" + GetTextIconByStat(attackSpeedScalingStat) + "))." +
            " Lasts " + movementBuff.duration + " seconds.";
        base.UpdateDescription();
    }
}
