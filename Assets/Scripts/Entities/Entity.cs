using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.AI;
using UnityEngine.Events;
public class Entity : NetworkBehaviour
{
    protected NetworkAnimator animator;
    protected List<Buff> buffs = new();
    public List<Skill> skills = new();
    public LayerMask enemyLayers;
    public LayerMask allyLayers;
    public VFXDatabase vfxDatabase;

    protected int animHash_Death = Animator.StringToHash("Death");

    public UnityEvent Stun_Begin = new();
    public UnityEvent Stun_End = new();

    public GameObject hudCircle;
    protected virtual void Start()
    {
        if (TryGetComponent(out HasHealth hp))
        {
            if (isServer)
                hp.On_Death.AddListener(RpcOnDeath);
            else if (isOwned)
                hp.On_Death.AddListener(CmdOnDeath);
        }
        animator = GetComponent<NetworkAnimator>();
        if (isOwned)
        {
            foreach (var item in skills)
            {
                item.ExecuteOnStart(this);
            }
        }
        hudCircle = transform.Find("HUDCircle").gameObject;
    }
    private void OnMouseOver()
    {
        hudCircle.SetActive(true);
    }
    private void OnMouseExit()
    {
        hudCircle.SetActive(false);
    }

    [Command(requiresAuthority = false)]
    protected virtual void CmdOnDeath()
    {
        RpcOnDeath();
        OnDeath();
    }
    [ClientRpc]
    protected virtual void RpcOnDeath()
    {
        OnDeath();
    }
    protected virtual void OnDeath()
    {
        if (animator)
        {
            animator.SetTrigger(animHash_Death);
            foreach (var item in GetComponentsInChildren<MonoBehaviour>())
            {
                item.enabled = false;
            }
            GetComponent<Collider>().enabled = false;
            GetComponent<NavMeshAgent>().enabled = false;
        }
        else
            gameObject.SetActive(false);
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
                if (isServer)
                    Debug.Log("Server");
                else
                    Debug.Log("Client");
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
    public void CmdStunEntity()
    {
        RpcStunEntity();
    }
    [ClientRpc]
    public void RpcStunEntity()
    {
        StunEntity();
    }
    public void StunEntity()
    {
        Stun_Begin.Invoke();
    }
    [Command(requiresAuthority = false)]
    public void CmdUnstunEntity()
    {
        RpcUnstunEntity();
    }
    [ClientRpc]
    public void RpcUnstunEntity()
    {
        UnstunEntity();
    }
    public void UnstunEntity()
    {
        Stun_End.Invoke();
    }
}
