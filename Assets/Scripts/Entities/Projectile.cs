using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using FMODUnity;
public enum ProjectileTravelType
{
    Skillshot, EntityTargeted, TerrainTargeted, Instant
}
public enum ProjectileImpactType
{
    Single, AoE
}
public enum ProjectileImpactEffect
{
    Damage, Healing, Buff
}
public struct ProjectileData
{
    public float effectValue;
    public bool isCritical;
    public BuffScriptable buff;
    public ProjectileTravelType projectileTravel;
    public ProjectileImpactType projectileImpact;
    public ProjectileImpactEffect impactEffect;
    public HasHealth targetedEntity;
    public Vector3 targetPoint;
    public float skillshotRange;
    public float aoeRadius;
    public float speed;
    public LayerMask targetsMask;
    public bool ignoresArmor;
    public Entity owner;
    public bool affectsStructures;
    public bool affectsEntities;
    public bool affectsOwner;
}
public class Projectile : NetworkBehaviour
{
    private ProjectileData data;
    public EventReference launchSound;
    public EventReference impactSound;
    public GameObject impactParticlePrefab;
    private void OnTriggerEnter(Collider other)
    {
        if (data.projectileTravel == ProjectileTravelType.Skillshot && (data.targetsMask.value & (1 << other.transform.gameObject.layer)) > 0)
        {
            data.targetedEntity = other.GetComponent<HasHealth>();
            ProjectileImpact();
            GetComponent<Collider>().enabled = false;
        }
    }
    public void InitializeProjectile(ProjectileData data)
    {
        this.data = data;
        StartCoroutine(WaitForServerLoad());
    }
    private void PlaySound(EventReference sound)
    {
        FindObjectOfType<AudioManager>().PlayOneShot(sound, transform.position);
    }
    private IEnumerator WaitForServerLoad()
    {
        while (!(isClient || isServer)) 
        {
            yield return null;
        }

        if (isServer && data.targetedEntity == null && data.projectileTravel == ProjectileTravelType.EntityTargeted)
            NetworkServer.Destroy(gameObject);

        if (!launchSound.IsNull)
            PlaySound(launchSound);

        switch (data.projectileTravel)
        {
            case ProjectileTravelType.Skillshot:
                StartCoroutine(Skillshot());
                break;
            case ProjectileTravelType.EntityTargeted:
                StartCoroutine(EntityTargeted());
                break;
            case ProjectileTravelType.TerrainTargeted:
                StartCoroutine(TerrainTargeted());
                break;
            case ProjectileTravelType.Instant:
                ProjectileImpact();
                break;
            default:
                break;
        }
    }
    private IEnumerator EntityTargeted()
    {
        while (true)
        {
            if (data.targetedEntity == null)
                yield break;
            transform.position = Vector3.MoveTowards(transform.position, data.targetedEntity.transform.position, Time.deltaTime * data.speed);
            if (Vector3.Distance(transform.position, data.targetedEntity.transform.position) < 0.5)
            {
                ProjectileImpact();
                break;
            }
            yield return null;
        }
    }
    private IEnumerator TerrainTargeted()
    {
        while (true)
        {
            transform.position = Vector3.MoveTowards(transform.position, data.targetPoint, Time.deltaTime * data.speed);
            if (Vector3.Distance(transform.position, data.targetPoint) < 0.5)
            {
                ProjectileImpact();
                break;
            }
            yield return null;
        }
    }
    private IEnumerator Skillshot()
    {
        var rot = Quaternion.LookRotation(data.targetPoint - data.owner.transform.position);
        rot.z = 0;
        rot.x = 0;
        transform.rotation = rot;
        Vector3 startingPos = transform.position;
        while (true)
        {
            transform.position += transform.forward * Time.deltaTime * data.speed;
            if (Vector3.Distance(transform.position, startingPos) > data.skillshotRange)
            {
                ProjectileImpact();
                break;
            }
            yield return null;
        }
    }
    private void ProjectileImpact()
    {
        if (!launchSound.IsNull)
            PlaySound(impactSound);
        var targets = new List<HasHealth>();
        switch (data.projectileImpact)
        {
            case ProjectileImpactType.Single:
                if (data.targetedEntity != null)
                    targets.Add(data.targetedEntity);
                break;
            case ProjectileImpactType.AoE:
                Collider[] colliders = new Collider[20];
                Physics.OverlapSphereNonAlloc(transform.position, data.aoeRadius, colliders, data.targetsMask);
                foreach (var item in colliders)
                {
                    if (item != null)
                        if (item.TryGetComponent(out HasHealth entity))
                            targets.Add(entity);
                }
                HasHealth[] copy = new HasHealth[20];
                targets.CopyTo(copy);
                foreach (var item in copy)
                {
                    if (item)
                    {
                        if (item.TryGetComponent(out Structure structure) && !data.affectsStructures)
                        {
                            targets.Remove(item);
                        }
                        if (item.TryGetComponent(out Entity entity) && !data.affectsEntities)
                        {
                            targets.Remove(item);
                        }
                        if (item.TryGetComponent(out Entity owner) && !data.affectsOwner)
                        {
                            if (owner == data.owner)
                                targets.Remove(item);
                        }
                    }
                }
                break;
            default:
                break;
        }
        ProjectileEffect(targets);
    }
    private void ProjectileEffect(List<HasHealth> entities)
    {
        if (entities.Count != 0)
        {
            switch (data.impactEffect)
            {
                case ProjectileImpactEffect.Damage:
                    foreach (var item in entities)
                    {
                        if (isServer)
                            item.RpcTakeDamage(data.effectValue, data.ignoresArmor, data.owner.GetComponent<NetworkIdentity>(), data.isCritical, true);
                    }
                    break;
                case ProjectileImpactEffect.Healing:
                    foreach (var item in entities)
                    {
                        if (isServer)
                            item.RpcHealDamage(data.effectValue, false);
                    }
                    break;
                case ProjectileImpactEffect.Buff:
                    foreach (var item in entities)
                    {
                        if (isServer)
                        {
                            item.GetComponent<Character>().AddBuff(data.buff.name);
                            item.GetComponent<Character>().RpcAddBuff(data.buff.name);
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        if (impactParticlePrefab)
        {
            Instantiate(impactParticlePrefab, transform.position, Quaternion.identity);
        }
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
        if (isServer)
            StartCoroutine(DelayedDestroy());
    }
    private IEnumerator DelayedDestroy()
    {
        yield return new WaitForSeconds(2);
        NetworkServer.Destroy(gameObject);
    }
}
