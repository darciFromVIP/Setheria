using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;
public class Character : Entity
{
    [SerializeField] protected float baseRotateSpeed;
    protected Transform rotateTarget;

    protected List<Buff> buffs = new();
    public List<Skill> skills = new();
    public LayerMask enemyLayers;
    public LayerMask allyLayers;
    public VFXDatabase vfxDatabase;

    [HideInInspector] public UnityEvent Stun_Begin = new();
    [HideInInspector] public UnityEvent Stun_End = new();
    [HideInInspector] public UnityEvent Stop_Acting = new();

    protected int animHash_Skill1 = Animator.StringToHash("Skill1");
    protected int animHash_Skill2 = Animator.StringToHash("Skill2");
    protected int animHash_Skill3 = Animator.StringToHash("Skill3");
    protected int animHash_Skill4 = Animator.StringToHash("Skill4");
    protected int animHash_Skill5 = Animator.StringToHash("Skill5");

    protected override void Start()
    {
        base.Start();
        if (TryGetComponent(out CanAttack attackComp))
        {
            attackComp.Target_Acquired.AddListener(RotateTargetAcquired);
            attackComp.Target_Lost.AddListener(RotateTargetLost);
        }
        if (isOwned)
        {
            foreach (var item in skills)
            {
                item.ExecuteOnStart(this);
            }
        }
    }
    protected void Update()
    {
        if (rotateTarget)
            RotateToTarget();
    }
    protected void RotateToTarget()
    {
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
    private void RotateTargetLost()
    {
        rotateTarget = null;
    }
    public void CastSkill1()
    {
        animator.SetTrigger(animHash_Skill1);
        StopActing();
    }
    public void CastSkill2()
    {
        animator.SetTrigger(animHash_Skill2);
        StopActing();
    }
    public void CastSkill3()
    {
        animator.SetTrigger(animHash_Skill3);
        StopActing();
    }
    public void CastSkill4()
    {
        animator.SetTrigger(animHash_Skill4);
        StopActing();
    }
    public void CastSkill5()
    {
        animator.SetTrigger(animHash_Skill5);
        StopActing();
    }
    private void StopActing()
    {
        Stop_Acting.Invoke();
    }
    [Command(requiresAuthority = false)]
    public void CmdAddBuff(BuffScriptable buff, NetworkConnectionToClient conn = null)
    {
        if (buff.buffType != BuffType.InventorySlots)
            RpcAddBuff(buff);
        else
            SingleClientAddBuff(conn, buff);

    }
    [TargetRpc]
    public void SingleClientAddBuff(NetworkConnection conn, BuffScriptable buff)
    {
        AddBuff(buff);
    }
    [ClientRpc]
    public void RpcAddBuff(BuffScriptable buff)
    {
        AddBuff(buff);
    }
    protected virtual void AddBuff(BuffScriptable buff)
    {
        Buff buffInstance = null;
        switch (buff.buffType)
        {
            case BuffType.Bleed:
                buffInstance = new BBleed(buff.value, this);
                break;
            case BuffType.Slow:
                break;
            case BuffType.Stun:
                buffInstance = new BStun(this);
                break;
            case BuffType.Regen:
                buffInstance = new BRegen(buff.value, this);
                break;
            case BuffType.MaxHealth:
                buffInstance = new BMaxHealth(buff.value, this);
                break;
            case BuffType.MaxMana:
                buffInstance = new BMaxMana(buff.value, this);
                break;
            case BuffType.Fear:
                break;
            case BuffType.ManaRegen:
                buffInstance = new BManaRegen(buff.value, this);
                break;
            case BuffType.InventorySlots:
                buffInstance = new BInventorySlots(buff.value, FindObjectOfType<InventoryManager>());
                break;
            case BuffType.Power:
                buffInstance = new BPower(buff.value, this);
                break;
            case BuffType.CriticalChance:
                buffInstance = new BCriticalChance(buff.value, this);
                break;
            case BuffType.CriticalDamage:
                buffInstance = new BCriticalDamage(buff.value, this);
                break;
            case BuffType.AttackSpeed:
                buffInstance = new BAttackSpeed(buff.value, this);
                break;
            case BuffType.CooldownReduction:
                break;
            case BuffType.Armor:
                buffInstance = new BArmor(buff.value, this);
                break;
            case BuffType.AttackRange:
                buffInstance = new BAttackRange(buff.value, this);
                break;
            case BuffType.MovementSpeed:
                buffInstance = new BSpeed(buff.value, this);
                break;
            default:
                break;
        }
        if (buffInstance != null)
        {
            var vfx = vfxDatabase.GetVFXByName(buff.buffName);
            if (vfx)
            {
                var effectInstance = Instantiate(vfxDatabase.GetVFXByName(buff.buffName), transform);
                buffInstance.effect = effectInstance;
            }
            buffs.Add(buffInstance);
            if (buff.duration > 0)
                StartCoroutine(buffInstance.TimedBuff(buff.duration));
        }
    }
    public void BuffExpired(GameObject effect)
    {
        foreach (var item in effect.GetComponentsInChildren<ParticleSystem>())
        {
            item.Stop();
        }
    }
    [Command(requiresAuthority = false)]
    public void CmdRemoveBuff(BuffScriptable buff)
    {
        RpcRemoveBuff(buff);
    }
    [ClientRpc]
    public void RpcRemoveBuff(BuffScriptable buff)
    {
        List<Buff> temp = new();
        buffs.CopyTo(temp);
        foreach (var item in temp)
        {
            if (item.value == buff.value && item.buffType == buff.buffType)
            {
                item.BuffExpired();
                buffs.Remove(item);
            }
        }
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
        Stun_End.Invoke();
    }
}
