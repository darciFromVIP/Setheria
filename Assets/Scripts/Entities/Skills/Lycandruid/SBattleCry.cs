using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Skills/Lycandruid/Battle Cry")]
public class SBattleCry : Skill
{
    public float range;
    public float baseDuration;
    public float movementBase;
    private float movementFinal;
    public PlayerStat movementScalingStat;
    public float movementScalingValue;
    public float attackSpeedBase;
    private float attackSpeedFinal;
    public PlayerStat attackSpeedScalingStat;
    public float attackSpeedScalingValue;
    public BuffScriptable movementBuff;
    public BuffScriptable attackSpeedBuff;
    public Projectile projectile;
    public override void Execute(Character self)
    {
        base.Execute(self);
        castingEntity = self;
        castingEntity.GetComponent<Character>().CastSkill4();
        castingEntity.GetComponent<PlayerController>().ChangeState(PlayerState.Busy);
        FindObjectOfType<AudioManager>().PlayOneShot(sound, castingEntity.transform.position);
        if (castingEntity.isOwned)
            self.GetComponentInChildren<AnimatorEventReceiver>().Skill4_Casted.AddListener(Cast);
    }
    private void Cast()
    {
        movementBuff.duration = baseDuration;
        attackSpeedBuff.duration = baseDuration;
        movementBuff.value = movementFinal;
        attackSpeedBuff.value = attackSpeedFinal;
        var proj = Instantiate(projectile, castingEntity.transform.position, Quaternion.identity);
        proj.InitializeProjectile(new ProjectileData()
        {
            projectileTravel = ProjectileTravelType.Instant,
            projectileImpact = ProjectileImpactType.AoE,
            impactEffect = ProjectileImpactEffect.Buff,
            buff = movementBuff,
            affectsEntities = true,
            targetsMask = LayerMask.GetMask("Player"),
            aoeRadius = range,
            affectsOwner = true
        });
        var proj2 = Instantiate(projectile, castingEntity.transform.position, Quaternion.identity);
        proj2.InitializeProjectile(new ProjectileData()
        {
            projectileTravel = ProjectileTravelType.Instant,
            projectileImpact = ProjectileImpactType.AoE,
            impactEffect = ProjectileImpactEffect.Buff,
            buff = attackSpeedBuff,
            affectsEntities = true,
            targetsMask = LayerMask.GetMask("Player"),
            aoeRadius = range,
            affectsOwner = true
        });
        PlayerController player = castingEntity.GetComponent<PlayerController>();
        player.GetComponent<HasMana>().SpendMana(manaCost);
        player.StartCooldownE();
        player.GetComponentInChildren<AnimatorEventReceiver>().Skill4_Casted.RemoveAllListeners();
        player.ChangeState(PlayerState.None);
    }
    public override void UpdateDescription()
    {
        movementFinal = movementBase + GetScalingStatValue(movementScalingStat) * movementScalingValue;
        attackSpeedFinal = attackSpeedBase + GetScalingStatValue(attackSpeedScalingStat) * attackSpeedScalingValue;
        movementBuff.duration = baseDuration;
        description = GetTextIconByStat(PlayerStat.CooldownReduction) + (cooldown * castingEntity.GetComponent<CanAttack>().GetCooldownReductionModifier()).ToString("F1")
            + " " + GetTextIconByStat(PlayerStat.MaxMana) + manaCost + "\nWolferius howls, granting himself and nearby allies Speed (increases "
            + GetTextIconByStat(PlayerStat.MovementSpeed) + " by <color=orange>" + (movementFinal * 100).ToString("F0") + "%</color> (" + (movementBase * 100).ToString("F0") + "% + "
            + (movementScalingValue * 100).ToString("F0") + "*" + GetTextIconByStat(movementScalingStat) + ")) and Energized (increases " 
            + GetTextIconByStat(PlayerStat.AttackSpeed) + " by <color=orange>" + (attackSpeedFinal * 100).ToString("F0") + "%</color> (" + (attackSpeedBase * 100).ToString("F0")
            + "% + " + (attackSpeedScalingValue * 100).ToString("F0") + "*" + GetTextIconByStat(attackSpeedScalingStat) + "))." +
            " Lasts " + movementBuff.duration + " seconds.";
        base.UpdateDescription();
    }
}
