using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Mirror;
using System.Linq;

public class HasAggro : NetworkBehaviour
{
    [SerializeField] private float aggroRange;
    [SerializeField] private float allyHelpRange;
    public bool shouldCallForHelp = true;
    public EnemyType enemyType;
    [Tooltip("If this enemy is a Beast, assign this event. Otherwise leave blank.")]
    public EventScriptable BeastKilledEvent;
    private LayerMask enemyLayers;
    private LayerMask allyLayers;
    private Dictionary<NetworkIdentity, float> aggroList = new();

    [System.NonSerialized]
    public UnityEvent<NetworkIdentity> Target_Found = new();
    private void Start()
    {
        StartCoroutine(CheckForTargets());
        enemyLayers = GetComponent<Character>().enemyLayers;
        allyLayers = GetComponent<Character>().allyLayers;
        if (BeastKilledEvent)
            GetComponent<HasHealth>().On_Death.AddListener(BeastKilledEventInvoke);
        if (TryGetComponent(out HasHealth hp))
        {
            hp.Damage_Taken_Amount.AddListener(DamageTaken);
            hp.On_Death.AddListener(DisableScript);
        }
        if (TryGetComponent(out CanAttack attack))
            attack.Target_Lost.AddListener(TargetLost);
        Target_Found.AddListener(AddEmptyAggroList);
    }
    private void DisableScript()
    {
        StopAllCoroutines();
        GetComponent<CanAttack>().TargetLost();
        enabled = false;
    }
    private IEnumerator CheckForTargets()
    {
        while (true)
        {
            if (enabled && aggroList.Count == 0)
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
                            var ignore = item.GetComponentInChildren<IgnoredByEnemies>();
                            if (ignore)
                            {
                                if (!ignore.ignoredEnemies.Contains(enemyType))
                                {
                                    distance = currentDistance;
                                    resultTarget = item;
                                }
                            }
                            else
                            {
                                distance = currentDistance;
                                resultTarget = item;
                            }
                           
                        }
                    }
                }
                if (resultTarget)
                {
                    if (resultTarget.TryGetComponent(out HasHealth hp))
                        if (hp.GetHealth() > 0)
                        {
                            if (resultTarget.TryGetComponent(out Character character))
                                if (character.HasBuff("Sleeping") > 0)
                                    continue;

                            if (shouldCallForHelp)
                            {
                                Collider[] allies = new Collider[10];
                                Physics.OverlapSphereNonAlloc(transform.position, allyHelpRange, allies, allyLayers);
                                foreach (var item in allies)
                                {
                                    if (item)
                                    {
                                        if (item.TryGetComponent(out CanAttack attackComp))
                                            if (attackComp.enemyTarget == null)
                                            {
                                                item.GetComponent<HasAggro>().Target_Found.Invoke(resultTarget.GetComponent<NetworkIdentity>());
                                            }
                                    }
                                }
                            }
                            else
                            {
                                Target_Found.Invoke(resultTarget.GetComponent<NetworkIdentity>());
                            }
                                
                        }
                }
            }
            yield return new WaitForSeconds(1);
        }
    }
    private void TargetLost()
    {
        aggroList.Clear();
    }
    private void DamageTaken(NetworkIdentity enemy, float damageAmount)
    {
        if (enemy == GetComponent<NetworkIdentity>() || GetComponent<HasHealth>().GetHealth() <= 0)
            return;
        if (enemy.TryGetComponent(out HasHealth hp))
            if (hp.GetHealth() <= 0)
                return;
        int multiplier = 1;
        if (enemy.TryGetComponent(out PlayerCharacter player))
        {
            if (player.TryGetComponent(out Shapeshifter shapeshifter))
            {
                if (shapeshifter.defaultModel.gameObject.activeSelf)
                    multiplier = 3;
            }
            else if (false)                    //Add tank players here
            {
                multiplier = 3;
            }

        }
        if (aggroList.ContainsKey(enemy))
            aggroList[enemy] += damageAmount * multiplier;
        else
            aggroList.Add(enemy, damageAmount * multiplier);
        var topAggro = aggroList.OrderByDescending(pair => pair.Value).Take(1).ToList()[0].Key;
        Target_Found.Invoke(topAggro);
    }
    private void BeastKilledEventInvoke()
    {
        BeastKilledEvent.voidEvent.Invoke();
    }
    public void CheckForStructures()
    {
        StartCoroutine(CheckForStructuresCoro());
    }
    private IEnumerator CheckForStructuresCoro()
    {
        float timer = 10;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                timer = 10;
                if (!GetComponent<CanAttack>().enemyTarget)
                {
                    var attackPoint = Vector3.zero;
                    foreach (var item in FindObjectsOfType<Structure>(true))
                    {
                        attackPoint = item.transform.position;
                        break;
                    }
                    GetComponent<CanMove>().MoveTo(attackPoint);
                }
            }
            yield return null;
        }
    }
    private void AddEmptyAggroList(NetworkIdentity key)
    {
        if (!aggroList.ContainsKey(key))
            aggroList.Add(key, 0);
    }
}
