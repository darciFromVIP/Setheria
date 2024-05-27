using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Skills/Lycandruid/Wild Rage")]
public class SWildRage : Skill
{
    public float baseDuration;
    [HideInInspector] public float durationFinal;
    public PlayerStat durationScalingStat;
    public float durationScalingValue;
    public float hpRegenBase;
    [HideInInspector] public float hpRegenFinal;
    public PlayerStat hpRegenScalingStat;
    public float hpRegenScalingValue;
    public BuffScriptable invulnerabilityBuff;
    public BuffScriptable healthRegenBuff;
    public Projectile projectile;
    public override void Execute(Character self)
    {
        base.Execute(self);
        castingEntity = self;
        invulnerabilityBuff.duration = durationFinal;
        healthRegenBuff.duration = durationFinal;
        healthRegenBuff.value = hpRegenFinal;
        if (castingEntity.isOwned)
            castingEntity.GetComponent<Character>().CastSkill5();
        FindObjectOfType<AudioManager>().PlayOneShot(sound, castingEntity.transform.position);
        self.GetComponentInChildren<AnimatorEventReceiver>().Skill5_Casted.AddListener(Cast);
        if (castingEntity.isOwned)
        {
            castingEntity.skillIndicator.ShowRadius(1, false, RPG_Indicator.RpgIndicator.IndicatorColor.Ally, 0);
            castingEntity.skillIndicator.Casting(1.33f);
        }
        StartCasting();
    }
    public override void StopExecute()
    {
        base.StopExecute();
        castingEntity.GetComponentInChildren<AnimatorEventReceiver>().Skill5_Casted.RemoveAllListeners();
        if (castingEntity.isOwned)
            castingEntity.skillIndicator.InterruptCasting();
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
        hpRegenFinal = hpRegenBase + GetScalingStatValue(hpRegenScalingStat) * hpRegenScalingValue;
        durationFinal = baseDuration + GetScalingStatValue(durationScalingStat) * durationScalingValue;
        invulnerabilityBuff.duration = durationFinal;
        description = GetTextIconByStat(PlayerStat.CooldownReduction) + (cooldown * castingEntity.GetComponent<CanAttack>().GetCooldownReductionModifier()).ToString("F1")
            + " " + GetTextIconByStat(PlayerStat.MaxMana) + manaCost + "\nWolferius enters enraged state, granting himself Invulnerability and Invigoration (increases "
            + GetTextIconByStat(PlayerStat.HealthRegen) + " by <color=orange>" + hpRegenFinal.ToString("F1") + "</color> (" + hpRegenBase + " + " + (hpRegenScalingValue * 100).ToString("F0") + "% " + GetTextIconByStat(hpRegenScalingStat) + ")." +
            " Lasts <color=orange>" + invulnerabilityBuff.duration + "</color> (" + baseDuration + " + " + (durationScalingValue * 100).ToString("F0") + "% " + GetTextIconByStat(durationScalingStat) + ")." + " seconds.";
        base.UpdateDescription();
    }
}
