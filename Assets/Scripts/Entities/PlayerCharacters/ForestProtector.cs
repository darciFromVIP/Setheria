using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForestProtector : NetworkBehaviour
{
    private int manaMasteryLevel = 0;
    private int healingDustLevel = 0;
    public void CastGreenDust()
    {
        SGreenDust skill = (SGreenDust)GetComponent<PlayerCharacter>().skillInstances[2];
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
        if (healingDustLevel > 0)
            GetComponent<HasHealth>().CmdHealDamage(20 * healingDustLevel, false);
    }
    public void CastCorruptedDust()
    {
        SCorruptedDust skill = (SCorruptedDust)GetComponent<PlayerCharacter>().skillInstances[2];
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
        NetworkServer.Spawn(proj.gameObject);
        if (healingDustLevel > 0)
            GetComponent<HasHealth>().CmdHealDamage(20 * healingDustLevel, false);
    }
    public void CastRegeneratingDust()
    {
        SRegeneratingDust skill = (SRegeneratingDust)GetComponent<PlayerCharacter>().skillInstances[2];
        var proj = Instantiate(skill.healingProjectile, GetComponent<CanAttack>().projectileLaunchPoint.position, Quaternion.identity);
        proj.InitializeProjectile(new ProjectileData()
        {
            projectileTravel = ProjectileTravelType.TerrainTargeted,
            projectileImpact = ProjectileImpactType.AoE,
            impactEffect = ProjectileImpactEffect.Healing,
            targetsMask = LayerMask.GetMask("Player"),
            aoeRadius = skill.aoeRadius,
            effectValue = skill.finalHealing,
            speed = 5,
            targetPoint = skill.actualPoint,
            affectsOwner = true,
            affectsEntities = true,
            affectsStructures = false,
            owner = GetComponent<PlayerCharacter>()
        });
        NetworkServer.Spawn(proj.gameObject);
        if (healingDustLevel > 0)
            GetComponent<HasHealth>().CmdHealDamage(20 * healingDustLevel, false);
    }
    public void CastRejuvenation()
    {
        SRejuvenation skill = (SRejuvenation)GetComponent<PlayerCharacter>().skillInstances[3];
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
    public void CastDefilement()
    {
        SDefilement skill = (SDefilement)GetComponent<PlayerCharacter>().skillInstances[3];
        var proj = Instantiate(skill.projectile, GetComponent<CanAttack>().projectileLaunchPoint.position, Quaternion.identity);
        proj.InitializeProjectile(new ProjectileData()
        {
            projectileTravel = ProjectileTravelType.Instant,
            projectileImpact = ProjectileImpactType.Single,
            impactEffect = ProjectileImpactEffect.Buff,
            buff = skill.buff,
            targetedEntity = skill.enemy.GetComponent<HasHealth>()
        });
        NetworkServer.Spawn(proj.gameObject);
    }
    public void CastEntanglingRoots()
    {
        SEntanglingRoots skill = (SEntanglingRoots)GetComponent<PlayerCharacter>().skillInstances[4];
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

    public void LearnManaMastery()
    {
        manaMasteryLevel++;
        GetComponent<HasMana>().Mana_Spent.AddListener(ManaMasteryTrigger);
    }
    public void UnlearnManaMastery()
    {
        manaMasteryLevel--;
        if (manaMasteryLevel <= 0)
            GetComponent<HasMana>().Mana_Spent.RemoveListener(ManaMasteryTrigger);
    }
    private void ManaMasteryTrigger(float previousMana, float currentMana)
    {
        int random = Random.Range(0, 100);
        if (random >= 0 && random <= manaMasteryLevel * 5)
        {
            GetComponent<HasMana>().RestoreMana(previousMana - currentMana);
        }
    }
    public void LearnHealingDust()
    {
        healingDustLevel++;
    }
    public void UnlearnHealingDust()
    {
        healingDustLevel--;
    }
}
