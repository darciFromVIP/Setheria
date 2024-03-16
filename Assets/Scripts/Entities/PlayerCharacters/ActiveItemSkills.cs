using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveItemSkills : NetworkBehaviour
{
    public ISBlowpipe blowpipeSkill;

    [Command(requiresAuthority = false)]
    public void CmdCastBlowpipe(NetworkIdentity enemy)
    {
        Debug.Log("Casting Blowpipe");
        var proj = Instantiate(blowpipeSkill.projectile, transform.position + Vector3.up * 2, Quaternion.identity);
        proj.InitializeProjectile(new ProjectileData()
        {
            projectileTravel = ProjectileTravelType.EntityTargeted,
            projectileImpact = ProjectileImpactType.Single,
            impactEffect = ProjectileImpactEffect.Buff,
            targetedEntity = enemy.GetComponent<HasHealth>(),
            buff = blowpipeSkill.sleepBuff,
            owner = blowpipeSkill.castingEntity,
            speed = 6
        });
        NetworkServer.Spawn(proj.gameObject);
    }
}
