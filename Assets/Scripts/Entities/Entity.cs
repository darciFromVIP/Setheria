using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.AI;
using UnityEngine.Events;
public class Entity : NetworkBehaviour, IUsesAnimator
{
    public NetworkAnimator animator;

    protected int animHash_Death = Animator.StringToHash("Death");

    public UnityEvent On_Death = new();

    public GameObject hudCircle;
    public VFXDatabase vfxDatabase;
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
    }
    protected virtual void OnMouseEnter()
    {
    }
    protected virtual void OnMouseOver()
    {
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
        var bar = GetComponentInChildren<EntityStatusBar>(true);
        if (bar)
        {
            if (bar.gameObject.activeSelf)
                hudCircle.SetActive(false);
        }
        else
            hudCircle.SetActive(false);
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
            foreach (var item in GetComponentsInChildren<MonoBehaviour>())
            {
                item.enabled = false;
            }
            GetComponent<Collider>().enabled = false;
            GetComponent<NavMeshAgent>().enabled = false;
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
}
