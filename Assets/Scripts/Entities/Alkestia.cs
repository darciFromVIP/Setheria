using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Alkestia : NetworkBehaviour
{
    public void CastFlowerPowerHeal()
    {
        SFlowerPowerHeal skill = (SFlowerPowerHeal)GetComponent<Character>().skills[0];
        var proj = Instantiate(skill.projectile, transform.position, Quaternion.identity);
        proj.InitializeProjectile(new ProjectileData()
        {
            projectileTravel = ProjectileTravelType.Instant,
            projectileImpact = ProjectileImpactType.AoE,
            impactEffect = ProjectileImpactEffect.Healing,
            affectsEntities = true,
            targetsMask = LayerMask.GetMask("Player"),
            aoeRadius = skill.range,
            affectsOwner = false,
            effectValue = skill.heal,
        });
        NetworkServer.Spawn(proj.gameObject);
    }
}
