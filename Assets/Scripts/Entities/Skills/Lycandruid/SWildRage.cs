using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Skills/Lycandruid/Wild Rage")]
public class SWildRage : Skill
{
    public float baseDuration;
    public float maxHPBase;
    [HideInInspector] public float maxHPFinal;
    public PlayerStat maxHPScalingStat;
    public float maxHPScalingValue;
    public float cooldownReductionBase;
    public BuffScriptable maxHPBuff;
    public BuffScriptable cooldownReductionBuff;
    public Projectile projectile;
    public override void Execute(Character self)
    {
        base.Execute(self);
        castingEntity = self;
        maxHPBuff.duration = baseDuration;
        cooldownReductionBuff.duration = baseDuration;
        maxHPBuff.value = maxHPFinal;
        cooldownReductionBuff.value = cooldownReductionBase;
        if (castingEntity.isOwned)
            castingEntity.GetComponent<Character>().CastSkill5();
        castingEntity.GetComponent<PlayerController>().ChangeState(PlayerState.Busy);
        FindObjectOfType<AudioManager>().PlayOneShot(sound, castingEntity.transform.position);
        self.GetComponentInChildren<AnimatorEventReceiver>().Skill5_Casted.AddListener(Cast);
        if (castingEntity.isOwned)
        {
            castingEntity.skillIndicator.ShowRadius(1, false, RPG_Indicator.RpgIndicator.IndicatorColor.Ally, 0);
            castingEntity.skillIndicator.Casting(1.33f);
        }
    }
    protected override void Cast()
    {
        base.Cast();
        if (castingEntity.isServer)
            castingEntity.GetComponent<Lycandruid>().CastWildRage();
        PlayerController player = castingEntity.GetComponent<PlayerController>();
        if (castingEntity.isServer)
            player.GetComponent<HasMana>().RpcSpendMana(manaCost);
        player.StartCooldown5();
        player.GetComponentInChildren<AnimatorEventReceiver>().Skill5_Casted.RemoveAllListeners();
        player.ChangeState(PlayerState.None);
    }
    public override void UpdateDescription()
    {
        maxHPFinal = maxHPBase + GetScalingStatValue(maxHPScalingStat) * maxHPScalingValue;
        maxHPBuff.duration = baseDuration;
        description = GetTextIconByStat(PlayerStat.CooldownReduction) + (cooldown * castingEntity.GetComponent<CanAttack>().GetCooldownReductionModifier()).ToString("F1")
            + " " + GetTextIconByStat(PlayerStat.MaxMana) + manaCost + "\nWolferius enters enraged state, granting himself Fortitude (increases "
            + GetTextIconByStat(PlayerStat.MaxHealth) + " by <color=orange>" + maxHPFinal + "</color> (" + maxHPBase + " + "
            + (maxHPScalingValue * 100).ToString("F0") + "% " + GetTextIconByStat(maxHPScalingStat) + ")) and Haste (increases "
            + GetTextIconByStat(PlayerStat.CooldownReduction) + " by <color=orange>" + cooldownReductionBase + "%</color>)." +
            " Lasts " + maxHPBuff.duration + " seconds.";
        base.UpdateDescription();
    }
}
