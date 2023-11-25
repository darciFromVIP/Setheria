using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForestProtector : NetworkBehaviour
{
    public void CastGreenDust()
    {
        SGreenDust skill = (SGreenDust)GetComponent<PlayerCharacter>().skills[2];
        var proj = Instantiate(skill.damageProjectile, GetComponent<CanAttack>().projectileLaunchPoint.position, Quaternion.identity);
        proj.InitializeProjectile(new ProjectileData()
        {
            projectileTravel = ProjectileTravelType.TerrainTargeted,
            projectileImpact = ProjectileImpactType.AoE,
            impactEffect = ProjectileImpactEffect.Damage,
            targetsMask = LayerMask.GetMask("Enemy"),
            aoeRadius = skill.aoeRadius,
            effectValue = skill.finalDamage,
            speed = 5,
            targetPoint = skill.actualPoint,
            affectsEntities = true,
            owner = GetComponent<PlayerCharacter>()
        });
        var proj2 = Instantiate(skill.healingProjectile, GetComponent<CanAttack>().projectileLaunchPoint.position, Quaternion.identity);
        proj2.InitializeProjectile(new ProjectileData()
        {
            projectileTravel = ProjectileTravelType.TerrainTargeted,
            projectileImpact = ProjectileImpactType.AoE,
            impactEffect = ProjectileImpactEffect.Healing,
            targetsMask = LayerMask.GetMask("Player"),
            aoeRadius = skill.aoeRadius,
            effectValue = skill.finalHeal,
            speed = 5,
            targetPoint = skill.actualPoint,
            affectsOwner = false,
            affectsEntities = true,
            affectsStructures = false,
            owner = GetComponent<PlayerCharacter>()
        });
        NetworkServer.Spawn(proj.gameObject);
        NetworkServer.Spawn(proj2.gameObject);
    }
    public void CastRejuvenation()
    {
        SRejuvenation skill = (SRejuvenation)GetComponent<PlayerCharacter>().skills[3];
        var proj = Instantiate(skill.projectile, GetComponent<CanAttack>().projectileLaunchPoint.position, Quaternion.identity);
        proj.InitializeProjectile(new ProjectileData()
        {
            projectileTravel = ProjectileTravelType.Instant,
            projectileImpact = ProjectileImpactType.Single,
            impactEffect = ProjectileImpactEffect.Buff,
            buff = skill.buff,
            targetedEntity = skill.ally.GetComponent<HasHealth>()
        });
        NetworkServer.Spawn(proj.gameObject);
    }
    public void CastEntanglingRoots()
    {
        SEntanglingRoots skill = (SEntanglingRoots)GetComponent<PlayerCharacter>().skills[4];
        var proj = Instantiate(skill.projectile, GetComponent<CanAttack>().projectileLaunchPoint.position, Quaternion.identity);
        proj.InitializeProjectile(new ProjectileData()
        {
            projectileTravel = ProjectileTravelType.Instant,
            projectileImpact = ProjectileImpactType.Single,
            impactEffect = ProjectileImpactEffect.Buff,
            buff = skill.damageBuff,
            targetedEntity = skill.enemy.GetComponent<HasHealth>()
        });
        var proj2 = Instantiate(skill.projectile, GetComponent<CanAttack>().projectileLaunchPoint.position, Quaternion.identity);
        proj2.InitializeProjectile(new ProjectileData()
        {
            projectileTravel = ProjectileTravelType.Instant,
            projectileImpact = ProjectileImpactType.Single,
            impactEffect = ProjectileImpactEffect.Buff,
            buff = skill.stunBuff,
            targetedEntity = skill.enemy.GetComponent<HasHealth>()
        });
        NetworkServer.Spawn(proj.gameObject);
        NetworkServer.Spawn(proj2.gameObject);
    }
}
