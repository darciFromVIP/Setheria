using FMODUnity;
using Mirror;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public enum AttackType
{
    Melee, Ranged
}
public class CanAttack : NetworkBehaviour, IUsesAnimator
{
    [SerializeField] private float basePower;
    [SerializeField] private float gearPower = 0;
    [SerializeField] private float finalPower;
    [SerializeField] private float powerMultiplier = 1;
    [SerializeField] private float powerScaling = 1;
    [SerializeField] private float baseCriticalChance = 0f;
    private float gearCriticalChance = 0;
    private float finalCriticalChance;
    [SerializeField] private float baseCriticalDamage = 0f;
    private float gearCriticalDamage = 0;
    private float finalCriticalDamage;
    [SerializeField] private float baseAttackSpeed = 0;
    [SerializeField] private float defaultAttackSpeed = 0;
    [SerializeField] private float attackSpeedMultiplier = 0;
    private float finalAttackSpeed;
    public float attackSpeedTimer = 0;
    [SerializeField] private float attackRange;
    private float additionalRange = 0;
    [SerializeField] private float baseCooldownReduction = 0;
    private float gearCooldownReduction = 0;
    private float finalCooldownReduction;
    [SerializeField] private AttackType attackType;
    public bool canAct = true;
    private bool isDelayingTargetLost = false;
    private bool isStunned = false;
    public bool isCasting = false;
    public bool flyingUnit = false;
    public float cooldownOnTargetAcquired = 3;
    private float timerOnTargetAcquired = 0;

    public GameObject projectilePrefab;
    public Transform projectileLaunchPoint;

    public HasHealth enemyTarget;
    private CanMove moveComp;
    private NetworkAnimator netAnim;
    private Entity entity;
    private Character character;
    private int currentSkillIndex = 0;

    public EventReference attackSound;

    [System.NonSerialized] public UnityEvent<NetworkIdentity> Target_Acquired = new();
    [System.NonSerialized] public UnityEvent Target_Lost = new();
    [System.NonSerialized] public UnityEvent<float, float, float, float> Power_Changed = new();
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
        if (TryGetComponent(out HasHealth hpComp))
        {
            hpComp.On_Death.AddListener(TargetLost);
        }
        if (TryGetComponent(out PlayerController player))
        {
            player.Resume_Acting.AddListener(CmdResumeActing);

            if (isClient)
            {
                player.Enemy_Clicked.AddListener(CmdTargetAcquired);
            }
        }
        character = GetComponent<Character>();
        if (character)
        {
            character.Resume_Acting.AddListener(CmdResumeActing);
            character.Stop_Acting.AddListener(CmdStopActing);
            character.Stun_Begin.AddListener(CmdStunBegin);
            character.Stun_End.AddListener(CmdStunEnd);
        }
        netAnim = GetComponent<NetworkAnimator>();
        entity = GetComponent<Character>();
        SetBaseAttackSpeed(baseAttackSpeed);
        SetDefaultAttackSpeed(baseAttackSpeed);
        SetPower(basePower);
        currentSkillIndex = 0;
    }
    private void Update()
    {
        if (!isServer)
            return;

        if (attackSpeedTimer > 0)
            attackSpeedTimer -= Time.deltaTime;
        if (timerOnTargetAcquired > 0)
            timerOnTargetAcquired -= Time.deltaTime;

        if (!moveComp && enemyTarget)
        {
            if (Vector3.Distance(enemyTarget.transform.position, transform.position) > attackRange)
                TargetLost();
        }

        if (enemyTarget && canAct && !isCasting && !isDelayingTargetLost && !isStunned)
        {
            if (moveComp)
            {
                if (!moveComp.agent.isOnNavMesh)
                {
                    TargetLost();
                    return;
                }
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
            float stoppingDistance = 0;
            if (moveComp)
            {
                stoppingDistance = moveComp.agent.stoppingDistance;
            }
            if (Vector3.Distance(transform.position, enemyTarget.transform.position) <= (attackRange > stoppingDistance ? attackRange : stoppingDistance) + additionalRange)
            {
                if (moveComp)
                    moveComp.Stop();
                if (attackSpeedTimer <= 0)
                {
                    if (netAnim)
                    {
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
                }
                if (moveComp && entity is EnemyCharacter)
                    moveComp.Stop();
            }
            else if (moveComp)
                moveComp.RpcMoveTo(enemyTarget.transform.position);

            if (!character.IsAnyCooldownTicking() && character.canCastSkills && timerOnTargetAcquired <= 0)
            {
                netAnim.animator.speed = 1;
                character.skillInstances[currentSkillIndex].Execute(character);

                character.canCastSkills = false;
                currentSkillIndex++;
                if (currentSkillIndex >= character.skillInstances.Count)
                    currentSkillIndex = 0;
            }
        }
    }
    [ClientRpc]
    private void RpcSetCanAct(bool value)
    {
        canAct = value;
    }
    [Command(requiresAuthority = false)]
    public void CmdSetCasting(bool value)
    {
        isCasting = value;
    }
    public void Attack()
    {
        if (attackSpeedTimer > 0 || !canAct || isCasting)
            return;
        attackSpeedTimer = 1;                           //This is a fix against multiattacks
        canAct = false;                                 //RPC is too slow so we're doing it again on the server
        RpcSetCanAct(false);
        if (netAnim.clientAuthority)
            ClientAttack(connectionToClient);
        else
            AttackAnimation();
    }
    [TargetRpc]
    private void ClientAttack(NetworkConnection conn)
    {
        AttackAnimation();
    }
    private void AttackAnimation()
    {
        if (isServer)
            RpcSetAnimationSpeed(1.5f / GetFinalAttackSpeed());
        else
            CmdSetAnimationSpeed(1.5f / GetFinalAttackSpeed());
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
    [Command(requiresAuthority = false)]
    private void CmdSetAnimationSpeed(float speed)
    {
        RpcSetAnimationSpeed(speed);
    }
    [ClientRpc]
    private void RpcSetAnimationSpeed(float speed)
    {
        netAnim.animator.speed = speed;
    }
    public void MeleeAttack()                           //This reacts to animations, that are run on both the server and client
    {
        if (!attackSound.IsNull)
            FindObjectOfType<AudioManager>().PlayOneShot(attackSound, transform.position);
        if (!isServer)
            return;
        float modifier = 1;
        var random = Random.Range(0f, 100f);
        if (random < GetFinalCritChance())
            modifier = 1 + (GetFinalCritDamage() / 100);
        if (enemyTarget)
            enemyTarget.GetComponent<HasHealth>().RpcTakeDamage(GetFinalPower() * modifier * GetPowerScaling(), false, GetComponent<NetworkIdentity>(), modifier > 1 ? true : false, true, false);
        Attacked();
        RpcSetCanAct(true);
    }
    public void RangedAttack()                          //This reacts to animations, that are run on both the server and client
    {
        if (!isServer)
            return;
        if (enemyTarget)
        {
            SpawnProjectile();
            Attacked();
        }
        RpcSetCanAct(true);
    }
    private void Attacked()
    {
        attackSpeedTimer = finalAttackSpeed;
        RpcSetAnimationSpeed(1);
        Has_Attacked.Invoke();
    }
    public float GetAttackCooldown()
    {
        return finalAttackSpeed;
    }
    private void SpawnProjectile()
    {
        GameObject projectile = Instantiate(projectilePrefab.gameObject, projectileLaunchPoint.position, Quaternion.identity);
        NetworkServer.Spawn(projectile);
        StartCoroutine(WaitForProjectileLoad(projectile.GetComponent<NetworkIdentity>()));
    }
    private IEnumerator WaitForProjectileLoad(NetworkIdentity proj)
    {
        while (!(proj.isClient || proj.isServer))
        {
            yield return null;
        }
        RpcInitializeProjectile(proj);
    }
    [ClientRpc]
    private void RpcInitializeProjectile(NetworkIdentity proj)
    {
        float modifier = 1;
        var random = Random.Range(0f, 100f);
        if (random < GetFinalCritChance())
            modifier = 1 + (GetFinalCritDamage() / 100);
        Entity owner;
        if (TryGetComponent(out Pet pet))
            owner = pet.petOwner.GetComponent<Entity>();
        else
            owner = GetComponent<Entity>();
        proj.GetComponent<Projectile>().InitializeProjectile(new ProjectileData()
        {
            effectValue = GetFinalPower() * modifier * GetPowerScaling(),
            projectileTravel = ProjectileTravelType.EntityTargeted,
            projectileImpact = ProjectileImpactType.Single,
            impactEffect = ProjectileImpactEffect.Damage,
            speed = 10,
            owner = owner,
            targetedEntity = enemyTarget,
            isCritical = modifier > 1 ? true : false
        });
    }
    [Command(requiresAuthority = false)]
    public void CmdStopActing()
    {
        Stop_Acting.Invoke();
        RpcSetCanAct(false);
    }
    [Command(requiresAuthority = false)]
    private void CmdStunBegin()
    {
        Stop_Acting.Invoke();
        RpcSetCanAct(false);
        isStunned = true;
    }
    [Command(requiresAuthority = false)]
    private void CmdStunEnd()
    {
        Resume_Acting.Invoke();
        RpcSetCanAct(true);
        isStunned = false;
    }
    [Command(requiresAuthority = false)]
    public void CmdResumeActing()
    {
        Resume_Acting.Invoke();
        RpcSetCanAct(true);
    }
    [Command(requiresAuthority = false)]
    public void CmdTargetAcquired(NetworkIdentity target)
    {
        RpcTargetAcquired(target);
    }
    [ClientRpc]
    public void RpcTargetAcquired(NetworkIdentity target)
    {
        TargetAcquired(target);
    }
    public void TargetAcquired(NetworkIdentity target)
    {
        if (!target)
            return;
        if (TryGetComponent(out HasHealth hasHealth))
            if (hasHealth.GetHealth() <= 0)
                return;
        enemyTarget = target.GetComponent<HasHealth>();
        if (!enemyTarget)
            if (target.TryGetComponent(out Pet pet))
            {
                if (pet.petOwner)
                {
                    var petOwner = pet.petOwner.GetComponent<HasHealth>();
                    if (petOwner)
                        enemyTarget = petOwner;
                }
            }
        if (enemyTarget)
        {
            if (TryGetComponent(out HasHealth hp))
                enemyTarget.Target_Received.Invoke(hp);
            enemyTarget.On_Death.AddListener(CmdTargetLost);
            Target_Acquired.Invoke(enemyTarget.GetComponent<NetworkIdentity>());
            float temp = 0;
            timerOnTargetAcquired = cooldownOnTargetAcquired;
            additionalRange = 0;
            if (!enemyTarget.TryGetComponent(out NavMeshObstacle obstacle))
                temp = enemyTarget.GetComponent<Collider>().bounds.size.magnitude / 2;
            else
                additionalRange = enemyTarget.GetComponent<Collider>().bounds.size.magnitude / 2;

            if (flyingUnit)
            {
                var elevationDifference = transform.position.y - enemyTarget.transform.position.y;
                if (elevationDifference > 0)
                    additionalRange += elevationDifference;
            }

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
    public float GetBasePower()
    {
        return basePower;
    }
    public float GetFinalPower()
    {
        return finalPower;
    }
    public float GetBaseCritChance()
    {
        return baseCriticalChance;
    }
    public float GetFinalCritChance()
    {
        return finalCriticalChance;
    }
    public float GetBaseCritDamage()
    {
        return baseCriticalDamage;
    }
    public float GetFinalCritDamage()
    {
        return finalCriticalDamage;
    }
    public float GetBaseAttackSpeed()
    {
        return baseAttackSpeed;
    }
    public float GetFinalAttackSpeed()
    {
        return finalAttackSpeed;
    }
    public float GetAttackRange()
    {
        return attackRange;
    }
    public float GetBaseCooldownReduction()
    {
        return baseCooldownReduction;
    }
    public float GetFinalCooldownReduction()
    {
        return finalCooldownReduction;
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
        basePower += value;
        finalPower = (basePower + gearPower) * powerMultiplier;
        if (finalPower <= 0)
            finalPower = 0;
        Power_Changed.Invoke(basePower, gearPower, finalPower, powerMultiplier);
    }
    [Command(requiresAuthority = false)]
    public void CmdChangeGearPower(float value)
    {
        RpcChangeGearPower(value);
    }
    [ClientRpc]
    public void RpcChangeGearPower(float value)
    {
        ChangeGearPower(value);
    }
    public void ChangeGearPower(float value)
    {
        gearPower += value;
        finalPower = (basePower + gearPower) * powerMultiplier;
        if (finalPower <= 0)
            finalPower = 0;
        Power_Changed.Invoke(basePower, gearPower, finalPower, powerMultiplier);
    }
    public void SetPower(float value)
    {
        basePower = value;
        finalPower = (basePower + gearPower) * powerMultiplier;
        if (finalPower <= 0)
            finalPower = 0;
        Power_Changed.Invoke(basePower, gearPower, finalPower, powerMultiplier);
    }
    [Command(requiresAuthority = false)]
    public void CmdChangePowerMultiplier(float value)
    {
        RpcChangePowerMultiplier(value);
    }
    [ClientRpc]
    public void RpcChangePowerMultiplier(float value)
    {
        ChangePowerMultiplier(value);
    }
    public void ChangePowerMultiplier(float value)
    {
        powerMultiplier += value;
        finalPower = (basePower + gearPower) * powerMultiplier;
        if (finalPower <= 0)
            finalPower = 0;
        Power_Changed.Invoke(basePower, gearPower, finalPower, powerMultiplier);
    }
    public void SetPowerMultiplier(float value)
    {
        powerMultiplier = value;
        finalPower = (basePower + gearPower) * powerMultiplier;
        if (finalPower <= 0)
            finalPower = 0;
        Power_Changed.Invoke(basePower, gearPower, finalPower, powerMultiplier);
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
        baseCriticalChance += value;
        finalCriticalChance = baseCriticalChance + gearCriticalChance;
        if (finalCriticalChance > 100)
            finalCriticalChance = 100;
        Critical_Chance_Changed.Invoke(finalCriticalChance);
    }
    [Command(requiresAuthority = false)]
    public void CmdChangeGearCriticalChance(float value)
    {
        RpcChangeGearCriticalChance(value);
    }
    [ClientRpc]
    public void RpcChangeGearCriticalChance(float value)
    {
        ChangeGearCriticalChance(value);
    }
    public void ChangeGearCriticalChance(float value)
    {
        gearCriticalChance += value;
        finalCriticalChance = baseCriticalChance + gearCriticalChance;
        if (finalCriticalChance > 100)
            finalCriticalChance = 100;
        Critical_Chance_Changed.Invoke(finalCriticalChance);
    }
    public void SetCriticalChance(float value)
    {
        baseCriticalChance = value;
        finalCriticalChance = baseCriticalChance + gearCriticalChance;
        if (finalCriticalChance > 100)
            finalCriticalChance = 100;
        Critical_Chance_Changed.Invoke(finalCriticalChance);
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
        baseCriticalDamage += value;
        finalCriticalDamage = gearCriticalDamage + baseCriticalDamage;
        Critical_Damage_Changed.Invoke(finalCriticalDamage);
    }
    [Command(requiresAuthority = false)]
    public void CmdChangeGearCriticalDamage(float value)
    {
        RpcChangeGearCriticalDamage(value);
    }
    [ClientRpc]
    public void RpcChangeGearCriticalDamage(float value)
    {
        ChangeGearCriticalDamage(value);
    }
    public void ChangeGearCriticalDamage(float value)
    {
        gearCriticalDamage += value;
        finalCriticalDamage = gearCriticalDamage + baseCriticalDamage;
        Critical_Damage_Changed.Invoke(finalCriticalDamage);
    }
    public void SetCriticalDamage(float value)
    {
        baseCriticalDamage = value;
        finalCriticalDamage = gearCriticalDamage + baseCriticalDamage;
        Critical_Damage_Changed.Invoke(finalCriticalDamage);
    }
    [Command(requiresAuthority = false)]
    public void CmdChangeAttackSpeedMultiplier(float value)
    {
        RpcChangeAttackSpeedMultiplier(value);
    }
    [ClientRpc]
    public void RpcChangeAttackSpeedMultiplier(float value)
    {
        ChangeAttackSpeedMultiplier(value);
    }
    public void ChangeAttackSpeedMultiplier(float value)
    {
        attackSpeedMultiplier += value;
        finalAttackSpeed = baseAttackSpeed * (1 - attackSpeedMultiplier);
        Attack_Speed_Changed.Invoke(finalAttackSpeed);
    }
    public void SetBaseAttackSpeed(float value)
    {
        baseAttackSpeed = value;
        finalAttackSpeed = baseAttackSpeed * (1 - attackSpeedMultiplier);
        Attack_Speed_Changed.Invoke(finalAttackSpeed);
    }
    public void SetDefaultAttackSpeed(float value)
    {
        defaultAttackSpeed = value;
    }
    public void ChangeAttackSpeedToDefault()
    {
        baseAttackSpeed = defaultAttackSpeed;
        finalAttackSpeed = baseAttackSpeed * (1 - attackSpeedMultiplier);
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
        baseCooldownReduction += value;
        finalCooldownReduction = baseCooldownReduction + gearCooldownReduction;
        Cooldown_Reduction_Changed.Invoke(finalCooldownReduction);
    }
    [Command(requiresAuthority = false)]
    public void CmdChangeGearCooldownReduction(float value)
    {
        RpcChangeGearCooldownReduction(value);
    }
    [ClientRpc]
    public void RpcChangeGearCooldownReduction(float value)
    {
        ChangeGearCooldownReduction(value);
    }
    public void ChangeGearCooldownReduction(float value)
    {
        gearCooldownReduction += value;
        finalCooldownReduction = baseCooldownReduction + gearCooldownReduction;
        Cooldown_Reduction_Changed.Invoke(finalCooldownReduction);
    }
    public void SetCooldownReduction(float value)
    {
        baseCooldownReduction = value;
        finalCooldownReduction = baseCooldownReduction + gearCooldownReduction;
        Cooldown_Reduction_Changed.Invoke(finalCooldownReduction);
    }
    public float GetCooldownReductionModifier()
    {
        return 1 - (GetFinalCooldownReduction() / 100);
    }
    [Command(requiresAuthority = false)]
    public void CmdSetPowerScaling(float value)
    {
        RpcSetPowerScaling(value);
    }
    [ClientRpc]
    public void RpcSetPowerScaling(float value)
    {
        SetPowerScaling(value);
    }
    public void SetPowerScaling(float value)
    {
        powerScaling = value;
    }
    public float GetPowerScaling()
    {
        return powerScaling;
    }
    public void SetNewAnimator(Animator animator)
    {
        netAnim.animator = animator;
    }
}
