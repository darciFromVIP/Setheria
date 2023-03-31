using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mirror;
using UnityEngine.Events;

[RequireComponent(typeof(NavMeshAgent), typeof(NetworkTransform))]
public class CanMove : NetworkBehaviour
{
    [HideInInspector] public NavMeshAgent agent;
    private Entity entity;
    private Animator animator;

    protected int animHash_Walk = Animator.StringToHash("Walk");
    protected int animHash_Run = Animator.StringToHash("Run");

    public UnityEvent Moved_Within_Range = new();
    private void Start()
    {
        entity = GetComponent<Entity>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        if (TryGetComponent(out Character character))
        {
            character.Stop_Acting.AddListener(StopAgent);
            character.Stun_Begin.AddListener(StopAgent);
            character.Stun_End.AddListener(ResumeAgent);
        }
        if (TryGetComponent(out CanAttack attack))
        {
            attack.Stop_Acting.AddListener(StopAgent);
            attack.Resume_Acting.AddListener(ResumeAgent);
        }
        if (TryGetComponent(out PlayerController player))
        {
            player.Resume_Acting.AddListener(ResumeAgent);
        }
    }
    private void Update()
    {
        if (!(isOwned || (entity is not PlayerCharacter && isServer)))
            return;
        if (animator)
        {
            if (agent.velocity.magnitude > 0.1f)
                animator.SetBool(animHash_Run, true);
            else
                animator.SetBool(animHash_Run, false);
        }
    }
    public void MoveTo(Vector3 destination)
    {
        if (agent)
            agent.destination = destination;
    }
    public void Stop()
    {
        if (agent)
            agent.destination = transform.position;
    }
    public void ResumeAgent()
    {
        agent.isStopped = false;
    }
    public void StopAgent()
    {
        agent.isStopped = true;
    }
    public bool HasReachedDestination()
    {
        return (Vector3.Distance(transform.position, agent.destination) < (1.5f + agent.stoppingDistance));
    }
    public void MoveWithinRange(Transform target, float range)
    {
        StartCoroutine(MoveWithinRangeCoro(target, range));
    }
    private IEnumerator MoveWithinRangeCoro(Transform target, float range)
    {
        MoveTo(target.position);
        var originDest = agent.destination;
        while (true)
        {
            if (originDest != agent.destination)
            {
                Moved_Within_Range.RemoveAllListeners();
                yield break;
            }
            if (Vector3.Distance(transform.position, target.position) <= range)
                break;
            yield return null;
        }
        Stop();
        Moved_Within_Range.Invoke();
        Moved_Within_Range.RemoveAllListeners();
    }
}
