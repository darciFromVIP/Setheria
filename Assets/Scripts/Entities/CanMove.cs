using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mirror;
using UnityEngine.Events;
using Unity.VisualScripting;

[RequireComponent(typeof(NavMeshAgent), typeof(NetworkTransform))]
public class CanMove : NetworkBehaviour, IUsesAnimator
{
    [HideInInspector] public NavMeshAgent agent;
    public float baseMovementSpeed;
    private float bonusMovementSpeed = 1;
    private Entity entity;
    private Animator animator;

    private bool stunned;
    private Vector3 startingLocation;

    protected int animHash_Walk = Animator.StringToHash("Walk");
    protected int animHash_Run = Animator.StringToHash("Run");

    public UnityEvent Moved_Within_Range = new();
    private void Awake()
    {
        entity = GetComponent<Entity>();
        agent = GetComponent<NavMeshAgent>();
        if (!animator)
            animator = GetComponentInChildren<Animator>();
        agent.speed = baseMovementSpeed;

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
        startingLocation = transform.position;
    }
    private void Update()
    {
        if (baseMovementSpeed == 0)
            transform.position = startingLocation;
        if (animator)
            animator.SetFloat("AgentVelocity", agent.velocity.magnitude);
    }
    [Command(requiresAuthority = false)]
    public void CmdMoveTo(Vector3 destination)
    {
        RpcMoveTo(destination);
    }
    [ClientRpc]
    public void RpcMoveTo(Vector3 destination)
    {
        MoveTo(destination);
    }
    public void MoveTo(Vector3 destination)
    {
        if (agent.isOnNavMesh)
        {
            NavMeshPath path = new();
            NavMesh.CalculatePath(transform.position, destination, agent.areaMask, path);
            if (path.status == NavMeshPathStatus.PathComplete)
                agent.path = path;
            else
            {
                Vector3 tempVector = destination;
                do
                {
                    tempVector = Vector3.MoveTowards(tempVector, transform.position, 1);
                    NavMesh.CalculatePath(transform.position, tempVector, agent.areaMask, path);
                } while (path.corners.Length <= 0);
                agent.path = path;
            }
        }
    }

    private IEnumerator CheckPathEnd()
    {
        yield return new WaitForSeconds(1f);
        while (Vector3.Distance(transform.position, agent.pathEndPosition) > 0.1f)
        {
            yield return null;
        }
        animator.SetBool(animHash_Run, false);
        animator.SetBool(animHash_Walk, false);
    }
    [Command(requiresAuthority = false)]
    public void CmdStop()
    {
        RpcStop();
    }
    [ClientRpc]
    public void RpcStop()
    {
        Stop();
    }
    public void Stop()
    {
        if (agent.isOnNavMesh)
        {
            agent.destination = transform.position;
            animator.SetFloat("AgentVelocity", 0);
        }
    }
    public void ResumeAgent()
    {
        if (agent.enabled && !stunned)
            agent.isStopped = false;
    }
    public void StopAgent()
    {
        if (agent.isOnNavMesh)
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
    public void MoveWithinRangeEnemy(Transform target, float range)
    {
        StartCoroutine(MoveWithinRangeCoroEnemy(target, range));
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
    private IEnumerator MoveWithinRangeCoroEnemy(Transform target, float range)
    {
        while (true)
        {
            MoveTo(target.position);
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
