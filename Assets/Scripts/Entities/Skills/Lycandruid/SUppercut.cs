using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Skills/Lycandruid/Uppercut")]
public class SUppercut : Skill
{
    public float range;
    public float baseDuration;
    public float baseDamage;
    public PlayerStat damageScalingStat;
    public float damageScalingValue;
    [HideInInspector] public float damageFinal;
    public float armorBaseReduction;
    [HideInInspector] public float armorFinalReduction;
    public PlayerStat armorReductionScalingStat;
    public float armorReductionScalingValue;
    public float damageBaseReduction;
    [HideInInspector] public float damageFinalReduction;
    public PlayerStat damageReductionScalingStat;
    public float damageReductionScalingValue;
    public BuffScriptable armorReductionBuff;
    public BuffScriptable damageReductionBuff;
    [HideInInspector] public EnemyCharacter enemy;
    public Projectile projectile;
    public override void Execute(Character self)
    {
        base.Execute(self);
        self.GetComponent<PlayerController>().Enemy_Left_Clicked.AddListener(MoveWithinRange);
        self.GetComponent<PlayerController>().ChangeCastingState(CastingState.EnemyOnly);
        castingEntity.skillIndicator.ShowRange(range, RPG_Indicator.RpgIndicator.IndicatorColor.Enemy, 0);
    }

    public override void ExecuteOnStart(Character self)
    {
    }

    public override void StopExecute()
    {
        base.StopExecute();
        castingEntity.GetComponent<PlayerController>().Enemy_Left_Clicked.RemoveListener(MoveWithinRange);
        castingEntity.GetComponent<CanMove>().Moved_Within_Range.RemoveListener(StartCasting);
        castingEntity.skillIndicator.InterruptCasting();
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
        armorReductionBuff.duration = baseDuration;
        damageReductionBuff.duration = baseDuration;
        armorReductionBuff.value = armorFinalReduction;
        damageReductionBuff.value = damageFinalReduction;
        FindObjectOfType<AudioManager>().PlayOneShot(sound, castingEntity.transform.position);
        castingEntity.GetComponent<PlayerController>().ChangeState(PlayerState.Busy);
        castingEntity.GetComponent<CanMove>().Moved_Within_Range.RemoveListener(StartCasting);
        castingEntity.GetComponentInChildren<AnimatorEventReceiver>().Skill3_Casted.AddListener(Cast);
        castingEntity.GetComponent<Character>().RotateToPoint(enemy.transform.position);
        if (castingEntity.isServer)
            castingEntity.GetComponent<Character>().CastSkill3();
        castingEntity.skillIndicator.Casting(0);
    }
    protected override void Cast()
    {
        base.Cast();
        if (castingEntity.isServer)
            castingEntity.GetComponent<Lycandruid>().CastUppercut();
        PlayerController player = castingEntity.GetComponent<PlayerController>();
        if (castingEntity.isServer)
            player.GetComponent<HasMana>().RpcSpendMana(manaCost);
        player.StartCooldown3();
        player.GetComponentInChildren<AnimatorEventReceiver>().Skill3_Casted.RemoveAllListeners();
        player.ChangeState(PlayerState.None);
        player.ChangeCastingState(CastingState.None);
    }
    public override void UpdateDescription()
    {
        damageFinal = baseDamage + (GetScalingStatValue(damageScalingStat) * damageScalingValue);
        armorFinalReduction = (armorBaseReduction + GetScalingStatValue(armorReductionScalingStat) * armorReductionScalingValue) * -1;
        damageFinalReduction = (damageBaseReduction + GetScalingStatValue(damageReductionScalingStat) * damageReductionScalingValue) * -1;
        armorReductionBuff.duration = baseDuration;
        description = GetTextIconByStat(PlayerStat.CooldownReduction) + (cooldown * castingEntity.GetComponent<CanAttack>().GetCooldownReductionModifier()).ToString("F1")
            + " " + GetTextIconByStat(PlayerStat.MaxMana) + manaCost + "\nPerforms an uppercut, which deals <color=orange>" + damageFinal + "</color> damage ("
            + baseDamage + " + " + (damageScalingValue * 100).ToString("F0") + "% " + GetTextIconByStat(damageScalingStat) + ") and applies Cripple (reduces "
            + GetTextIconByStat(PlayerStat.Power) + " by <color=orange>" + (damageFinalReduction * -1).ToString("F2") + "</color> (" + damageBaseReduction + " + "
            + (int)(damageReductionScalingValue * 100) + "% " + GetTextIconByStat(damageReductionScalingStat) +
            ")) and Sunder (reduces " + GetTextIconByStat(PlayerStat.Armor) + " by <color=orange>" + (armorFinalReduction * -1).ToString("F2") + "</color> ("
            + armorBaseReduction + " + " + (int)(armorReductionScalingValue * 100) + "% " + GetTextIconByStat(armorReductionScalingStat) + "))." +
            " Lasts " + armorReductionBuff.duration + " seconds.";
        base.UpdateDescription();
    }
}
