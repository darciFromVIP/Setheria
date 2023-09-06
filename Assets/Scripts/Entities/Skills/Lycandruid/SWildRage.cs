using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Skills/Lycandruid/Wild Rage")]
public class SWildRage : Skill
{
    public float baseDuration;
    public float maxHPBase;
    private float maxHPFinal;
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
        castingEntity.GetComponent<Character>().CastSkill5();
        castingEntity.GetComponent<PlayerController>().ChangeState(PlayerState.Busy);
        FindObjectOfType<AudioManager>().PlayOneShot(sound, castingEntity.transform.position);
        if (castingEntity.isOwned)
            self.GetComponentInChildren<AnimatorEventReceiver>().Skill5_Casted.AddListener(Cast);
    }
    private void Cast()
    {
        maxHPBuff.duration = baseDuration;
        cooldownReductionBuff.duration = baseDuration;
        maxHPBuff.value = maxHPFinal;
        cooldownReductionBuff.value = cooldownReductionBase;
        var proj = Instantiate(projectile, castingEntity.transform.position, Quaternion.identity);
        proj.InitializeProjectile(new ProjectileData()
        {
            projectileTravel = ProjectileTravelType.Instant,
            projectileImpact = ProjectileImpactType.Single,
            impactEffect = ProjectileImpactEffect.Buff,
            buff = maxHPBuff,
            targetedEntity = castingEntity.GetComponent<HasHealth>()
        });
        var proj2 = Instantiate(projectile, castingEntity.transform.position, Quaternion.identity);
        proj2.InitializeProjectile(new ProjectileData()
        {
            projectileTravel = ProjectileTravelType.Instant,
            projectileImpact = ProjectileImpactType.Single,
            impactEffect = ProjectileImpactEffect.Buff,
            buff = cooldownReductionBuff,
            targetedEntity = castingEntity.GetComponent<HasHealth>()
        });
        PlayerController player = castingEntity.GetComponent<PlayerController>();
        player.GetComponent<HasMana>().CmdSpendMana(manaCost);
        player.StartCooldownR();
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
