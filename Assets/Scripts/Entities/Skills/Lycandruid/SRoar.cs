using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Skills/Lycandruid/Roar")]
public class SRoar : Skill
{
    public float range;
    public float baseDuration;
    public float movementBaseReduction;
    [HideInInspector] public float movementFinalReduction;
    public PlayerStat movementReductionScalingStat;
    public float movementReductionScalingValue;
    public float attackSpeedBaseReduction;
    [HideInInspector] public float attackSpeedFinalReduction;
    public PlayerStat attackSpeedReductionScalingStat;
    public float attackSpeedReductionScalingValue;
    public BuffScriptable movementReductionBuff;
    public BuffScriptable attackSpeedReductionBuff;
    public Projectile projectile;
    public override void Execute(Character self)
    {
        base.Execute(self);
        castingEntity = self;
        if (castingEntity.isServer)
            castingEntity.GetComponent<Character>().CastSkill4();
        castingEntity.GetComponent<PlayerController>().ChangeState(PlayerState.Busy);
        FindObjectOfType<AudioManager>().PlayOneShot(sound, castingEntity.transform.position);
        self.GetComponentInChildren<AnimatorEventReceiver>().Skill4_Casted.AddListener(Cast);
        movementReductionBuff.duration = baseDuration;
        attackSpeedReductionBuff.duration = baseDuration;
        movementReductionBuff.value = movementFinalReduction;
        attackSpeedReductionBuff.value = attackSpeedFinalReduction;
        castingEntity.skillIndicator.ShowRadius(range, false, RPG_Indicator.RpgIndicator.IndicatorColor.Enemy, 0);
        castingEntity.skillIndicator.Casting(1.5f);
    }
    protected override void Cast()
    {
        base.Cast();
        if (castingEntity.isServer)
            castingEntity.GetComponent<Lycandruid>().CastRoar();
        PlayerController player = castingEntity.GetComponent<PlayerController>();
        if (castingEntity.isServer)
            player.GetComponent<HasMana>().RpcSpendMana(manaCost);
        player.StartCooldown4();
        player.GetComponentInChildren<AnimatorEventReceiver>().Skill4_Casted.RemoveAllListeners();
        player.ChangeState(PlayerState.None);
    }
    public override void UpdateDescription()
    {
        movementFinalReduction = movementBaseReduction + GetScalingStatValue(movementReductionScalingStat) * movementReductionScalingValue;
        attackSpeedFinalReduction = (attackSpeedBaseReduction + GetScalingStatValue(attackSpeedReductionScalingStat) * attackSpeedReductionScalingValue) * -1;
        movementReductionBuff.duration = baseDuration;
        description = GetTextIconByStat(PlayerStat.CooldownReduction) + (cooldown * castingEntity.GetComponent<CanAttack>().GetCooldownReductionModifier()).ToString("F1")
            + " " + GetTextIconByStat(PlayerStat.MaxMana) + manaCost + "\nUnleashes a deep roar, inflicting Slow (reduces "
            + GetTextIconByStat(PlayerStat.MovementSpeed) + " by <color=orange>" + (movementFinalReduction * 100).ToString("F0") + "%</color> (" + (movementBaseReduction * 100).ToString("F0") + "% + "
            + (movementReductionScalingValue * 100).ToString("F0") + "*" + GetTextIconByStat(movementReductionScalingStat) + ")) and Shaken (reduces " 
            + GetTextIconByStat(PlayerStat.AttackSpeed) + " by <color=orange>" + (attackSpeedFinalReduction * -100).ToString("F0") + "%</color> (" + (attackSpeedBaseReduction * 100).ToString("F0")
            + "% + " + (attackSpeedReductionScalingValue * 100).ToString("F0") + "*" + GetTextIconByStat(attackSpeedReductionScalingStat) + ")) to all enemies around Wolferius." +
            " Lasts " + movementReductionBuff.duration + " seconds.";
        base.UpdateDescription();
    }
}
