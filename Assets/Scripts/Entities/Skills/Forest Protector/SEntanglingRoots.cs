using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Forest Protector/Entangling Roots")]
public class SEntanglingRoots : Skill
{
    public float range;
    public float baseDuration;
    public float baseDamage;
    private float finalDamage;
    public PlayerStat damageScalingStat;
    public float damageScalingValue;
    public BuffScriptable stunBuff;
    public BuffScriptable damageBuff;
    [HideInInspector] public EnemyCharacter enemy;
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
        damageBuff.value = finalDamage;
        damageBuff.duration = baseDuration;
        stunBuff.duration = baseDuration;
        if (castingEntity.isServer)
            castingEntity.GetComponent<Character>().CastSkill4();
        castingEntity.GetComponent<PlayerController>().ChangeState(PlayerState.Busy);
        castingEntity.GetComponent<CanMove>().Moved_Within_Range.RemoveListener(StartCasting);
        castingEntity.GetComponentInChildren<AnimatorEventReceiver>().Skill4_Casted.AddListener(Cast);
        castingEntity.GetComponent<Character>().RotateToPoint(enemy.transform.position);
    }
    private void Cast()
    {
        if (castingEntity.isServer)
            castingEntity.GetComponent<ForestProtector>().CastEntanglingRoots();
        PlayerController player = castingEntity.GetComponent<PlayerController>();
        FindObjectOfType<AudioManager>().PlayOneShot(sound, enemy.transform.position);
        if (castingEntity.isServer)
            player.GetComponent<HasMana>().RpcSpendMana(manaCost);
        player.StartCooldownE();
        player.GetComponentInChildren<AnimatorEventReceiver>().Skill4_Casted.RemoveAllListeners();
        player.ChangeState(PlayerState.None);
        player.ChangeCastingState(CastingState.None);
    }
    public override void UpdateDescription()
    {
        finalDamage = baseDamage + GetScalingStatValue(damageScalingStat) * damageScalingValue;
        stunBuff.duration = baseDuration;
        description = GetTextIconByStat(PlayerStat.CooldownReduction) + (cooldown * castingEntity.GetComponent<CanAttack>().GetCooldownReductionModifier()).ToString("F1")
            + " " + GetTextIconByStat(PlayerStat.MaxMana) + manaCost + "\nSummons roots from the ground to entangle a target enemy, inflicting Bleed (reduces "
            + GetTextIconByStat(PlayerStat.HealthRegen) + " by <color=orange>" + finalDamage.ToString("F2") + "</color> (" + baseDamage + " + " + (int)(damageScalingValue * 100) + "% " + GetTextIconByStat(damageScalingStat) +
            ")) and Rooted (stuns the target). Lasts " + stunBuff.duration + " seconds.";
        base.UpdateDescription();
    }
}
