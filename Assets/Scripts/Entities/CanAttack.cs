using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Mirror;
using FMODUnity;

public enum AttackType
{
    Melee, Ranged
}
public class CanAttack : NetworkBehaviour, IUsesAnimator
{
    [SerializeField] private float power;
    [SerializeField] private float criticalChance = 0f;
    [SerializeField] private float criticalDamage = 0f;
    private float finalAttackSpeed;
    [SerializeField] private float baseAttackSpeed = 0;
    private float bonusAttackSpeed = 0;
    public float attackSpeedTimer = 0;
    [SerializeField] private float attackRange;
    [SerializeField] private float cooldownReduction = 0;
    [SerializeField] private AttackType attackType;
    public bool canAct = true;
    private bool isDelayingTargetLost = false;

    public GameObject projectilePrefab;
    public Transform projectileLaunchPoint;

    public HasHealth enemyTarget;
    private CanMove moveComp;
    private NetworkAnimator netAnim;
    private Entity entity;

    public EventReference attackSound;

    [System.NonSerialized] public UnityEvent<NetworkIdentity> Target_Acquired = new();
    [System.NonSerialized] public UnityEvent Target_Lost = new();
    [System.NonSerialized] public UnityEvent<float> Power_Changed = new();
    [System.NonSerialized] public UnityEvent<float> Critical_Chance_Changed = new();
    [System.NonSerialized] public UnityEvent<float> Critical_Damage_Changed = new();
    [System.NonSerialized] public UnityEvent<float> Attack_Speed_Changed = new();
    [System.NonSerialized] public UnityEvent<float> Attack_Range_Changed = new();
    [System.NonSerialized] public UnityEvent<float> Cooldown_Reduction_Changed = new();
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
        }
        moveComp = GetComponent<CanMove>();
        if (TryGetComponent(out Character character))
        {
            character.Stop_Acting.AddListener(StopActing);
            character.Stun_Begin.AddListener(StopActing);
            character.Stun_End.AddListener(ResumeActing);
        }
        if (TryGetComponent(out HasHealth hpComp))
        {
            hpComp.On_Death.AddListener(TargetLost);
        }
        if (TryGetComponent(out PlayerController player))
        {
            player.Resume_Acting.AddListener(ResumeActing);

            if (isClient)
            {
                player.Enemy_Clicked.AddListener(CmdTargetAcquired);
            }
        }
        netAnim = GetComponent<NetworkAnimator>();
        entity = GetComponent<Character>();
        SetBaseAttackSpeed(baseAttackSpeed);
    }
    private void Update()
    {
        if (!isServer)
            return;

        if (attackSpeedTimer > 0)
            attackSpeedTimer -= Time.deltaTime;

        if (enemyTarget && canAct && !isDelayingTargetLost)
        {
            if (!moveComp.agent.isOnNavMesh)
            {
                TargetLost();
                return;
            }
            if (enemyTarget.GetComponent<HasHealth>().GetHealth() <= 0)
            {
                TargetLost();
                return;
            }
            if (enemyTarget.TryGetComponent(out PlayerController player))
            {
                if (player.state == PlayerState.OutOfGame)
                {
                    TargetLost();
                    return;
                }
            }
            Debug.Log("Have Target");
            if (Vector3.Distance(transform.position, enemyTarget.transform.position) <= (attackRange > moveComp.agent.stoppingDistance ? attackRange : moveComp.agent.stoppingDistance))
            {
                Debug.Log("Stopping");
                moveComp.Stop();
                if (attackSpeedTimer <= 0)
                {
                    if (netAnim)
                    {
                        Debug.Log("Attacking");
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
                    RpcSetCanAct(false);
                }
                if (moveComp && entity is EnemyCharacter)
                    moveComp.Stop();
            }
            else if (moveComp)
                moveComp.RpcMoveTo(enemyTarget.transform.position);
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
        RpcSetCanAct(false);
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
        if (!isServer)
            return;
        if (!attackSound.IsNull)
            FindObjectOfType<AudioManager>().PlayOneShot(attackSound, transform.position);
        float modifier = 1;
        var random = Random.Range(0f, 100f);
        if (random < criticalChance)
            modifier = 1 + (criticalDamage / 100);
        if (enemyTarget)
            enemyTarget.GetComponent<HasHealth>().RpcTakeDamage(power * modifier, false, GetComponent<NetworkIdentity>());
        Attacked();
        RpcSetCanAct(true);
    }
    public void RangedAttack()                          //This reacts to animations, that are run on both the server and client
    {
        if (!isServer)
            return;
        if (enemyTarget)
            SpawnProjectile();
        Attacked();
        RpcSetCanAct(true);
    }
    private void Attacked()
    {
        attackSpeedTimer = 5 / finalAttackSpeed;
        Has_Attacked.Invoke();
    }
    public float GetAttackCooldown()
    {
        return 5 / finalAttackSpeed;
    }
    private void SpawnProjectile()
    {
        float modifier = 1;
        var random = Random.Range(0f, 100f);
        if (random < criticalChance)
            modifier = 1 + (criticalDamage / 100);
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
        NetworkServer.Spawn(projectile);
    }
    public void StopActing()
    {
        Stop_Acting.Invoke();
        canAct = false;
    }
    public void ResumeActing()
    {
        Resume_Acting.Invoke();
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
        if (!enemyTarget)
            if (target.TryGetComponent(out Pet pet))
                enemyTarget = pet.petOwner.GetComponent<HasHealth>();

        if (enemyTarget)
        {
            if (TryGetComponent(out HasHealth hp))
                enemyTarget.Target_Received.Invoke(hp);
            enemyTarget.On_Death.AddListener(CmdTargetLost);
            Target_Acquired.Invoke(target);
            float temp = enemyTarget.GetComponent<Collider>().bounds.size.magnitude / 2;
            if (moveComp)
            {
                if (temp > attackRange)
                    moveComp.agent.stoppingDistance = temp;
                else
                    moveComp.agent.stoppingDistance = attackRange;
            }
        }
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
            StopCoroutine("DelayedTargetLost");
            StartCoroutine("DelayedTargetLost");
            return;
        }
        if (enemyTarget)
        {
            enemyTarget.On_Death.RemoveListener(TargetLost);
            enemyTarget.Received_Target_Lost.Invoke(GetComponent<HasHealth>());
        }
        enemyTarget = null;
        Target_Lost.Invoke();
        if (moveComp)
            moveComp.agent.stoppingDistance = 0;
    }
    private IEnumerator DelayedTargetLost()
    {
        isDelayingTargetLost = true;
        while (!canAct)
            yield return null;
        TargetLost();
        isDelayingTargetLost = false;
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
        return finalAttackSpeed;
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
    public void CmdChangeBonusAttackSpeed(float value)
    {
        RpcChangeBonusAttackSpeed(value);
    }
    [ClientRpc]
    public void RpcChangeBonusAttackSpeed(float value)
    {
        ChangeBonusAttackSpeed(value);
    }
    public void ChangeBonusAttackSpeed(float value)
    {
        bonusAttackSpeed += value;
        finalAttackSpeed = baseAttackSpeed * (1 + bonusAttackSpeed);
        Attack_Speed_Changed.Invoke(finalAttackSpeed);
    }
    public void SetBaseAttackSpeed(float value)
    {
        baseAttackSpeed = value;
        finalAttackSpeed = baseAttackSpeed * (1 + bonusAttackSpeed);
        Attack_Speed_Changed.Invoke(finalAttackSpeed);
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

    public void SetNewAnimator(Animator animator)
    {
        netAnim.animator = animator;
    }
}
