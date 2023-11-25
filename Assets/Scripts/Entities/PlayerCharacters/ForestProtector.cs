using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForestProtector : NetworkBehaviour
{
    [Command(requiresAuthority = false)]
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

}
