using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lycandruid : NetworkBehaviour
{
    public void CastBite()
    {
        SBite skill = (SBite)GetComponent<PlayerCharacter>().skills[2];
        var proj = Instantiate(skill.projectile, skill.actualPoint, Quaternion.identity);
        proj.InitializeProjectile(new ProjectileData()
        {
            projectileTravel = ProjectileTravelType.Instant,
            projectileImpact = ProjectileImpactType.Single,
            impactEffect = ProjectileImpactEffect.Damage,
            effectValue = skill.finalDamage,
            targetedEntity = skill.enemy.GetComponent<HasHealth>(),
            owner = GetComponent<Entity>()
        });
        var proj2 = Instantiate(skill.projectile, skill.actualPoint, Quaternion.identity);
        proj2.InitializeProjectile(new ProjectileData()
        {
            projectileTravel = ProjectileTravelType.Instant,
            projectileImpact = ProjectileImpactType.Single,
            impactEffect = ProjectileImpactEffect.Healing,
            effectValue = skill.finalHeal,
            targetedEntity = GetComponent<HasHealth>(),
            owner = GetComponent<Entity>()
        });
        NetworkServer.Spawn(proj.gameObject);
        NetworkServer.Spawn(proj2.gameObject);
    }
    public void CastPounce()
    {
        SPounce skill = (SPounce)GetComponent<PlayerCharacter>().skills[3];
        var proj = Instantiate(skill.projectile, skill.actualPoint, Quaternion.identity);
        proj.InitializeProjectile(new ProjectileData()
        {
            projectileTravel = ProjectileTravelType.Instant,
            projectileImpact = ProjectileImpactType.Single,
            impactEffect = ProjectileImpactEffect.Damage,
            effectValue = skill.finalDamage,
            targetedEntity = skill.enemy.GetComponent<HasHealth>(),
            owner = GetComponent<Entity>()
        });
        var proj2 = Instantiate(skill.projectile, skill.actualPoint, Quaternion.identity);
        proj2.InitializeProjectile(new ProjectileData()
        {
            projectileTravel = ProjectileTravelType.Instant,
            projectileImpact = ProjectileImpactType.Single,
            impactEffect = ProjectileImpactEffect.Buff,
            buff = skill.stunBuff,
            targetedEntity = skill.enemy.GetComponent<HasHealth>(),
            owner = GetComponent<Entity>()
        });
        NetworkServer.Spawn(proj.gameObject);
        NetworkServer.Spawn(proj2.gameObject);
    }
    public void CastBattleCry()
    {
        SBattleCry skill = (SBattleCry)GetComponent<PlayerCharacter>().skills[4];
        var proj = Instantiate(skill.projectile, transform.position, Quaternion.identity);
        proj.InitializeProjectile(new ProjectileData()
        {
            projectileTravel = ProjectileTravelType.Instant,
            projectileImpact = ProjectileImpactType.AoE,
            impactEffect = ProjectileImpactEffect.Buff,
            buff = skill.movementBuff,
            affectsEntities = true,
            targetsMask = LayerMask.GetMask("Player"),
            aoeRadius = skill.range,
            affectsOwner = true
        });
        var proj2 = Instantiate(skill.projectile, transform.position, Quaternion.identity);
        proj2.InitializeProjectile(new ProjectileData()
        {
            projectileTravel = ProjectileTravelType.Instant,
            projectileImpact = ProjectileImpactType.AoE,
            impactEffect = ProjectileImpactEffect.Buff,
            buff = skill.attackSpeedBuff,
            affectsEntities = true,
            targetsMask = LayerMask.GetMask("Player"),
            aoeRadius = skill.range,
            affectsOwner = true
        });
        NetworkServer.Spawn(proj.gameObject);
        NetworkServer.Spawn(proj2.gameObject);
    }
    public void CastSwipe()
    {
        SSwipe skill = (SSwipe)GetComponent<PlayerCharacter>().skills[2];
        var proj = Instantiate(skill.damageProjectile, skill.actualPoint, Quaternion.identity);
        proj.InitializeProjectile(new ProjectileData()
        {
            projectileTravel = ProjectileTravelType.Instant,
            projectileImpact = ProjectileImpactType.AoE,
            impactEffect = ProjectileImpactEffect.Damage,
            targetsMask = LayerMask.GetMask("Enemy"),
            aoeRadius = skill.aoeRadius,
            effectValue = skill.finalDamage,
            affectsEntities = true,
            owner = GetComponent<Entity>()
        });
        var proj2 = Instantiate(skill.damageProjectile, skill.actualPoint, Quaternion.identity);
        proj2.InitializeProjectile(new ProjectileData()
        {
            projectileTravel = ProjectileTravelType.Instant,
            projectileImpact = ProjectileImpactType.AoE,
            impactEffect = ProjectileImpactEffect.Buff,
            targetsMask = LayerMask.GetMask("Enemy"),
            buff = skill.bleedingBuff,
            aoeRadius = skill.aoeRadius,
            affectsEntities = true,
            owner = GetComponent<Entity>()
        });
        NetworkServer.Spawn(proj.gameObject);
        NetworkServer.Spawn(proj2.gameObject);
    }
    public void CastUppercut()
    {
        SUppercut skill = (SUppercut)GetComponent<PlayerCharacter>().skills[3];
        var proj = Instantiate(skill.projectile, transform.position, Quaternion.identity);
        proj.InitializeProjectile(new ProjectileData()
        {
            projectileTravel = ProjectileTravelType.Instant,
            projectileImpact = ProjectileImpactType.Single,
            impactEffect = ProjectileImpactEffect.Buff,
            buff = skill.armorReductionBuff,
            targetedEntity = skill.enemy.GetComponent<HasHealth>()
        });
        var proj2 = Instantiate(skill.projectile, transform.position, Quaternion.identity);
        proj2.InitializeProjectile(new ProjectileData()
        {
            projectileTravel = ProjectileTravelType.Instant,
            projectileImpact = ProjectileImpactType.Single,
            impactEffect = ProjectileImpactEffect.Buff,
            buff = skill.damageReductionBuff,
            targetedEntity = skill.enemy.GetComponent<HasHealth>()
        });
        var proj3 = Instantiate(skill.projectile, transform.position, Quaternion.identity);
        proj3.InitializeProjectile(new ProjectileData()
        {
            projectileTravel = ProjectileTravelType.Instant,
            projectileImpact = ProjectileImpactType.Single,
            impactEffect = ProjectileImpactEffect.Damage,
            effectValue = skill.damageFinal,
            targetedEntity = skill.enemy.GetComponent<HasHealth>(),
            owner = GetComponent<Entity>()
        });
        NetworkServer.Spawn(proj.gameObject);
        NetworkServer.Spawn(proj2.gameObject);
        NetworkServer.Spawn(proj3.gameObject);
    }
    public void CastRoar()
    {
        SRoar skill = (SRoar)GetComponent<PlayerCharacter>().skills[4];
        var proj = Instantiate(skill.projectile, transform.position, Quaternion.identity);
        proj.InitializeProjectile(new ProjectileData()
        {
            projectileTravel = ProjectileTravelType.Instant,
            projectileImpact = ProjectileImpactType.AoE,
            impactEffect = ProjectileImpactEffect.Buff,
            buff = skill.movementReductionBuff,
            affectsEntities = true,
            targetsMask = LayerMask.GetMask("Enemy"),
            aoeRadius = skill.range
        });
        var proj2 = Instantiate(skill.projectile, transform.position, Quaternion.identity);
        proj2.InitializeProjectile(new ProjectileData()
        {
            projectileTravel = ProjectileTravelType.Instant,
            projectileImpact = ProjectileImpactType.AoE,
            impactEffect = ProjectileImpactEffect.Buff,
            buff = skill.attackSpeedReductionBuff,
            affectsEntities = true,
            targetsMask = LayerMask.GetMask("Enemy"),
            aoeRadius = skill.range
        });
        NetworkServer.Spawn(proj.gameObject);
        NetworkServer.Spawn(proj2.gameObject);
    }
    public void CastWildRage()
    {
        SWildRage skill = (SWildRage)GetComponent<PlayerCharacter>().skills[5];
        var proj = Instantiate(skill.projectile, transform.position, Quaternion.identity);
        proj.InitializeProjectile(new ProjectileData()
        {
            projectileTravel = ProjectileTravelType.Instant,
            projectileImpact = ProjectileImpactType.Single,
            impactEffect = ProjectileImpactEffect.Buff,
            buff = skill.maxHPBuff,
            targetedEntity = GetComponent<HasHealth>()
        });
        var proj2 = Instantiate(skill.projectile, transform.position, Quaternion.identity);
        proj2.InitializeProjectile(new ProjectileData()
        {
            projectileTravel = ProjectileTravelType.Instant,
            projectileImpact = ProjectileImpactType.Single,
            impactEffect = ProjectileImpactEffect.Buff,
            buff = skill.cooldownReductionBuff,
            targetedEntity = GetComponent<HasHealth>()
        });
        NetworkServer.Spawn(proj.gameObject);
        NetworkServer.Spawn(proj2.gameObject);
    }
}
