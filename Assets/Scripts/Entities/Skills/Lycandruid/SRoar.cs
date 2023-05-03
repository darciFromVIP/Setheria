using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Skills/Lycandruid/Roar")]
public class SRoar : Skill
{
    public float range;
    public float baseDuration;
    public float movementBaseReduction;
    private float movementFinalReduction;
    public PlayerStat movementReductionScalingStat;
    public float movementReductionScalingValue;
    public float attackSpeedBaseReduction;
    private float attackSpeedFinalReduction;
    public PlayerStat attackSpeedReductionScalingStat;
    public float attackSpeedReductionScalingValue;
    public BuffScriptable movementReductionBuff;
    public BuffScriptable attackSpeedReductionBuff;
    public Projectile projectile;
    public override void Execute(Character self)
    {
        base.Execute(self);
        castingEntity = self;
        castingEntity.GetComponent<Character>().CastSkill4();
        castingEntity.GetComponent<PlayerController>().ChangeState(PlayerState.Busy);
        if (castingEntity.isOwned)
            self.GetComponentInChildren<AnimatorEventReceiver>().Skill4_Casted.AddListener(Cast);
    }
    private void Cast()
    {
        movementReductionBuff.duration = baseDuration;
        attackSpeedReductionBuff.duration = baseDuration;
        movementReductionBuff.value = movementFinalReduction;
        attackSpeedReductionBuff.value = attackSpeedFinalReduction;
        var proj = Instantiate(projectile, castingEntity.transform.position, Quaternion.identity);
        proj.InitializeProjectile(new ProjectileData()
        {
            projectileTravel = ProjectileTravelType.Instant,
            projectileImpact = ProjectileImpactType.AoE,
            impactEffect = ProjectileImpactEffect.Buff,
            buff = movementReductionBuff,
            affectsEntities = true,
            targetsMask = LayerMask.GetMask("Enemy"),
            aoeRadius = range
        });
        var proj2 = Instantiate(projectile, castingEntity.transform.position, Quaternion.identity);
        proj2.InitializeProjectile(new ProjectileData()
        {
            projectileTravel = ProjectileTravelType.Instant,
            projectileImpact = ProjectileImpactType.AoE,
            impactEffect = ProjectileImpactEffect.Buff,
            buff = attackSpeedReductionBuff,
            affectsEntities = true,
            targetsMask = LayerMask.GetMask("Enemy"),
            aoeRadius = range
        });
        PlayerController player = castingEntity.GetComponent<PlayerController>();
        player.GetComponent<HasMana>().SpendMana(manaCost);
        player.StartCooldownE();
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
