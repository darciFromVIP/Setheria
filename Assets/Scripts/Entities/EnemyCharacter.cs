using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[System.Serializable]
public class EnemyCharacter : Character, ISaveable
{
    [SerializeField] private int xpGranted;
    [SerializeField] private LayerMask xpReceiverMask;
    [Tooltip("After chasing a player beyond this range, the enemy returns to its camp ignoring the player.")]
    public float campRadius;
    protected Vector3 startingPosition;

    protected CanAttack attackComp;
    protected CanMove moveComp;
    protected HasAggro aggroComp;
    protected HasHealth hpComp;
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
        StopCoroutine("CheckCampRadius");
    }
    protected IEnumerator CheckCampRadius()
    {
        if (!isServer)
            yield break;
        while (Vector3.Distance(startingPosition, transform.position) < campRadius)
            yield return null;
        ReturnToCamp();
    }
    [ClientRpc]
    private void ReturnToCamp()
    {
        aggroComp.enabled = false;
        attackComp.TargetLost();
        moveComp.MoveTo(startingPosition);
        hpComp.HealDamage(10000000, true);
        hpComp.SetInvulnerability(true);
        StartCoroutine(CheckCampReturn());
    }
    protected IEnumerator CheckCampReturn()
    {
        while (!moveComp.HasReachedDestination())
            yield return null;
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
        }
    }
    public SaveDataWorldObject SaveState()
    {
        return new SaveDataWorldObject
        {
            positionX = transform.position.x,
            positionY = transform.position.y,
            positionZ = transform.position.z,
            floatData1 = GetComponent<HasHealth>().GetHealth()
        };
    }

    public void LoadState(SaveDataWorldObject state)
    {
        if (!isServer)
            GetComponent<NetworkTransform>().CmdTeleport(new Vector3(state.positionX, state.positionY, state.positionZ));
        else
            GetComponent<NetworkTransform>().RpcTeleport(new Vector3(state.positionX, state.positionY, state.positionZ));
        GetComponent<HasHealth>().SetHealth(state.floatData1);
    }
}
