using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Forest Protector/Green Dust")]
public class SGreenDust : Skill
{
    public float baseDamage;
    public PlayerStat damageScalingStat;
    public float damageScalingValue;
    public float baseHeal;
    public PlayerStat healScalingStat;
    public float healScalingValue;
    [HideInInspector] public float finalDamage;
    [HideInInspector] public float finalHeal;
    public float aoeRadius;
    public float range;
    public Projectile damageProjectile;
    public Projectile healingProjectile;


    [HideInInspector] public Vector3 actualPoint;
    public override Skill GetInstance()
    {
        var instance = (SGreenDust)base.GetInstance();
        instance.baseDamage = baseDamage;
        instance.damageScalingStat = damageScalingStat;
        instance.damageScalingValue = damageScalingValue;
        instance.baseHeal = baseHeal;
        instance.healScalingStat = healScalingStat;
        instance.healScalingValue = healScalingValue;
        instance.aoeRadius = aoeRadius;
        instance.range = range;
        instance.damageProjectile = damageProjectile;
        instance.healingProjectile = healingProjectile;
        return instance;
    }
    public override void Execute(Character self)
    {
        base.Execute(self);
        self.GetComponent<PlayerController>().Ground_Left_Clicked.AddListener(StartCasting);
        self.GetComponent<PlayerController>().ChangeCastingState(CastingState.BothExceptSelf);
        if (castingEntity.isOwned)
            self.skillIndicator.ShowArea(aoeRadius, range, true, RPG_Indicator.RpgIndicator.IndicatorColor.Neutral, 0);
    }
    public override void ExecuteOnStart(Character self)
    {
        
    }
    public override void StopExecute()
    {
        base.StopExecute();
        castingEntity.GetComponent<PlayerController>().Ground_Left_Clicked.RemoveListener(StartCasting);
        castingEntity.GetComponentInChildren<AnimatorEventReceiver>().Skill2_Casted.RemoveListener(Cast);
        if (castingEntity.isOwned)
            castingEntity.skillIndicator.InterruptCasting();
    }
    protected override void StartCasting(Vector3 point)
    {
        base.StartCasting(point);
        FindObjectOfType<AudioManager>().PlayOneShot(sound, castingEntity.transform.position);
        actualPoint = Vector3.MoveTowards(castingEntity.transform.position, point, range);
        castingEntity.GetComponent<PlayerController>().Ground_Left_Clicked.RemoveListener(StartCasting);
        if (castingEntity.isOwned)
            castingEntity.GetComponent<Character>().CastSkill2();
        castingEntity.GetComponent<CharacterVFXReference>().skill2.SetActive(true);
        castingEntity.GetComponentInChildren<AnimatorEventReceiver>().Skill2_Casted.AddListener(Cast);
        castingEntity.GetComponent<Character>().RotateToPoint(point);
        if (castingEntity.isOwned)
            castingEntity.skillIndicator.Casting(0.8f);
    }
    protected override void Cast()
    {
        base.Cast();
        if (castingEntity.isServer)
            castingEntity.GetComponent<ForestProtector>().CastGreenDust();
        PlayerController player = castingEntity.GetComponent<PlayerController>();
        if (castingEntity.isServer)
            player.GetComponent<HasMana>().RpcSpendMana(manaCost);
        player.StartCooldown2();
        player.GetComponentInChildren<AnimatorEventReceiver>().Skill2_Casted.RemoveListener(Cast);
        player.CmdChangeState(PlayerState.None);
        player.ChangeCastingState(CastingState.None);
    }

    public override void UpdateDescription()
    {
        finalDamage = baseDamage + GetScalingStatValue(damageScalingStat) * damageScalingValue;
        finalHeal = baseHeal + GetScalingStatValue(healScalingStat) * healScalingValue;
        description = GetTextIconByStat(PlayerStat.CooldownReduction) + (cooldown * castingEntity.GetComponent<CanAttack>().GetCooldownReductionModifier()).ToString("F1") 
            + " " + GetTextIconByStat(PlayerStat.MaxMana) + manaCost + "\nSprays magical green dust to a target area, dealing <color=orange>" + finalDamage
            + "</color> damage " + "(" + baseDamage + " + " + (int)(damageScalingValue * 100) + "% " + GetTextIconByStat(damageScalingStat) + ")"
            + " to enemies as well as healing allies hit (except self) for <color=orange>" + finalHeal
            + "</color> (" + baseHeal + " + " + (int)(healScalingValue * 100) + "% " + GetTextIconByStat(healScalingStat) + ")";
        base.UpdateDescription();
    }
}
