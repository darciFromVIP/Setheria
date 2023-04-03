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
    private float finalDamage;
    private float finalHeal;
    public float aoeRadius;
    public float range;
    public Projectile damageProjectile;
    public Projectile healingProjectile;

    private Vector3 actualPoint;
    public override void Execute(Entity self)
    {
        base.Execute(self);
        self.GetComponent<PlayerController>().Ground_Left_Clicked.AddListener(StartCast);
        self.GetComponent<PlayerController>().ChangeCastingState(CastingState.BothExceptSelf);
    }
    public override void ExecuteOnStart(Entity self)
    {
        
    }
    public override void StopExecute()
    {
        castingEntity.GetComponent<PlayerController>().Ground_Left_Clicked.RemoveListener(StartCast);
        castingEntity.GetComponentInChildren<AnimatorEventReceiver>().Skill2_Casted.RemoveListener(Cast);
    }
    private void StartCast(Vector3 point)
    {
        actualPoint = Vector3.MoveTowards(castingEntity.transform.position, point, range);
        castingEntity.GetComponent<PlayerController>().ChangeState(PlayerState.Busy);
        castingEntity.GetComponent<PlayerController>().Ground_Left_Clicked.RemoveListener(StartCast);
        castingEntity.GetComponent<Character>().CastSkill2();
        castingEntity.GetComponent<CharacterVFXReference>().skill2.SetActive(true);
        castingEntity.GetComponentInChildren<AnimatorEventReceiver>().Skill2_Casted.AddListener(Cast);
        castingEntity.GetComponent<Character>().RotateToPoint(point);
    }
    private void Cast()
    {
        var proj = Instantiate(damageProjectile, castingEntity.GetComponent<CanAttack>().projectileLaunchPoint.position, Quaternion.identity);
        proj.InitializeProjectile(new ProjectileData()
        {
            projectileTravel = ProjectileTravelType.TerrainTargeted,
            projectileImpact = ProjectileImpactType.AoE,
            impactEffect = ProjectileImpactEffect.Damage,
            targetsMask = LayerMask.GetMask("Enemy"),
            aoeRadius = aoeRadius,
            effectValue = finalDamage,
            speed = 5,
            targetPoint = actualPoint,
            affectsEntities = true,
            owner = castingEntity
        });
        var proj2 = Instantiate(healingProjectile, castingEntity.GetComponent<CanAttack>().projectileLaunchPoint.position, Quaternion.identity);
        proj2.InitializeProjectile(new ProjectileData()
        {
            projectileTravel = ProjectileTravelType.TerrainTargeted,
            projectileImpact = ProjectileImpactType.AoE,
            impactEffect = ProjectileImpactEffect.Healing,
            targetsMask = LayerMask.GetMask("Player"),
            aoeRadius = aoeRadius,
            effectValue = finalHeal,
            speed = 5,
            targetPoint = actualPoint,
            affectsOwner = false,
            affectsEntities = true,
            affectsStructures = false,
            owner = castingEntity
        });
        PlayerController player = castingEntity.GetComponent<PlayerController>();
        player.GetComponent<HasMana>().SpendMana(manaCost);
        player.StartCooldownQ();
        player.GetComponentInChildren<AnimatorEventReceiver>().Skill2_Casted.RemoveListener(Cast);
        player.ChangeState(PlayerState.None);
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
