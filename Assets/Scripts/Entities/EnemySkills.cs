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
        SSnakePoison skill = (SSnakePoison)GetComponent<EnemyCharacter>().skills[0];
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
        SStunningDash skill = (SStunningDash)GetComponent<EnemyCharacter>().skills[0];
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
            NetworkServer.Spawn(proj.gameObject);
        }
    }
}
