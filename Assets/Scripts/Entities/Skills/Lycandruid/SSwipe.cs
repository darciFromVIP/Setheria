using FMOD.Studio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Skills/Lycandruid/Swipe")]
public class SSwipe : Skill
{
    public float baseDuration;
    public float baseDamage;
    public PlayerStat damageScalingStat;
    public float damageScalingValue;
    public float bleedBaseDamage;
    public PlayerStat bleedDamageScalingStat;
    public float bleedDamageScalingValue;
    [HideInInspector] public float finalDamage;
    [HideInInspector] public float finalBleedDamage;
    public float aoeRadius;
    public float range;
    public Projectile damageProjectile;
    public BuffScriptable bleedingBuff;

    [HideInInspector] public Vector3 actualPoint;
    public override void Execute(Character self)
    {
        base.Execute(self);
        self.GetComponent<PlayerController>().Ground_Left_Clicked.AddListener(StartCasting);
        self.GetComponent<PlayerController>().ChangeCastingState(CastingState.EnemyOnly);
        if (castingEntity.isOwned)
            castingEntity.skillIndicator.ShowArea(aoeRadius, range, true, RPG_Indicator.RpgIndicator.IndicatorColor.Enemy, 0);
    }
    public override Skill GetInstance()
    {
        var instance = (SSwipe)base.GetInstance();
        instance.baseDamage = baseDamage;
        instance.damageScalingStat = damageScalingStat;
        instance.damageScalingValue = damageScalingValue;
        instance.baseDuration = baseDuration;
        instance.bleedBaseDamage = bleedBaseDamage;
        instance.bleedDamageScalingStat = bleedDamageScalingStat;
        instance.bleedDamageScalingValue = bleedDamageScalingValue;
        instance.aoeRadius = aoeRadius;
        instance.damageProjectile = damageProjectile;
        instance.bleedingBuff = bleedingBuff;
        instance.range = range;
        return instance;
    }
    public override void ExecuteOnStart(Character self)
    {

    }
    public override void StopExecute()
    {
        base.StopExecute();
        castingEntity.GetComponent<PlayerController>().Ground_Left_Clicked.RemoveListener(StartCasting);
        castingEntity.GetComponentInChildren<AnimatorEventReceiver>().Skill2_Casted.RemoveAllListeners();
        if (castingEntity.isOwned)
            castingEntity.skillIndicator.InterruptCasting();
    }
    protected override void StartCasting(Vector3 point)
    {
        base.StartCasting(point);
        actualPoint = Vector3.MoveTowards(castingEntity.transform.position, point, range);
        bleedingBuff.value = finalBleedDamage;
        bleedingBuff.duration = baseDuration;
        FindObjectOfType<AudioManager>().PlayOneShot(sound, castingEntity.transform.position);
        castingEntity.GetComponent<PlayerController>().Ground_Left_Clicked.RemoveListener(StartCasting);
        castingEntity.GetComponentInChildren<AnimatorEventReceiver>().Skill2_Casted.AddListener(Cast);
        castingEntity.GetComponent<Character>().RotateToPoint(point);
        if (castingEntity.isOwned)
            castingEntity.GetComponent<Character>().CastSkill2();
        if (castingEntity.isOwned)
            castingEntity.skillIndicator.Casting(0.43f);
    }
    protected override void Cast()
    {
        base.Cast();
        if (castingEntity.isServer)
            castingEntity.GetComponent<Lycandruid>().CastSwipe();
        PlayerController player = castingEntity.GetComponent<PlayerController>();
        if (castingEntity.isServer)
            player.GetComponent<HasMana>().RpcSpendMana(manaCost);
        player.StartCooldown2();
        player.GetComponentInChildren<AnimatorEventReceiver>().Skill2_Casted.RemoveListener(Cast);
        player.ChangeState(PlayerState.None);
        player.ChangeCastingState(CastingState.None);
    }

    public override void UpdateDescription()
    {
        finalDamage = baseDamage + GetScalingStatValue(damageScalingStat) * damageScalingValue;
        finalBleedDamage = bleedBaseDamage + GetScalingStatValue(bleedDamageScalingStat) * bleedDamageScalingValue;
        bleedingBuff.duration = baseDuration;
        description = GetTextIconByStat(PlayerStat.CooldownReduction) + (cooldown * castingEntity.GetComponent<CanAttack>().GetCooldownReductionModifier()).ToString("F1")
            + " " + GetTextIconByStat(PlayerStat.MaxMana) + manaCost + "\nSwipes forward with both claws, dealing <color=orange>" + finalDamage
            + "</color> damage " + "(" + baseDamage + " + " + (int)(damageScalingValue * 100) + "% " + GetTextIconByStat(damageScalingStat) + ")"
            + " to all enemies hit as well as applying Bleed reducing " + GetTextIconByStat(PlayerStat.HealthRegen) + " by <color=orange>"
            + finalBleedDamage + "</color> (" + bleedBaseDamage + " + " + (int)(bleedDamageScalingValue * 100) + "% " + GetTextIconByStat(bleedDamageScalingStat) + ")." +
            " Lasts " + bleedingBuff.duration + " seconds.";
        base.UpdateDescription();
    }
}
