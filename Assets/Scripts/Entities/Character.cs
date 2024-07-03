using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;
using RPG_Indicator;
using Unity.VisualScripting;
using UnityEngine.PlayerLoop;
using FMODUnity;
using System;

public class Character : Entity
{
    [SerializeField] protected float baseRotateSpeed;
    [SyncVar]
    public int level;
    protected Transform rotateTarget;

    public List<Buff> buffs = new();
    public List<Skill> skills = new();
    public List<Skill> skillInstances = new();
    public List<EventReference> skillSounds = new();
    public LayerMask enemyLayers;
    public LayerMask allyLayers;
    public BuffDatabase buffDatabase;
    public bool canCastSkills;
    public RpgIndicator skillIndicator;
    private bool isStunned;

    public float cooldown1;
    public float cooldown2;
    public float cooldown3;
    public float cooldown4;
    public float cooldown5;

    [HideInInspector] public UnityEvent Stun_Begin = new();
    [HideInInspector] public UnityEvent Stun_End = new();
    [HideInInspector] public UnityEvent Stop_Acting = new();
    [HideInInspector] public UnityEvent Resume_Acting = new();
    [HideInInspector] public UnityEvent<string, Buff> Buff_Added = new();

    protected int animHash_Skill1 = Animator.StringToHash("Skill1");
    protected int animHash_Skill2 = Animator.StringToHash("Skill2");
    protected int animHash_Skill3 = Animator.StringToHash("Skill3");
    protected int animHash_Skill4 = Animator.StringToHash("Skill4");
    protected int animHash_Skill5 = Animator.StringToHash("Skill5");
    protected int animHash_Revive = Animator.StringToHash("Revive");

    protected override void Start()
    {
        base.Start();
        skillIndicator = GetComponentInChildren<RpgIndicator>(true);
        canCastSkills = TryGetComponent(out EnemySkills enemySkills);

        if (TryGetComponent(out CanAttack attackComp))
        {
            attackComp.Target_Acquired.AddListener(RotateTargetAcquired);
            attackComp.Target_Lost.AddListener(RotateTargetLost);
        }
        foreach (var item in skills)
        {
            skillInstances.Add(item.GetInstance());
        }
        if (TryGetComponent(out Shapeshifter shapeshifter))
        {
            shapeshifter.SetDefaultSkillInstances();
        }
        if (isOwned)
        {
            foreach (var item in skillInstances)
            {
                item.ExecuteOnStart(this);
            }
        }
    }
    protected override void OnMouseEnter()
    {
        base.OnMouseEnter();
        if (!isOwned || this is Ship)
            FindObjectOfType<CharacterHoverDetail>().Show(this, false);
    }
    protected override void OnMouseExit()
    {
        base.OnMouseExit();
        if (!isOwned || this is Ship)
            FindObjectOfType<CharacterHoverDetail>().Hide(false);
    }
    protected void Update()
    {
        if (cooldown1 > 0)
            cooldown1 -= Time.deltaTime;
        if (cooldown2 > 0)
            cooldown2 -= Time.deltaTime;
        if (cooldown3 > 0)
            cooldown3 -= Time.deltaTime;
        if (cooldown4 > 0)
            cooldown4 -= Time.deltaTime;
        if (cooldown5 > 0)
            cooldown5 -= Time.deltaTime;

        if (rotateTarget)
            RotateToTarget();
    }
    protected void RotateToTarget()
    {
        if (isStunned)
            return;
        Vector3 targetDir = new Vector3(rotateTarget.position.x, 0, rotateTarget.position.z) - new Vector3(transform.position.x, 0, transform.position.z);
        float step = baseRotateSpeed * Time.deltaTime;
        Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, step, 0.0f);
        transform.rotation = Quaternion.LookRotation(newDir);
    }
    public void RotateToPoint(Vector3 point)
    {
        StartCoroutine(RotateToPointCoro(point));
    }
    private IEnumerator RotateToPointCoro(Vector3 point)
    {
        Vector3 targetDir = new Vector3(point.x, 0, point.z) - new Vector3(transform.position.x, 0, transform.position.z);
        do
        {
            float step = baseRotateSpeed * Time.deltaTime;
            Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, step, 0.0f);
            transform.rotation = Quaternion.LookRotation(newDir);
            yield return null;
        } while (Quaternion.Angle(Quaternion.LookRotation(new Vector3(transform.forward.x, 0, transform.forward.z)), Quaternion.LookRotation(targetDir)) > 10);
    }
    private void RotateTargetAcquired(NetworkIdentity target)
    {
        rotateTarget = target.transform;
    }
    public void RotateTargetLost()
    {
        rotateTarget = null;
    }
    public void CastSkill1()
    {
        animator.SetTrigger(animHash_Skill1);
        if (skillSounds.Count >= 1)
            FindObjectOfType<AudioManager>().PlayOneShot(skillSounds[0], transform.position);
        StopActing();
    }
    public void CastSkill2()
    {
        animator.SetTrigger(animHash_Skill2);
        if (skillSounds.Count >= 2)
            FindObjectOfType<AudioManager>().PlayOneShot(skillSounds[1], transform.position);
        StopActing();
    }
    public void CastSkill3()
    {
        animator.SetTrigger(animHash_Skill3);
        if (skillSounds.Count >= 3)
            FindObjectOfType<AudioManager>().PlayOneShot(skillSounds[2], transform.position);
        StopActing();
    }
    public void CastSkill4()
    {
        animator.SetTrigger(animHash_Skill4);
        if (skillSounds.Count >= 4)
            FindObjectOfType<AudioManager>().PlayOneShot(skillSounds[3], transform.position);
        StopActing();
    }
    public void CastSkill5()
    {
        animator.SetTrigger(animHash_Skill5);
        if (skillSounds.Count >= 5)
            FindObjectOfType<AudioManager>().PlayOneShot(skillSounds[4], transform.position);
        StopActing();
    }
    [Command(requiresAuthority = false)]
    private void StopActing()
    {
        Stop_Acting.Invoke();
        RpcStopActing();
    }
    [ClientRpc]
    private void RpcStopActing()
    {
        Stop_Acting.Invoke();
    }

    public void StartCooldown1()
    {
        cooldown1 = GetComponent<EnemyCharacter>().skillInstances[0].cooldown;
        Resume_Acting.Invoke();
        RpcResumeActing();
    }
    public void StartCooldown2()
    {
        cooldown2 = GetComponent<EnemyCharacter>().skillInstances[1].cooldown;
        Resume_Acting.Invoke();
        RpcResumeActing();
    }
    public void StartCooldown3()
    {
        cooldown3 = GetComponent<EnemyCharacter>().skillInstances[2].cooldown;
        Resume_Acting.Invoke();
        RpcResumeActing();
    }
    public void StartCooldown4()
    {
        cooldown4 = GetComponent<EnemyCharacter>().skillInstances[3].cooldown;
        Resume_Acting.Invoke();
        RpcResumeActing();
    }
    public void StartCooldown5()
    {
        cooldown5 = GetComponent<EnemyCharacter>().skillInstances[4].cooldown;
        Resume_Acting.Invoke();
        RpcResumeActing();
    }
    [ClientRpc]
    private void RpcResumeActing()
    {
        Resume_Acting.Invoke();
    }
    public bool IsAnyCooldownTicking()
    {
        return cooldown1 > 0 || cooldown2 > 0 || cooldown3 > 0 || cooldown4 > 0 || cooldown5 > 0;
    }
    [Command(requiresAuthority = false)]
    public void CmdAddBuff(string buff, NetworkConnectionToClient conn = null)
    {
        var buffScriptable = buffDatabase.GetBuffByName(buff);
        if (buffScriptable)
        {
            if (buffScriptable.buffType != BuffType.InventorySlots)
                RpcAddBuff(buff);
            else
                SingleClientAddBuff(conn, buff);
        }
    }
    [TargetRpc]
    public void SingleClientAddBuff(NetworkConnection conn, string buff)
    {
        AddBuff(buff);
    }
    [ClientRpc]
    public void RpcAddBuff(string buff)
    {
        AddBuff(buff);
    }
    public virtual void AddBuff(string buff)
    {
        Buff buffInstance = null;
        var buffScriptable = buffDatabase.GetBuffByName(buff);
        if (!buffScriptable)
            return;
        foreach (var item in buffs)
        {
            if (item.name == buff && buffScriptable.stackable && item.stacks < buffScriptable.maxStacks)
            {
                item.IncreaseStacks();
                return;
            }
            else if (item.name == buff && !buffScriptable.stackable)
                return;
            else if (item.name == buff && buffScriptable.stackable && item.stacks >= buffScriptable.maxStacks)
            {
                FindObjectOfType<SystemMessages>().AddMessage("This buff is at max stacks.");
                return;
            }
        }
        switch (buffScriptable.buffType)
        {
            case BuffType.Bleed:
                buffInstance = new BBleed(buffScriptable.value, this);
                break;
            case BuffType.Slow:
                buffInstance = new BSlow(buffScriptable.value, this);
                break;
            case BuffType.Stun:
                buffInstance = new BStun(this);
                break;
            case BuffType.Regen:
                buffInstance = new BRegen(buffScriptable.value, this);
                break;
            case BuffType.MaxHealth:
                buffInstance = new BMaxHealth(buffScriptable.value, this);
                break;
            case BuffType.MaxMana:
                buffInstance = new BMaxMana(buffScriptable.value, this);
                break;
            case BuffType.Fear:
                break;
            case BuffType.ManaRegen:
                buffInstance = new BManaRegen(buffScriptable.value, this);
                break;
            case BuffType.InventorySlots:
                buffInstance = new BInventorySlots(buffScriptable.value, FindObjectOfType<InventoryManager>(true));
                break;
            case BuffType.Power:
                buffInstance = new BPower(buffScriptable.value, this);
                break;
            case BuffType.CriticalChance:
                buffInstance = new BCriticalChance(buffScriptable.value, this);
                break;
            case BuffType.CriticalDamage:
                buffInstance = new BCriticalDamage(buffScriptable.value, this);
                break;
            case BuffType.AttackSpeedMultiplier:
                buffInstance = new BAttackSpeed(buffScriptable.value, this);
                break;
            case BuffType.CooldownReduction:
                buffInstance = new BCooldownReduction(buffScriptable.value, this);
                break;
            case BuffType.Armor:
                buffInstance = new BArmor(buffScriptable.value, this);
                break;
            case BuffType.AttackRange:
                buffInstance = new BAttackRange(buffScriptable.value, this);
                break;
            case BuffType.Speed:
                buffInstance = new BSpeed(buffScriptable.value, this);
                break;
            case BuffType.BaseAttackSpeed:
                buffInstance = new BBaseAttackSpeed(buffScriptable.value, this);
                break;
            case BuffType.PowerScaling:
                buffInstance = new BPowerScaling(buffScriptable.value, this);
                break;
            case BuffType.CorruptionResistance:
                buffInstance = new BCorruptionResistance(buffScriptable.value, this);
                break;
            case BuffType.Corruption:
                buffInstance = new BCorruption(buffScriptable.value, this);
                break;
            case BuffType.Sleep:
                buffInstance = new BSleep(buffScriptable.value, this);
                break;
            case BuffType.Invulnerability:
                buffInstance = new BInvulnerability(buffScriptable.value, this);
                break;
            default:
                break;
        }
        if (!buffScriptable.sound.IsNull)
            buffInstance.SetSound(FindObjectOfType<AudioManager>().CreateEventInstance(buffScriptable.sound, transform));

        if (buffInstance != null)
        {
            buffInstance.name = buffScriptable.name;
            var vfx = vfxDatabase.GetVFXByName(buffScriptable.buffName);
            if (vfx)
            {
                var effectInstance = Instantiate(vfxDatabase.GetVFXByName(buffScriptable.buffName), transform);
                buffInstance.effect = effectInstance;
            }
            buffs.Add(buffInstance);
            Buff_Added.Invoke(buffScriptable.name, buffInstance);
            if (this is PlayerCharacter && isOwned && buffScriptable.sprite != null)
            {
                FindObjectOfType<BuffListHero>().AddBuff(buffScriptable.name, buffInstance);
            }
            if (buffScriptable.duration > 0)
                StartCoroutine(buffInstance.TimedBuff(buffScriptable.duration));
        }
    }
    public void BuffExpired(GameObject effect)
    {
        if (effect)
        {
            foreach (var item in effect.GetComponentsInChildren<ParticleSystem>())
            {
                item.Stop();
            }
            if (effect.TryGetComponent(out Animator animator))
            {
                animator.SetTrigger("Death");
            }
        }
        UpdateSkills();
    }
    [Command(requiresAuthority = false)]
    public void CmdRemoveBuff(string buffName, NetworkConnectionToClient conn = null)
    {
        var buffScriptable = buffDatabase.GetBuffByName(buffName);
        if (buffScriptable)
        {
            if (buffScriptable.buffType != BuffType.InventorySlots)
                RpcRemoveBuff(buffName);
            else
                SingleClientRemoveBuff(conn, buffName);
        }
    }
    [ClientRpc]
    public void RpcRemoveBuff(string buffName)
    {
        RemoveBuff(buffName);
    }
    [TargetRpc]
    public void SingleClientRemoveBuff(NetworkConnection conn, string buffName)
    {
        RemoveBuff(buffName);
    }
    private void RemoveBuff(string buffName)
    {
        var buff = buffDatabase.GetBuffByName(buffName);
        if (buff == null)
            return;
        List<Buff> temp = new();
        buffs.CopyTo(temp);
        foreach (var item in temp)
        {
            if (item.name == buff.name)
            {
                if (item.stacks > 1)
                    item.DecreaseStacks();
                else
                {
                    item.BuffExpired();
                    buffs.Remove(item);
                }
                break;
            }
        }
        UpdateSkills();
    }
    public int HasBuff(string name)
    {
        foreach (var item in buffs)
        {
            if (item.name == name)
                return item.stacks;
        }
        return 0;
    }
    [Command(requiresAuthority = false)]
    public void CmdStunCharacter()
    {
        RpcStunCharacter();
    }
    [ClientRpc]
    public void RpcStunCharacter()
    {
        StunCharacter();
    }
    public void StunCharacter()
    {
        isStunned = true;
        Stun_Begin.Invoke();
    }
    [Command(requiresAuthority = false)]
    public void CmdUnstunCharacter()
    {
        RpcUnstunCharacter();
    }
    [ClientRpc]
    public void RpcUnstunCharacter()
    {
        UnstunCharacter();
    }
    public void UnstunCharacter()
    {
        isStunned = false;
        Stun_End.Invoke();
    }
    public void SleepCharacter()
    {
        StunCharacter();
        GetComponent<HasHealth>().Damage_Taken.AddListener(WakeupCharacter);
    }
    public void WakeupCharacter(NetworkIdentity source)
    {
        UnstunCharacter();
        RemoveBuff("Sleeping");
        GetComponent<HasHealth>().Damage_Taken.RemoveListener(WakeupCharacter);
    }
    public void WakeupCharacter()
    {
        UnstunCharacter();
        GetComponent<HasHealth>().Damage_Taken.RemoveListener(WakeupCharacter);
    }
    public void UpdateSkills()
    {
        if (TryGetComponent(out Shapeshifter shapeshifter))
        {
            foreach (var item in shapeshifter.defaultSkills)
            {
                item.SetCastingEntity(this);
            }
            foreach (var item in shapeshifter.shapeshiftedSkills)
            {
                item.SetCastingEntity(this);
            }
        }
        else
        {
            foreach (var item in skillInstances)
            {
                item.SetCastingEntity(this);
            }
        }
    }
    public void SetRotateTarget(Transform target)
    {
        rotateTarget = target;
    }
}
