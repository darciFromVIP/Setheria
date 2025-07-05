using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lycandruid : NetworkBehaviour
{
    private int adrenalineRushLevel;
    private float adrenalineRushTimer;
    private bool regenerativeRage;
    private bool wildCompanion;

    private HasHealth hpComp;
    private void Start()
    {
        hpComp = GetComponent<HasHealth>();
    }
    private void Update()
    {
        if (adrenalineRushTimer > 0)
            adrenalineRushTimer -= Time.deltaTime;
    }
    public void CastBite()
    {
        SBite skill = (SBite)GetComponent<PlayerCharacter>().skillInstances[2];
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
        var player = GetComponent<PlayerCharacter>();
        if (wildCompanion)
        {
            var wolfSkill = (SCallOfTheWild)player.skillInstances[5];
            player.GetComponent<CanHavePets>().SpawnPet(wolfSkill.wolfPrefab.name, player.transform.position + (player.transform.right * 4), wolfSkill.duration, wolfSkill.finalPower);
        }
        SPounce skill = (SPounce)player.skillInstances[3];
        var proj = Instantiate(skill.projectile, skill.actualPoint, Quaternion.identity);
        proj.InitializeProjectile(new ProjectileData()
        {
            projectileTravel = ProjectileTravelType.Instant,
            projectileImpact = ProjectileImpactType.Single,
            impactEffect = ProjectileImpactEffect.Damage,
            effectValue = skill.finalDamage,
            targetedEntity = skill.enemy.GetComponent<HasHealth>(),
            owner = player
        });
        var proj2 = Instantiate(skill.projectile, skill.actualPoint, Quaternion.identity);
        proj2.InitializeProjectile(new ProjectileData()
        {
            projectileTravel = ProjectileTravelType.Instant,
            projectileImpact = ProjectileImpactType.Single,
            impactEffect = ProjectileImpactEffect.Buff,
            buff = skill.stunBuff,
            targetedEntity = skill.enemy.GetComponent<HasHealth>(),
            owner = player
        });
        NetworkServer.Spawn(proj.gameObject);
        NetworkServer.Spawn(proj2.gameObject);
    }
    public void CastBattleCry()
    {
        SBattleCry skill = (SBattleCry)GetComponent<PlayerCharacter>().skillInstances[4];
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
        SSwipe skill = (SSwipe)GetComponent<PlayerCharacter>().skillInstances[2];
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
        var player = GetComponent<PlayerCharacter>();
        if (wildCompanion)
        {
            var wolfSkill = (SCallOfTheWild)player.skillInstances[5];
            player.GetComponent<CanHavePets>().SpawnPet(wolfSkill.wolfPrefab.name, player.transform.position + (player.transform.right * 4), wolfSkill.duration, wolfSkill.finalPower);
        }
        SUppercut skill = (SUppercut)player.skillInstances[3];
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
            owner = player
        });
        NetworkServer.Spawn(proj.gameObject);
        NetworkServer.Spawn(proj2.gameObject);
        NetworkServer.Spawn(proj3.gameObject);
    }
    public void CastRoar()
    {
        SRoar skill = (SRoar)GetComponent<PlayerCharacter>().skillInstances[4];
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
        SWildRage skill = (SWildRage)GetComponent<PlayerCharacter>().skillInstances[5];
        var proj = Instantiate(skill.projectile, transform.position, Quaternion.identity);
        proj.InitializeProjectile(new ProjectileData()
        {
            projectileTravel = ProjectileTravelType.Instant,
            projectileImpact = ProjectileImpactType.Single,
            impactEffect = ProjectileImpactEffect.Buff,
            buff = skill.invulnerabilityBuff,
            targetedEntity = GetComponent<HasHealth>()
        });
        if (regenerativeRage)
        {
            var buff = GetComponent<PlayerCharacter>().buffDatabase.GetBuffByName("Invigoration");
            buff.duration = skill.duration;
            buff.value = GetComponent<HasHealth>().GetFinalMaxHealth() * 0.02f / skill.duration;
            var proj2 = Instantiate(skill.projectile, transform.position, Quaternion.identity);
            proj2.InitializeProjectile(new ProjectileData()
            {
                projectileTravel = ProjectileTravelType.Instant,
                projectileImpact = ProjectileImpactType.Single,
                impactEffect = ProjectileImpactEffect.Buff,
                buff = buff,
                targetedEntity = GetComponent<HasHealth>()
            });
            NetworkServer.Spawn(proj2.gameObject);
        }
        NetworkServer.Spawn(proj.gameObject);
    }

    [Command(requiresAuthority = false)]
    public void CmdLearnAdrenalineRush()
    {
        RpcLearnAdrenalineRush();
    }
    [ClientRpc]
    private void RpcLearnAdrenalineRush()
    {
        adrenalineRushLevel++;
        if (adrenalineRushLevel == 1)
            GetComponent<HasHealth>().Health_Changed.AddListener(AdrenalineRushTrigger);
    }
    [Command(requiresAuthority = false)]
    public void CmdUnlearnAdrenalineRush()
    {
        RpcUnlearnAdrenalineRush();
    }
    [ClientRpc]
    private void RpcUnlearnAdrenalineRush()
    {
        adrenalineRushLevel--;
        if (adrenalineRushLevel <= 0)
            GetComponent<HasHealth>().Health_Changed.RemoveListener(AdrenalineRushTrigger);
    }
    private void AdrenalineRushTrigger(float currentHealth, float maxHealth)
    {
        if (currentHealth / maxHealth <= 0.15f && adrenalineRushTimer <= 0)
        {
            var buff = GetComponent<PlayerCharacter>().buffDatabase.GetBuffByName("Adrenaline Rush");
            buff.value = (0.2f + adrenalineRushLevel * 0.1f) * maxHealth / 10;
            GetComponent<Character>().AddBuff("Adrenaline Rush");
            adrenalineRushTimer = 120 - (adrenalineRushLevel * 20);
        }
    }
    [Command(requiresAuthority = false)]
    public void CmdLearnRegenerativeRage()
    {
        RpcLearnRegenerativeRage();
    }
    [ClientRpc]
    private void RpcLearnRegenerativeRage()
    {
        regenerativeRage = true;
    }
    [Command(requiresAuthority = false)]
    public void CmdUnlearnRegenerativeRage()
    {
        RpcUnlearnRegenerativeRage();
    }
    [ClientRpc]
    private void RpcUnlearnRegenerativeRage()
    {
        regenerativeRage = false;
    }
    [Command(requiresAuthority = false)]
    public void CmdLearnCriticalSustain()
    {
        RpcLearnCriticalSustain();
    }
    [ClientRpc]
    private void RpcLearnCriticalSustain()
    {
        GetComponent<CanAttack>().Critically_Stricken.AddListener(CriticalSustainTrigger);
    }
    [Command(requiresAuthority = false)]
    public void CmdUnlearnCriticalSustain()
    {
        RpcUnlearnCriticalSustain();
    }
    [ClientRpc]
    private void RpcUnlearnCriticalSustain()
    {
        GetComponent<CanAttack>().Critically_Stricken.RemoveListener(CriticalSustainTrigger);
    }
    private void CriticalSustainTrigger()
    {
        hpComp.HealDamage(hpComp.GetFinalMaxHealth() * 0.01f, false);
    }
    [Command(requiresAuthority = false)]
    public void CmdLearnWildCompanion()
    {
        RpcLearnWildCompanion();
    }
    [ClientRpc]
    private void RpcLearnWildCompanion()
    {
        wildCompanion = true;
    }
    [Command(requiresAuthority = false)]
    public void CmdUnlearnWildCompanion()
    {
        RpcUnlearnWildCompanion();
    }
    [ClientRpc]
    private void RpcUnlearnWildCompanion()
    {
        wildCompanion = false;
    }
}
