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
    private float finalDamage;
    private float finalBleedDamage;
    public float aoeRadius;
    public float range;
    public Projectile damageProjectile;
    public BuffScriptable bleedingBuff;

    private Vector3 actualPoint;
    public override void Execute(Character self)
    {
        base.Execute(self);
        self.GetComponent<PlayerController>().Ground_Left_Clicked.AddListener(StartCast);
        self.GetComponent<PlayerController>().ChangeCastingState(CastingState.EnemyOnly);
    }
    public override void ExecuteOnStart(Character self)
    {

    }
    public override void StopExecute()
    {
        castingEntity.GetComponent<PlayerController>().Ground_Left_Clicked.RemoveListener(StartCast);
        castingEntity.GetComponentInChildren<AnimatorEventReceiver>().Skill2_Casted.RemoveListener(Cast);
    }
    private void StartCast(Vector3 point)
    {
        FindObjectOfType<AudioManager>().PlayOneShot(sound, castingEntity.transform.position);
        actualPoint = Vector3.MoveTowards(castingEntity.transform.position, point, range);
        castingEntity.GetComponent<PlayerController>().ChangeState(PlayerState.Busy);
        castingEntity.GetComponent<PlayerController>().Ground_Left_Clicked.RemoveListener(StartCast);
        castingEntity.GetComponent<Character>().CastSkill2();
        //castingEntity.GetComponent<CharacterVFXReference>().skill2.SetActive(true);
        castingEntity.GetComponentInChildren<AnimatorEventReceiver>().Skill2_Casted.AddListener(Cast);
        castingEntity.GetComponent<Character>().RotateToPoint(point);
    }
    private void Cast()
    {
        bleedingBuff.value = finalBleedDamage;
        bleedingBuff.duration = baseDuration;
        var proj = Instantiate(damageProjectile, actualPoint, Quaternion.identity);
        proj.InitializeProjectile(new ProjectileData()
        {
            projectileTravel = ProjectileTravelType.Instant,
            projectileImpact = ProjectileImpactType.AoE,
            impactEffect = ProjectileImpactEffect.Damage,
            targetsMask = LayerMask.GetMask("Enemy"),
            aoeRadius = aoeRadius,
            effectValue = finalDamage,
            affectsEntities = true,
            owner = castingEntity
        });
        var proj2 = Instantiate(damageProjectile, actualPoint, Quaternion.identity);
        proj.InitializeProjectile(new ProjectileData()
        {
            projectileTravel = ProjectileTravelType.Instant,
            projectileImpact = ProjectileImpactType.AoE,
            impactEffect = ProjectileImpactEffect.Buff,
            targetsMask = LayerMask.GetMask("Enemy"),
            buff = bleedingBuff,
            aoeRadius = aoeRadius,
            affectsEntities = true,
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
