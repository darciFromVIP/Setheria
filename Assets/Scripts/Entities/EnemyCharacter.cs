using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;
[System.Serializable]
public class EnemyCharacter : Character, ISaveable
{
    [SerializeField] private int xpGranted;
    [SerializeField] private LayerMask xpReceiverMask;
    [Tooltip("After chasing a player beyond this range, the enemy returns to its camp ignoring the player.")]
    public float campRadius;
    [Tooltip("This is for saving and loading purposes. Use the same name as the prefab name.")]
    public string nameOfEnemy;
    protected Vector3 startingPosition;

    protected CanAttack attackComp;
    protected CanMove moveComp;
    protected HasAggro aggroComp;
    protected HasHealth hpComp;

    public EventScriptable Enemy_Death;
    public UnityEvent Returned_To_Camp = new();
    protected override void Start()
    {
        base.Start();
        startingPosition = transform.position;
        aggroComp = GetComponent<HasAggro>();
        hpComp = GetComponent<HasHealth>();
        moveComp = GetComponent<CanMove>();
        attackComp = GetComponent<CanAttack>();
        attackComp.Target_Acquired.AddListener(TargetFound);
        attackComp.Target_Lost.AddListener(TargetLost);
    }
    protected void TargetFound(NetworkIdentity target)
    {
        StartCoroutine("CheckCampRadius");
    }
    protected void TargetLost()
    {
        if (isServer)
            ReturnToCamp(false);
        StopCoroutine("CheckCampRadius");
    }
    protected IEnumerator CheckCampRadius()
    {
        if (!isServer)
            yield break;
        while (Vector3.Distance(startingPosition, transform.position) < campRadius)
            yield return null;
        ReturnToCamp(true);
    }
    [ClientRpc]
    private void ReturnToCamp(bool loseTarget)
    {
        aggroComp.enabled = false;
        if (loseTarget)
            attackComp.TargetLost();
        if (moveComp)
            moveComp.MoveTo(startingPosition);
        hpComp.HealDamage(10000000, true);
        hpComp.SetInvulnerability(true);
        StartCoroutine(CheckCampReturn());
    }
    protected IEnumerator CheckCampReturn()
    {
        if (moveComp)
            while (!moveComp.HasReachedDestination())
                yield return null;
        Returned_To_Camp.Invoke();
        hpComp.SetInvulnerability(false);
        aggroComp.enabled = true;
    }
    protected override void OnDeath()
    {
        base.OnDeath();
        if (isServer)
        {
            Collider[] targets = new Collider[4];
            Physics.OverlapSphereNonAlloc(transform.position, 20, targets, xpReceiverMask);
            List<PlayerCharacter> xpReceivers = new();
            foreach (var item in targets)
            {
                if (item)
                    if (item.TryGetComponent(out PlayerCharacter player))
                        xpReceivers.Add(player);
            }
            foreach (var item in xpReceivers)
            {
                item.RpcAddXp(xpGranted / xpReceivers.Count);
            }
            if (Enemy_Death)
                Enemy_Death.voidEvent.Invoke();
        }
    }
    public SaveDataWorldObject SaveState()
    {
        if (hpComp.GetHealth() <= 0)
            return null;
        return new SaveDataWorldObject
        {
            positionX = transform.position.x,
            positionY = transform.position.y,
            positionZ = transform.position.z,
            floatData1 = GetComponent<HasHealth>().GetHealth(),
            name = nameOfEnemy
        };
    }

    public void LoadState(SaveDataWorldObject state)
    {
        if (state == null)
            if (isServer)
            {
                NetworkServer.Destroy(gameObject);
                return;
            }
            else
                return;
        if (!isServer)
            GetComponent<NetworkTransformUnreliable>().CmdTeleport(new Vector3(state.positionX, state.positionY, state.positionZ));
        else
            GetComponent<NetworkTransformUnreliable>().RpcTeleport(new Vector3(state.positionX, state.positionY, state.positionZ));
        GetComponent<HasHealth>().SetHealth(state.floatData1);
    }
}
