using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Forest Protector/Entangling Roots")]
public class SEntanglingRoots : Skill
{
    public float range;
    public float baseDuration;
    public float baseDamage;
    public PlayerStat damageScalingStat;
    public float damageScalingValue;
    public BuffScriptable stunBuff;
    public BuffScriptable damageBuff;
    private EnemyCharacter enemy;
    public Projectile projectile;
    public override void Execute(Entity self)
    {
        base.Execute(self);
        self.GetComponent<PlayerController>().Enemy_Left_Clicked.AddListener(MoveWithinRange);
        self.GetComponent<PlayerController>().ChangeCastingState(CastingState.EnemyOnly);
    }

    public override void ExecuteOnStart(Entity self)
    {
    }

    public override void StopExecute()
    {
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
        castingEntity.GetComponent<Character>().CastSkill4();
        castingEntity.GetComponent<PlayerController>().ChangeState(PlayerState.Busy);
        if (castingEntity.isOwned)
        {
            castingEntity.GetComponentInChildren<AnimatorEventReceiver>().Skill4_Casted.AddListener(Cast);
            castingEntity.GetComponent<Character>().RotateToPoint(enemy.transform.position);
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
            buff = damageBuff,
            targetedEntity = enemy.GetComponent<HasHealth>()
        });
        var proj2 = Instantiate(projectile, castingEntity.GetComponent<CanAttack>().projectileLaunchPoint.position, Quaternion.identity);
        proj2.InitializeProjectile(new ProjectileData()
        {
            projectileTravel = ProjectileTravelType.Instant,
            projectileImpact = ProjectileImpactType.Single,
            impactEffect = ProjectileImpactEffect.Buff,
            buff = stunBuff,
            targetedEntity = enemy.GetComponent<HasHealth>()
        });
        PlayerController player = castingEntity.GetComponent<PlayerController>();
        player.GetComponent<HasMana>().SpendMana(manaCost);
        player.StartCooldownE();
        player.GetComponentInChildren<AnimatorEventReceiver>().Skill4_Casted.RemoveAllListeners();
        player.ChangeState(PlayerState.None);
        player.ChangeCastingState(CastingState.None);
    }
    public override void UpdateDescription()
    {
        base.UpdateDescription();
    }
}
