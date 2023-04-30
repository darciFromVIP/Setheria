using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Skills/Lycandruid/Pounce")]
public class SPounce : Skill
{
    public float baseDamage;
    public PlayerStat damageScalingStat;
    public float damageScalingValue;
    private float finalDamage;
    public float baseDuration;
    public float range;
    public Projectile projectile;
    public BuffScriptable stunBuff;
    private EnemyCharacter enemy;

    private Vector3 actualPoint;
    public override void Execute(Character self)
    {
        base.Execute(self);
        self.GetComponent<PlayerController>().Enemy_Left_Clicked.AddListener(MoveWithinRange);
        self.GetComponent<PlayerController>().ChangeCastingState(CastingState.EnemyOnly);
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
        castingEntity.StartCoroutine(Jump());
    }
    private IEnumerator Jump()
    {
        var moveComp = castingEntity.GetComponent<CanMove>();
        moveComp.agent.enabled = false;
        float minDistance = moveComp.agent.stoppingDistance + castingEntity.GetComponent<CanAttack>().GetAttackRange();
        while (Vector3.Distance(castingEntity.transform.position, enemy.transform.position) > minDistance)
        {
            castingEntity.transform.position = Vector3.MoveTowards(castingEntity.transform.position, enemy.transform.position, Time.deltaTime * 25);
            yield return null;
        }
        moveComp.agent.enabled = true;
        CastEffect();
    }
    private void CastEffect()
    {
        stunBuff.duration = baseDuration;
        var proj = Instantiate(projectile, actualPoint, Quaternion.identity);
        proj.InitializeProjectile(new ProjectileData()
        {
            projectileTravel = ProjectileTravelType.Instant,
            projectileImpact = ProjectileImpactType.Single,
            impactEffect = ProjectileImpactEffect.Damage,
            effectValue = finalDamage,
            targetedEntity = enemy.GetComponent<HasHealth>(),
            owner = castingEntity
        });
        var proj2 = Instantiate(projectile, actualPoint, Quaternion.identity);
        proj2.InitializeProjectile(new ProjectileData()
        {
            projectileTravel = ProjectileTravelType.Instant,
            projectileImpact = ProjectileImpactType.Single,
            impactEffect = ProjectileImpactEffect.Buff,
            buff = stunBuff,
            targetedEntity = enemy.GetComponent<HasHealth>(),
            owner = castingEntity
        });
        PlayerController player = castingEntity.GetComponent<PlayerController>();
        player.GetComponent<HasMana>().SpendMana(manaCost);
        player.StartCooldownW();
        player.GetComponentInChildren<AnimatorEventReceiver>().Skill3_Casted.RemoveListener(Cast);
        player.ChangeState(PlayerState.None);
        player.ChangeCastingState(CastingState.None);
    }
    public override void UpdateDescription()
    {
        finalDamage = baseDamage + GetScalingStatValue(damageScalingStat) * damageScalingValue;
        description = GetTextIconByStat(PlayerStat.CooldownReduction) + (cooldown * castingEntity.GetComponent<CanAttack>().GetCooldownReductionModifier()).ToString("F1")
            + " " + GetTextIconByStat(PlayerStat.MaxMana) + manaCost + "\nPounces to the enemy target, dealing <color=orange>" + finalDamage
            + "</color> damage " + "(" + baseDamage + " + " + (int)(damageScalingValue * 100) + "% " + GetTextIconByStat(damageScalingStat) + ")"
            + " and stunning it for " + baseDuration + " seconds.";
        base.UpdateDescription();
    }
}
