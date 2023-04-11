using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.AI;
using UnityEngine.Events;
public class Entity : NetworkBehaviour
{
    protected NetworkAnimator animator;

    protected int animHash_Death = Animator.StringToHash("Death");

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
        hudCircle = transform.Find("HUDCircle").gameObject;
    }
    private void OnMouseOver()
    {
        hudCircle.SetActive(true);
        Debug.Log("Entered");
    }
    private void OnMouseExit()
    {
        hudCircle.SetActive(false);
        Debug.Log("Exited");
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
}
