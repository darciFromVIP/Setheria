using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Forest Protector/Corrupted Dust")]
public class SCorruptedDust : Skill
{
    public float baseDamage;
    public PlayerStat damageScalingStat;
    public float damageScalingValue;
    [HideInInspector] public float finalDamage;
    public float aoeRadius;
    public float range;
    public Projectile damageProjectile;

    [HideInInspector] public Vector3 actualPoint;
    public override Skill GetInstance()
    {
        var instance = (SCorruptedDust)base.GetInstance();
        instance.baseDamage = baseDamage;
        instance.damageScalingStat = damageScalingStat;
        instance.damageScalingValue = damageScalingValue;
        instance.aoeRadius = aoeRadius;
        instance.range = range;
        instance.damageProjectile = damageProjectile;
        return instance;
    }
    public override void Execute(Character self)
    {
        base.Execute(self);
        self.GetComponent<PlayerController>().Ground_Left_Clicked.AddListener(StartCasting);
        self.GetComponent<PlayerController>().ChangeCastingState(CastingState.EnemyOnly);
        if (castingEntity.isOwned)
            self.skillIndicator.ShowArea(aoeRadius, range, true, RPG_Indicator.RpgIndicator.IndicatorColor.Enemy, 0);
    }
    public override void ExecuteOnStart(Character self)
    {
        
    }
    public override void StopExecute()
    {
        base.StopExecute();
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
            castingEntity.GetComponent<ForestProtector>().CastCorruptedDust();
        PlayerController player = castingEntity.GetComponent<PlayerController>();
        if (castingEntity.isServer)
            player.GetComponent<HasMana>().RpcSpendMana(manaCost);
        player.StartCooldown2();
        player.GetComponentInChildren<AnimatorEventReceiver>().Skill2_Casted.RemoveListener(Cast);
    }

    public override void UpdateDescription()
    {
        finalDamage = baseDamage + GetScalingStatValue(damageScalingStat) * damageScalingValue;
        description = GetTextIconByStat(PlayerStat.CooldownReduction) + (cooldown * castingEntity.GetComponent<CanAttack>().GetCooldownReductionModifier()).ToString("F1") 
            + " " + GetTextIconByStat(PlayerStat.MaxMana) + manaCost + "\nSprays corrupted magical dust to a target area, dealing <color=orange>" + finalDamage
            + "</color> damage " + "(" + baseDamage + " + " + (int)(damageScalingValue * 100) + "% " + GetTextIconByStat(damageScalingStat) + ")"
            + " to all enemies hit.";
        base.UpdateDescription();
    }
}
