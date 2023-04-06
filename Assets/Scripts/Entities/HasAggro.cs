using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Mirror;
public class HasAggro : NetworkBehaviour
{
    [SerializeField] private float aggroRange;
    [SerializeField] private float allyHelpRange;
    private LayerMask enemyLayers;
    private LayerMask allyLayers;

    [System.NonSerialized]
    public UnityEvent<NetworkIdentity> Target_Found = new();
    private void Start()
    {
        StartCoroutine(CheckForTargets());
        enemyLayers = GetComponent<Character>().enemyLayers;
        allyLayers = GetComponent<Character>().allyLayers;
    }

    private IEnumerator CheckForTargets()
    {
        while (true)
        {
            if (enabled)
            {
                Collider[] targets = new Collider[5];
                Physics.OverlapSphereNonAlloc(transform.position, aggroRange, targets, enemyLayers);
                float distance = aggroRange;
                Collider resultTarget = null;
                foreach (var item in targets)
                {
                    if (item)
                    {
                        var currentDistance = Vector3.Distance(transform.position, item.transform.position);
                        if (currentDistance < distance)
                        {
                            distance = currentDistance;
                            resultTarget = item;
                        }
                    }
                }
                if (resultTarget)
                {
                    Collider[] allies = new Collider[10];
                    Physics.OverlapSphereNonAlloc(transform.position, allyHelpRange, allies, allyLayers);
                    foreach (var item in allies)
                    {
                        if (item)
                        {
                            item.GetComponent<HasAggro>().Target_Found.Invoke(resultTarget.GetComponent<NetworkIdentity>());
                        }
                    }
                }
            }
            yield return new WaitForSeconds(1);
        }
    }
}
