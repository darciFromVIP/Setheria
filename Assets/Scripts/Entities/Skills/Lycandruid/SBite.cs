using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Skills/Lycandruid/Bite")]
public class SBite : Skill
{
    public float baseDamage;
    public PlayerStat damageScalingStat;
    public float damageScalingValue;
    public float baseHeal;
    public PlayerStat healScalingStat;
    public float healScalingValue;
    [HideInInspector] public float finalDamage;
    [HideInInspector] public float finalHeal;
    public float range;
    public Projectile projectile;
    [HideInInspector] public EnemyCharacter enemy;

    [HideInInspector] public Vector3 actualPoint;
    public override void Execute(Character self)
    {
        base.Execute(self);
        self.GetComponent<PlayerController>().Enemy_Left_Clicked.AddListener(MoveWithinRange);
        self.GetComponent<PlayerController>().ChangeCastingState(CastingState.EnemyOnly);
        if (castingEntity.isOwned)
            castingEntity.skillIndicator.ShowRange(range, RPG_Indicator.RpgIndicator.IndicatorColor.Enemy, 0);
    }
    public override void StopExecute()
    {
        base.StopExecute();
        castingEntity.GetComponent<PlayerController>().Enemy_Left_Clicked.RemoveListener(MoveWithinRange);
        castingEntity.GetComponent<CanMove>().Moved_Within_Range.RemoveListener(StartCasting);
        if (castingEntity.isOwned)
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
        FindObjectOfType<AudioManager>().PlayOneShot(sound, castingEntity.transform.position);
        if (castingEntity.isOwned)
            castingEntity.GetComponent<Character>().CastSkill2();
        castingEntity.GetComponent<PlayerController>().ChangeState(PlayerState.Busy);
        castingEntity.GetComponent<CanMove>().Moved_Within_Range.RemoveListener(StartCasting);
        castingEntity.GetComponentInChildren<AnimatorEventReceiver>().Skill2_Casted.AddListener(Cast);
        castingEntity.GetComponent<Character>().RotateToPoint(enemy.transform.position);
        if (castingEntity.isOwned)
            castingEntity.skillIndicator.Casting(0);
    }
    protected override void Cast()
    {
        base.Cast();
        if (castingEntity.isServer)
            castingEntity.GetComponent<Lycandruid>().CastBite();
        PlayerController player = castingEntity.GetComponent<PlayerController>();
        if (castingEntity.isServer)
            player.GetComponent<HasMana>().RpcSpendMana(manaCost);
        player.StartCooldown2();
        player.GetComponentInChildren<AnimatorEventReceiver>().Skill2_Casted.RemoveListener(Cast);
        player.ChangeState(PlayerState.None);
        player.ChangeCastingState(CastingState.None);
    }
    public override void UpdateDescription()
    {
        finalDamage = baseDamage + GetScalingStatValue(damageScalingStat) * damageScalingValue;
        finalHeal = baseHeal + GetScalingStatValue(healScalingStat) * healScalingValue;
        description = GetTextIconByStat(PlayerStat.CooldownReduction) + (cooldown * castingEntity.GetComponent<CanAttack>().GetCooldownReductionModifier()).ToString("F1")
            + " " + GetTextIconByStat(PlayerStat.MaxMana) + manaCost + "\nBites the enemy target, dealing <color=orange>" + finalDamage
            + "</color> damage " + "(" + baseDamage + " + " + (int)(damageScalingValue * 100) + "% " + GetTextIconByStat(damageScalingStat) + ")"
            + " and healing self by <color=orange>" + finalHeal + "</color> (" + baseHeal + " + " + (int)(healScalingValue * 100) + "% " + GetTextIconByStat(healScalingStat) + ").";
        base.UpdateDescription();
    }
}