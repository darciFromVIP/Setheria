using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Skills/Forest Protector/Rejuvenation")]
public class SRejuvenation : Skill
{
    public float range;
    public float baseDuration;
    public float baseHeal;
    public PlayerStat healScalingStat;
    public float healScalingValue;
    public BuffScriptable buff;
    private PlayerCharacter ally;
    public Projectile projectile;
    public override void Execute(Character self)
    {
        base.Execute(self);
        self.GetComponent<PlayerController>().Ally_Left_Clicked.AddListener(MoveWithinRange);
        self.GetComponent<PlayerController>().ChangeCastingState(CastingState.AllyOnly);
    }

    public override void ExecuteOnStart(Character self)
    {

    }

    public override void StopExecute()
    {
        castingEntity.GetComponent<PlayerController>().Ally_Left_Clicked.RemoveListener(MoveWithinRange);
        castingEntity.GetComponent<CanMove>().Moved_Within_Range.RemoveListener(StartCasting);
        castingEntity.GetComponentInChildren<AnimatorEventReceiver>().Skill3_Casted.RemoveListener(Cast);
    }
    private void MoveWithinRange(PlayerCharacter ally)
    {
        this.ally = ally;
        if (Vector3.Distance(castingEntity.transform.position, ally.transform.position) > range)
        {
            castingEntity.GetComponent<CanMove>().Moved_Within_Range.AddListener(StartCasting);
            castingEntity.GetComponent<CanMove>().MoveWithinRange(ally.transform, range);
        }
        else
        {
            castingEntity.GetComponent<PlayerController>().Ally_Left_Clicked.RemoveAllListeners();
            StartCasting();
        }
    }
    private void StartCasting()
    {
        castingEntity.GetComponent<Character>().CastSkill3();
        castingEntity.GetComponent<PlayerController>().ChangeState(PlayerState.Busy);
        if (castingEntity.isOwned)
        {
            castingEntity.GetComponentInChildren<AnimatorEventReceiver>().Skill3_Casted.AddListener(Cast);
            castingEntity.GetComponent<Character>().RotateToPoint(ally.transform.position);
        }
    }
    private void Cast()
    {
        var proj = Instantiate(projectile, castingEntity.GetComponent<CanAttack>().projectileLaunchPoint.position, Quaternion.identity);
        proj.InitializeProjectile(new ProjectileData()
        {
            projectileTravel = ProjectileTravelType.Instant,
            projectileImpact = ProjectileImpactType.Single,
            impactEffect = ProjectileImpactEffect.Buff,
            buff = buff,
            targetedEntity = ally.GetComponent<HasHealth>()
        });
        PlayerController player = castingEntity.GetComponent<PlayerController>();
        player.GetComponent<HasMana>().CmdSpendMana(manaCost);
        player.StartCooldownW();
        player.GetComponentInChildren<AnimatorEventReceiver>().Skill3_Casted.RemoveAllListeners();
        player.ChangeState(PlayerState.None);
        player.ChangeCastingState(CastingState.None);
    }

    public override void UpdateDescription()
    {
        buff.duration = baseDuration;
        buff.value = baseHeal + healScalingValue * GetScalingStatValue(healScalingStat);
        description = GetTextIconByStat(PlayerStat.CooldownReduction) + (cooldown * castingEntity.GetComponent<CanAttack>().GetCooldownReductionModifier()).ToString("F1") +
            " " + GetTextIconByStat(PlayerStat.MaxMana) + manaCost +
            "\nBlesses target ally with restorative nature magic, increasing their " + GetTextIconByStat(PlayerStat.HealthRegen) + " by <color=orange>" + buff.value
            + "</color> " + "(" + baseHeal + " + " + (healScalingValue * 100).ToString("F0") + "% " + GetTextIconByStat(healScalingStat) + ")"
            + " for " + buff.duration.ToString("F1") + " seconds.";
        base.UpdateDescription();
    }
}
