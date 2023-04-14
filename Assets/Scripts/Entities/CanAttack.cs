using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Mirror;
public enum AttackType
{
    Melee, Ranged
}
public class CanAttack : NetworkBehaviour
{
    [SyncVar] [SerializeField] private float power;
    [SyncVar] [SerializeField] private float criticalChance = 0f;
    [SyncVar] [SerializeField] private float criticalDamage = 0f;
    [SyncVar] [SerializeField] private float attackSpeed;
    public float attackSpeedTimer = 0;
    [SyncVar] [SerializeField] private float attackRange;
    [SyncVar] [SerializeField] private float cooldownReduction = 0;
    [SyncVar] [SerializeField] private AttackType attackType;
    public bool canAct = true;

    public GameObject projectilePrefab;
    public Transform projectileLaunchPoint;

    public HasHealth enemyTarget;
    private CanMove moveComp;
    private NetworkAnimator netAnim;
    private Entity entity;

    [System.NonSerialized]
    public UnityEvent<NetworkIdentity> Target_Acquired = new();
    [System.NonSerialized]
    public UnityEvent Target_Lost = new();
    [System.NonSerialized]
    public UnityEvent<float> Power_Changed = new();
    [System.NonSerialized]
    public UnityEvent<float> Critical_Chance_Changed = new();
    [System.NonSerialized]
    public UnityEvent<float> Critical_Damage_Changed = new();
    [System.NonSerialized]
    public UnityEvent<float> Attack_Speed_Changed = new();
    [System.NonSerialized]
    public UnityEvent<float> Attack_Range_Changed = new();
    [System.NonSerialized]
    public UnityEvent<float> Cooldown_Reduction_Changed = new();
    [HideInInspector] public UnityEvent Stop_Acting = new();
    [HideInInspector] public UnityEvent Resume_Acting = new();
    [HideInInspector] public UnityEvent Has_Attacked = new();

    protected int animHash_Attack1 = Animator.StringToHash("Attack1");
    protected int animHash_Attack2 = Animator.StringToHash("Attack2");
    protected int animHash_Attack3 = Animator.StringToHash("Attack3");
    protected int animHash_Attack4 = Animator.StringToHash("Attack4");

    private void Start()
    {
        if (TryGetComponent(out HasAggro aggro))
        {
            if (isServer)
                aggro.Target_Found.AddListener(RpcTargetAcquired);
            else if (isOwned)
                aggro.Target_Found.AddListener(CmdTargetAcquired);
        }
        moveComp = GetComponent<CanMove>();
        if (TryGetComponent(out Character character))
        {
            character.Stop_Acting.AddListener(StopActing);
            character.Stun_Begin.AddListener(StopActing);
            character.Stun_End.AddListener(ResumeActing);
        }
        if (TryGetComponent(out PlayerController player))
        {
            player.Resume_Acting.AddListener(ResumeActing);

            if (isClient)
            {
                player.EnemyClickedAddListener(CmdTargetAcquired);
                player.Enemy_Lost.AddListener(CmdTargetLost);
            }
        }
        netAnim = GetComponent<NetworkAnimator>();
        entity = GetComponent<Character>();
    }
    private void Update()
    {
        if (!(isOwned || (entity is not PlayerCharacter && isServer)))
            return;
        
        if (attackSpeedTimer > 0)
            attackSpeedTimer -= Time.deltaTime;
        
        if (enemyTarget && canAct)
        {
            if (enemyTarget.GetComponent<HasHealth>().GetHealth() <= 0)
            {
                TargetLost();
                return;
            }
            if (Vector3.Distance(transform.position, enemyTarget.transform.position) <= (attackRange > moveComp.agent.stoppingDistance ? attackRange : moveComp.agent.stoppingDistance))
            {
                moveComp.Stop();
                if (attackSpeedTimer <= 0)
                {
                    if (netAnim)
                    {
                        if (!isOwned)
                            Stop_Acting.Invoke();
                        Attack();
                    }
                    else
                    {
                        switch (attackType)
                        {
                            case AttackType.Melee:
                                MeleeAttack();
                                break;
                            case AttackType.Ranged:
                                RangedAttack();
                                break;
                            default:
                                break;
                        }
                    }
                    if (!isServer)
                        StopActing();
                    else
                        RpcSetCanAct(false);
                }
                if (moveComp && entity is EnemyCharacter)
                    moveComp.Stop();
            }
            else if (moveComp)
                moveComp.MoveTo(enemyTarget.transform.position);
        }
    }
    [ClientRpc]
    private void RpcSetCanAct(bool value)
    {
        canAct = false;
    }
    public void Attack()
    {
        if (attackSpeedTimer > 0 || !canAct)
            return;
        StopActing();
        int random = Random.Range(0, 4);
        if (random == 0)
            netAnim.SetTrigger(animHash_Attack1);
        if (random == 1)
            netAnim.SetTrigger(animHash_Attack2);
        if (random == 2)
            netAnim.SetTrigger(animHash_Attack3);
        if (random == 3)
            netAnim.SetTrigger(animHash_Attack4);
    }
    public void MeleeAttack()                           //This reacts to animations, that are run on both the server and client
    {
        ResumeActing();
        float modifier = 1;
        var random = Random.Range(0f, 100f);
        if (random < criticalChance)
            modifier = 1 + criticalDamage;
        Resume_Acting.Invoke();
        enemyTarget.GetComponent<HasHealth>().TakeDamage(power * modifier, false, GetComponent<NetworkIdentity>());
        Attacked();
    }
    public void RangedAttack()                          //This reacts to animations, that are run on both the server and client
    {
        SpawnProjectile();
        Resume_Acting.Invoke();
        Attacked();
        ResumeActing();
    }
    private void Attacked()
    {
        attackSpeedTimer = 5 / attackSpeed;
        Has_Attacked.Invoke();
    }
    public float GetAttackCooldown()
    {
        return 5 / attackSpeed;
    }
    private void SpawnProjectile()
    {
        float modifier = 1;
        var random = Random.Range(0f, 100f);
        if (random < criticalChance)
            modifier = 1 + criticalDamage;
        GameObject projectile = Instantiate(projectilePrefab.gameObject, projectileLaunchPoint.position, Quaternion.identity);
        projectile.GetComponent<Projectile>().InitializeProjectile(new ProjectileData() 
        { 
            effectValue = power * modifier, 
            projectileTravel = ProjectileTravelType.EntityTargeted, 
            projectileImpact = ProjectileImpactType.Single, 
            impactEffect = ProjectileImpactEffect.Damage, 
            speed = 10, 
            owner = GetComponent<Entity>(),
            targetedEntity = enemyTarget,
        });
    }
    private void StopActing()
    {
        canAct = false;
    }
    private void ResumeActing()
    {
        canAct = true;
    }
    [Command(requiresAuthority = false)]
    public void CmdTargetAcquired(NetworkIdentity target)
    {
        RpcTargetAcquired(target);
    }
    [ClientRpc]
    private void RpcTargetAcquired(NetworkIdentity target)
    {
        TargetAcquired(target);
    }
    public void TargetAcquired(NetworkIdentity target)
    {
        enemyTarget = target.GetComponent<HasHealth>();
        enemyTarget.On_Death.AddListener(CmdTargetLost);
        Target_Acquired.Invoke(target);
        float temp = enemyTarget.GetComponent<Collider>().bounds.size.magnitude / 2;
        if (temp > attackRange)
            moveComp.agent.stoppingDistance = temp;
        else
            moveComp.agent.stoppingDistance = attackRange;
    }     
    [Command(requiresAuthority = false)]
    public void CmdTargetLost()
    {
        RpcTargetLost();
    }
    [ClientRpc]
    private void RpcTargetLost()
    {
        TargetLost();
    }
    public void TargetLost()
    {
        if (!canAct)
        {
            return;
        }
        if (enemyTarget)
            enemyTarget.On_Death.RemoveListener(TargetLost);
        enemyTarget = null;
        Target_Lost.Invoke();
        moveComp.agent.stoppingDistance = 0;
    }
    public float GetPower()
    {
        return power;
    }
    public float GetCritChance()
    {
        return criticalChance;
    }
    public float GetCritDamage()
    {
        return criticalDamage;
    }
    public float GetAttackSpeed()
    {
        return attackSpeed;
    }
    public float GetAttackRange()
    {
        return attackRange;
    }
    public float GetCooldownReduction()
    {
        return cooldownReduction;
    }
    [Command(requiresAuthority = false)]
    public void CmdChangePower(float value)
    {
        RpcChangePower(value);
    }
    [ClientRpc]
    public void RpcChangePower(float value)
    {
        ChangePower(value);
    }
    public void ChangePower(float value)
    {
        power += value;
        Power_Changed.Invoke(power);
    }
    public void SetPower(float value)
    {
        power = value;
        Power_Changed.Invoke(power);
    }
    [Command(requiresAuthority = false)]
    public void CmdChangeCriticalChance(float value)
    {
        RpcChangeCriticalChance(value);
    }
    [ClientRpc]
    public void RpcChangeCriticalChance(float value)
    {
        ChangeCriticalChance(value);
    }
    public void ChangeCriticalChance(float value)
    {
        criticalChance += value;
        if (criticalChance > 100)
            criticalChance = 100;
        Critical_Chance_Changed.Invoke(criticalChance);
    }
    public void SetCriticalChance(float value)
    {
        criticalChance = value;
        Critical_Chance_Changed.Invoke(criticalChance);
    }
    [Command(requiresAuthority = false)]
    public void CmdChangeCriticalDamage(float value)
    {
        RpcChangeCriticalDamage(value);
    }
    [ClientRpc]
    public void RpcChangeCriticalDamage(float value)
    {
        ChangeCriticalDamage(value);
    }
    public void ChangeCriticalDamage(float value)
    {
        criticalDamage += value;
        Critical_Damage_Changed.Invoke(criticalDamage);
    }
    public void SetCriticalDamage(float value)
    {
        criticalDamage = value;
        Critical_Damage_Changed.Invoke(criticalDamage);
    }
    [Command(requiresAuthority = false)]
    public void CmdChangeAttackSpeed(float value)
    {
        RpcChangeAttackSpeed(value);
    }
    [ClientRpc]
    public void RpcChangeAttackSpeed(float value)
    {
        ChangeAttackSpeed(value);
    }
    public void ChangeAttackSpeed(float value)
    {
        attackSpeed += value;
        Attack_Speed_Changed.Invoke(attackSpeed);
    }
    public void SetAttackSpeed(float value)
    {
        attackSpeed = value;
        Attack_Speed_Changed.Invoke(attackSpeed);
    }
    [Command(requiresAuthority = false)]
    public void CmdChangeAttackRange(float value)
    {
        RpcChangeAttackRange(value);
    }
    [ClientRpc]
    public void RpcChangeAttackRange(float value)
    {
        ChangeAttackRange(value);
    }
    public void ChangeAttackRange(float value)
    {
        attackRange += value;
        Attack_Range_Changed.Invoke(attackRange);
    }
    public void SetAttackRange(float value)
    {
        attackRange = value;
        Attack_Range_Changed.Invoke(attackRange);
    }
    [Command(requiresAuthority = false)]
    public void CmdChangeCooldownReduction(float value)
    {
        RpcChangeCooldownReduction(value);
    }
    [ClientRpc]
    public void RpcChangeCooldownReduction(float value)
    {
        ChangeCooldownReduction(value);
    }
    public void ChangeCooldownReduction(float value)
    {
        cooldownReduction += value;
        Cooldown_Reduction_Changed.Invoke(cooldownReduction);
    }
    public void SetCooldownReduction(float value)
    {
        cooldownReduction = value;
        Cooldown_Reduction_Changed.Invoke(cooldownReduction);
    }
    public float GetCooldownReductionModifier()
    {
        return 1 - (cooldownReduction / 100);
    }
}
