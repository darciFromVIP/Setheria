using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mirror;
using UnityEngine.Events;

[RequireComponent(typeof(NavMeshAgent), typeof(NetworkTransform))]
public class CanMove : NetworkBehaviour, IUsesAnimator
{
    [HideInInspector] public NavMeshAgent agent;
    public float baseMovementSpeed;
    private float bonusMovementSpeed = 1;
    private Entity entity;
    private Animator animator;

    private bool stunned;

    protected int animHash_Walk = Animator.StringToHash("Walk");
    protected int animHash_Run = Animator.StringToHash("Run");

    public UnityEvent Moved_Within_Range = new();
    private void Start()
    {
        entity = GetComponent<Entity>();
        agent = GetComponent<NavMeshAgent>();
        agent.speed = baseMovementSpeed;
        if (!animator)
            animator = GetComponentInChildren<Animator>();
        if (TryGetComponent(out Character character))
        {
            character.Stop_Acting.AddListener(StopAgent);
            character.Stun_Begin.AddListener(StunBegin);
            character.Stun_End.AddListener(StunEnd);
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
        if (!(isOwned || (entity is not PlayerCharacter && isServer)) || baseMovementSpeed == 0)
            return;
        if (animator)
        {
            if (agent.velocity.magnitude < 0.1f)
            {
                animator.SetBool(animHash_Run, false);
                animator.SetBool(animHash_Walk, false);
            }
        }
    }
    public void MoveTo(Vector3 destination)
    {
        if (agent)
        {
            NavMeshPath path = new();
            NavMesh.CalculatePath(transform.position, destination, agent.areaMask, path);
            agent.path = path;
            StartCoroutine(DelayedAnimation());
        }
    }
    private IEnumerator DelayedAnimation()
    {
        yield return new WaitForSeconds(0.1f);
        if (agent.velocity.magnitude > 2.9f)
        {
            animator.SetBool(animHash_Run, true);
            animator.SetBool(animHash_Walk, false);
        }
        else if (agent.velocity.magnitude > 0.1f)
        {
            animator.SetBool(animHash_Run, false);
            animator.SetBool(animHash_Walk, true);
        }
    }
    public void Stop()
    {
        if (agent)
            agent.destination = transform.position;
    }
    public void ResumeAgent()
    {
        if (agent.enabled && !stunned)
            agent.isStopped = false;
    }
    public void StopAgent()
    {
        agent.isStopped = true;
    }
    private void StunBegin()
    {
        stunned = true;
        StopAgent();
    }
    private void StunEnd()
    {
        stunned = false;
        ResumeAgent();
    }
    public void SetBaseMovementSpeed(float value)
    {
        baseMovementSpeed = value;
        UpdateMovementSpeed();
    }
    public void SetBonusMovementSpeed(float value)
    {
        bonusMovementSpeed = value;
        UpdateMovementSpeed();
    }
    public void ChangeBaseMovementSpeed(float value)
    {
        baseMovementSpeed += value;
        UpdateMovementSpeed();
    }
    public void ChangeBonusMovementSpeed(float value)
    {
        bonusMovementSpeed += value;
        UpdateMovementSpeed();
    }
    private void UpdateMovementSpeed()
    {
        agent.speed = GetFinalMovementSpeed();
    }
    public float GetFinalMovementSpeed()
    {
        float finalSpeed = baseMovementSpeed * bonusMovementSpeed;
        if (finalSpeed <= 0.1f)
            finalSpeed = 0.1f;
        return finalSpeed;
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
    public void FollowTarget(Transform target)
    {
        StartCoroutine(FollowTargetCoro(target));
    }
    private IEnumerator FollowTargetCoro(Transform target)
    {
        while (Vector3.Distance(transform.position, target.position) > 1)
        {
            MoveTo(target.position);
            yield return null;
        }
    }
    public void SetNewAnimator(Animator animator)
    {
        this.animator = animator;
    }
}
