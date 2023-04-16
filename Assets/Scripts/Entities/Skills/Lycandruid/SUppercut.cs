using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Skills/Lycandruid/Uppercut")]
public class SUppercut : Skill
{
    public float range;
    public float baseDuration;
    public float armorBaseReduction;
    private float armorFinalReduction;
    public PlayerStat armorReductionScalingStat;
    public float armorReductionScalingValue;
    public float damageBaseReduction;
    private float damageFinalReduction;
    public PlayerStat damageReductionScalingStat;
    public float damageReductionScalingValue;
    public BuffScriptable armorReductionBuff;
    public BuffScriptable damageReductionBuff;
    private EnemyCharacter enemy;
    public Projectile projectile;
    public override void Execute(Character self)
    {
        base.Execute(self);
        self.GetComponent<PlayerController>().Enemy_Left_Clicked.AddListener(MoveWithinRange);
        self.GetComponent<PlayerController>().ChangeCastingState(CastingState.EnemyOnly);
    }

    public override void ExecuteOnStart(Character self)
    {
    }

    public override void StopExecute()
    {
        castingEntity.GetComponent<PlayerController>().Enemy_Left_Clicked.RemoveListener(MoveWithinRange);
        castingEntity.GetComponent<CanMove>().Moved_Within_Range.RemoveListener(StartCasting);
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
    private void StartCasting()
    {
        castingEntity.GetComponent<Character>().CastSkill3();
        castingEntity.GetComponent<PlayerController>().ChangeState(PlayerState.Busy);
        if (castingEntity.isOwned)
        {
            castingEntity.GetComponent<CanMove>().Moved_Within_Range.RemoveListener(StartCasting);
            castingEntity.GetComponentInChildren<AnimatorEventReceiver>().Skill3_Casted.AddListener(Cast);
            castingEntity.GetComponent<Character>().RotateToPoint(enemy.transform.position);
        }
    }
    private void Cast()
    {
        armorReductionBuff.duration = baseDuration;
        damageReductionBuff.duration = baseDuration;
        armorReductionBuff.value = armorFinalReduction;
        damageReductionBuff.value = damageFinalReduction;
        var proj = Instantiate(projectile, castingEntity.transform.position, Quaternion.identity);
        proj.InitializeProjectile(new ProjectileData()
        {
            projectileTravel = ProjectileTravelType.Instant,
            projectileImpact = ProjectileImpactType.Single,
            impactEffect = ProjectileImpactEffect.Buff,
            buff = armorReductionBuff,
            targetedEntity = enemy.GetComponent<HasHealth>()
        });
        var proj2 = Instantiate(projectile, castingEntity.transform.position, Quaternion.identity);
        proj2.InitializeProjectile(new ProjectileData()
        {
            projectileTravel = ProjectileTravelType.Instant,
            projectileImpact = ProjectileImpactType.Single,
            impactEffect = ProjectileImpactEffect.Buff,
            buff = damageReductionBuff,
            targetedEntity = enemy.GetComponent<HasHealth>()
        });
        PlayerController player = castingEntity.GetComponent<PlayerController>();
        player.GetComponent<HasMana>().SpendMana(manaCost);
        player.StartCooldownW();
        player.GetComponentInChildren<AnimatorEventReceiver>().Skill3_Casted.RemoveAllListeners();
        player.ChangeState(PlayerState.None);
        player.ChangeCastingState(CastingState.None);
    }
    public override void UpdateDescription()
    {
        armorFinalReduction = (armorBaseReduction + GetScalingStatValue(armorReductionScalingStat) * armorReductionScalingValue) * -1;
        damageFinalReduction = (damageBaseReduction + GetScalingStatValue(damageReductionScalingStat) * damageReductionScalingValue) * -1;
        armorReductionBuff.duration = baseDuration;
        description = GetTextIconByStat(PlayerStat.CooldownReduction) + (cooldown * castingEntity.GetComponent<CanAttack>().GetCooldownReductionModifier()).ToString("F1")
            + " " + GetTextIconByStat(PlayerStat.MaxMana) + manaCost + "\nPerforms an uppercut, which applies Cripple (reduces "
            + GetTextIconByStat(PlayerStat.Power) + " by <color=orange>" + (damageFinalReduction * -1).ToString("F2") + "</color> (" + damageBaseReduction + " + "
            + (int)(damageReductionScalingValue * 100) + "% " + GetTextIconByStat(damageReductionScalingStat) +
            ")) and Sunder (reduces " + GetTextIconByStat(PlayerStat.Armor) + " by <color=orange>" + (armorFinalReduction * -1).ToString("F2") + "</color> ("
            + armorBaseReduction + " + " + (int)(armorReductionScalingValue * 100) + "% " + GetTextIconByStat(armorReductionScalingStat) + "))." +
            " Lasts " + armorReductionBuff.duration + " seconds.";
        base.UpdateDescription();
    }
}
