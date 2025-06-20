using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Skills/Forest Protector/Defilement")]
public class SDefilement : Skill
{
    public float range;
    public float baseDuration;
    public float baseDamage;
    public PlayerStat damageScalingStat;
    public float damageScalingValue;
    public BuffScriptable buff;
    [HideInInspector] public EnemyCharacter enemy;
    public Projectile projectile;
    public override void Execute(Character self)
    {
        base.Execute(self);
        self.GetComponent<PlayerController>().Enemy_Left_Clicked.AddListener(MoveWithinRange);
        self.GetComponent<PlayerController>().ChangeCastingState(CastingState.EnemyOnly);
        if (castingEntity.isOwned)
            castingEntity.skillIndicator.ShowRange(range, RPG_Indicator.RpgIndicator.IndicatorColor.Enemy, 0);
    }
    public override Skill GetInstance()
    {
        var instance = (SDefilement)base.GetInstance();
        instance.baseDamage = baseDamage;
        instance.damageScalingStat = damageScalingStat;
        instance.damageScalingValue = damageScalingValue;
        instance.range = range;
        instance.baseDuration = baseDuration;
        instance.buff = buff;
        instance.projectile = projectile;
        return instance;
    }
    public override void ExecuteOnStart(Character self)
    {

    }

    public override void StopExecute()
    {
        base.StopExecute();
    }
    private void MoveWithinRange(EnemyCharacter enemy)
    {
        this.enemy = enemy;
        if (Vector3.Distance(castingEntity.transform.position, enemy.transform.position) > range)
        {
            castingEntity.GetComponent<CanMove>().Moved_Within_Range.AddListener(StartCasting);
            castingEntity.GetComponent<CanMove>().MoveWithinRange(enemy.transform, range);
        }
        else
        {
            castingEntity.GetComponent<PlayerController>().Enemy_Left_Clicked.RemoveAllListeners();
            StartCasting();
        }
    }
    protected override void StartCasting()
    {
        base.StartCasting();
        if (castingEntity.isOwned)
            castingEntity.GetComponent<Character>().CastSkill3();
        castingEntity.GetComponentInChildren<AnimatorEventReceiver>().Skill3_Casted.AddListener(Cast);
        castingEntity.GetComponent<Character>().RotateToPoint(enemy.transform.position);
        if (castingEntity.isOwned)
            castingEntity.skillIndicator.Casting(0.86f);
    }
    protected override void Cast()
    {
        base.Cast();
        if (castingEntity.isServer)
            castingEntity.GetComponent<ForestProtector>().CastDefilement();
        PlayerController player = castingEntity.GetComponent<PlayerController>();
        if (castingEntity.isServer)
            player.GetComponent<HasMana>().RpcSpendMana(manaCost);
        player.StartCooldown3();
        player.GetComponentInChildren<AnimatorEventReceiver>().Skill3_Casted.RemoveAllListeners();
    }

    public override void UpdateDescription()
    {
        buff.duration = baseDuration;
        buff.value = baseDamage + damageScalingValue * GetScalingStatValue(damageScalingStat);
        description = GetTextIconByStat(PlayerStat.CooldownReduction) + (cooldown * castingEntity.GetComponent<CanAttack>().GetCooldownReductionModifier()).ToString("F1") +
            " " + GetTextIconByStat(PlayerStat.MaxMana) + manaCost +
            "\nDefiles an enemy target with vile magic, reducing their " + GetTextIconByStat(PlayerStat.HealthRegen) + " by <color=orange>" + buff.value
            + "</color> " + "(" + baseDamage + " + " + (damageScalingValue * 100).ToString("F0") + "% " + GetTextIconByStat(damageScalingStat) + ")"
            + " for " + buff.duration.ToString("F1") + " seconds.";
        base.UpdateDescription();
    }
}
