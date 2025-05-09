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
    [HideInInspector] public PlayerCharacter ally;
    public Projectile projectile;
    public override void Execute(Character self)
    {
        base.Execute(self);
        self.GetComponent<PlayerController>().Ally_Left_Clicked.AddListener(MoveWithinRange);
        self.GetComponent<PlayerController>().ChangeCastingState(CastingState.AllyOnly);
        if (castingEntity.isOwned)
            castingEntity.skillIndicator.ShowRange(range, RPG_Indicator.RpgIndicator.IndicatorColor.Ally, 0);
    }
    public override Skill GetInstance()
    {
        var instance = (SRejuvenation)base.GetInstance();
        instance.baseHeal = baseHeal;
        instance.healScalingStat = healScalingStat;
        instance.healScalingValue = healScalingValue;
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
    protected override void StartCasting()
    {
        base.StartCasting();
        if (castingEntity.isOwned)
            castingEntity.GetComponent<Character>().CastSkill3();
        castingEntity.GetComponentInChildren<AnimatorEventReceiver>().Skill3_Casted.AddListener(Cast);
        castingEntity.GetComponent<Character>().RotateToPoint(ally.transform.position);
        if (castingEntity.isOwned)
            castingEntity.skillIndicator.Casting(0.86f);
    }
    protected override void Cast()
    {
        base.Cast();
        if (castingEntity.isServer)
            castingEntity.GetComponent<ForestProtector>().CastRejuvenation();
        PlayerController player = castingEntity.GetComponent<PlayerController>();
        if (castingEntity.isServer)
            player.GetComponent<HasMana>().RpcSpendMana(manaCost);
        player.StartCooldown3();
        player.GetComponentInChildren<AnimatorEventReceiver>().Skill3_Casted.RemoveAllListeners();
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
