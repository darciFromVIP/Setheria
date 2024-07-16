using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.AI;
using UnityEngine.Events;
using FoW;
public class Entity : NetworkBehaviour, IUsesAnimator
{
    public NetworkAnimator animator;
    public bool corpseDecay = true;
    public List<GameObject> objectsToDisable;
    public bool changeNightVision;
    public float nightVisionRadius;
    private float dayVisionRadius;

    protected int animHash_Death = Animator.StringToHash("Death");

    public UnityEvent On_Death = new();

    public GameObject hudCircle;
    private bool hudCircleToggled = false;
    public VFXDatabase vfxDatabase;
    private FogOfWarUnit fowUnit;
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
        if (changeNightVision)
        {
            fowUnit = GetComponent<FogOfWarUnit>();
            dayVisionRadius = fowUnit.circleRadius;
            var daynight = FindObjectOfType<DayNightCycle>();
            daynight.Day_Started.AddListener(DayVision);
            daynight.Night_Started.AddListener(NightVision);
        }
    }
    protected virtual void OnMouseEnter()
    {
    }
    protected virtual void OnMouseOver()
    {
        if (hudCircle == null)
            return;
        if (hudCircleToggled)
            return;
        var bar = GetComponentInChildren<EntityStatusBar>(true);
        if (bar)
        { 
            if (bar.gameObject.activeSelf)
                hudCircle.SetActive(true);
        }
        else
            hudCircle.SetActive(true);
    }
    protected virtual void OnMouseExit()
    {
        if (hudCircle == null)
            return;
        if (hudCircleToggled)
            return;
        var bar = GetComponentInChildren<EntityStatusBar>(true);
        if (bar)
        {
            if (bar.gameObject.activeSelf)
                hudCircle.SetActive(false);
        }
        else
            hudCircle.SetActive(false);
    }
    public void ToggleHUDCircle(bool value)
    {
        hudCircleToggled = value;
        hudCircle.SetActive(value);
    }

    [Command(requiresAuthority = false)]
    public virtual void CmdOnDeath()
    {
        RpcOnDeath();
        OnDeath();
    }
    [ClientRpc]
    public virtual void RpcOnDeath()
    {
        OnDeath();
    }
    protected virtual void OnDeath()
    {
        if (animator)
        {
            foreach (var item in GetComponentsInChildren<Animator>())
            {
                item.SetTrigger(animHash_Death);
            }
            if (objectsToDisable.Count <= 0)
            {
                foreach (var item in GetComponentsInChildren<MonoBehaviour>())
                {
                    item.enabled = false;
                }
            }
            else
            {
                foreach (var item in objectsToDisable)
                {
                    item.SetActive(false);
                }
            }
            
            GetComponent<Collider>().enabled = false;
            if (TryGetComponent(out NavMeshAgent agent))
                agent.enabled = false;
            if (corpseDecay)
                StartCoroutine(CorpseDecay());
        }
        else
            gameObject.SetActive(false);
        On_Death.Invoke();
    }
    private IEnumerator CorpseDecay()
    {
        yield return new WaitForSeconds(30);
        float time = 3;
        while (time > 0)
        {
            transform.position = Vector3.MoveTowards(transform.position, transform.position + Vector3.down, Time.deltaTime);
            time -= Time.deltaTime;
            yield return null;
        }
        if (isServer)
            NetworkServer.Destroy(gameObject);
    }
    [Command(requiresAuthority = false)]
    public virtual void CmdRevive(Vector3 position, float hpPercentage)
    {
        RpcRevive(position, hpPercentage);
    }
    [ClientRpc]
    public virtual void RpcRevive(Vector3 position, float hpPercentage)
    {
        Revive(position, hpPercentage);
    }
    public virtual void Revive(Vector3 position, float hpPercentage)
    {
        transform.position = position;
        if (animator)
        {
            if (objectsToDisable.Count <= 0)
            {
                foreach (var item in GetComponentsInChildren<MonoBehaviour>())
                {
                    item.enabled = true;
                }
            }
            else
            {
                foreach (var item in objectsToDisable)
                {
                    item.SetActive(true);
                }
            }
            GetComponent<Collider>().enabled = true;
            if (TryGetComponent(out NavMeshAgent agent))
                agent.enabled = true;
        }
        if (TryGetComponent(out HasHealth hp))
        {
            hp.SetHealth(hp.GetFinalMaxHealth() * hpPercentage);
        }
        if (TryGetComponent(out HasMana mp))
        {
            mp.SetMana(mp.GetFinalMaxMana() * hpPercentage);
        }
    }
    public void SetNewAnimator(Animator animator)
    {
        this.animator.animator = animator;
    }
    [Command(requiresAuthority = false)]
    public void CmdSpawnVfx(string name)
    {
        RpcSpawnVfx(name);
    }
    private void RpcSpawnVfx(string name)
    {
        SpawnVfx(name);
    }
    private void SpawnVfx(string name)
    {
        Instantiate(vfxDatabase.GetVFXByName(name), transform);
    }
    private void NightVision()
    {
        fowUnit.circleRadius = nightVisionRadius;
    }
    private void DayVision()
    {
        fowUnit.circleRadius = dayVisionRadius;
    }
}
