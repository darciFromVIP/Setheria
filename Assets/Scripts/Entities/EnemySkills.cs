using Mirror;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class EnemySkills : NetworkBehaviour
{
    public void CastSnakePoison()
    {
        SSnakePoison skill = (SSnakePoison)GetComponent<EnemyCharacter>().skillInstances[0];
        var proj = Instantiate(skill.projectile, GetComponent<CanAttack>().projectileLaunchPoint.position, Quaternion.identity);
        proj.InitializeProjectile(new ProjectileData()
        {
            projectileTravel = ProjectileTravelType.Instant,
            projectileImpact = ProjectileImpactType.Single,
            impactEffect = ProjectileImpactEffect.Buff,
            buff = skill.poisonBuff,
            targetedEntity = skill.castingEntity.GetComponent<CanAttack>().enemyTarget
        });
        var proj2 = Instantiate(skill.projectile, GetComponent<CanAttack>().projectileLaunchPoint.position, Quaternion.identity);
        proj2.InitializeProjectile(new ProjectileData()
        {
            projectileTravel = ProjectileTravelType.Instant,
            projectileImpact = ProjectileImpactType.Single,
            impactEffect = ProjectileImpactEffect.Buff,
            buff = skill.slowBuff,
            targetedEntity = skill.castingEntity.GetComponent<CanAttack>().enemyTarget
        });
        NetworkServer.Spawn(proj.gameObject);
        NetworkServer.Spawn(proj2.gameObject);
    }
    public void CastStunningDash()
    {
        SStunningDash skill = (SStunningDash)GetComponent<EnemyCharacter>().skillInstances[0];
        foreach (var item in skill.enemiesHit)
        {
            var proj = Instantiate(skill.projectile, GetComponent<CanAttack>().projectileLaunchPoint.position, Quaternion.identity);
            proj.InitializeProjectile(new ProjectileData()
            {
                projectileTravel = ProjectileTravelType.Instant,
                projectileImpact = ProjectileImpactType.Single,
                impactEffect = ProjectileImpactEffect.Buff,
                buff = skill.stunBuff,
                targetedEntity = item.GetComponent<HasHealth>()
            });
            var proj2 = Instantiate(skill.projectile, GetComponent<CanAttack>().projectileLaunchPoint.position, Quaternion.identity);
            proj2.InitializeProjectile(new ProjectileData()
            {
                projectileTravel = ProjectileTravelType.Instant,
                projectileImpact = ProjectileImpactType.Single,
                impactEffect = ProjectileImpactEffect.Damage,
                effectValue = skill.baseDamage,
                targetedEntity = item.GetComponent<HasHealth>()
            });
            NetworkServer.Spawn(proj.gameObject);
        }
    }
    public void CastBash()
    {
        SBash skill = (SBash)GetComponent<EnemyCharacter>().skillInstances[0];
        var proj = Instantiate(skill.projectile, GetComponent<CanAttack>().projectileLaunchPoint.position, Quaternion.identity);
        proj.InitializeProjectile(new ProjectileData()
        {
            projectileTravel = ProjectileTravelType.Instant,
            projectileImpact = ProjectileImpactType.AoE,
            impactEffect = ProjectileImpactEffect.Damage,
            targetsMask = GetComponent<Character>().enemyLayers,
            aoeRadius = skill.radius,
            effectValue = skill.baseDamage,
            speed = 5,
            targetPoint = transform.position,
            affectsEntities = true,
            affectsStructures = true,
            owner = GetComponent<EnemyCharacter>()
        });
        NetworkServer.Spawn(proj.gameObject);
    }

}
