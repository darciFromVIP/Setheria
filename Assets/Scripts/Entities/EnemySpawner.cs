using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Unity.VisualScripting;

[System.Serializable]
public struct EnemySpawnChances
{
    public List<EnemySpawnChance> enemySpawnChances;
}
[System.Serializable]
public struct EnemySpawnChance
{
    public EnemyCharacter enemy;
    public int amount;
    public int chance;
}
[System.Serializable]
public class EnemySpawner : NetworkBehaviour
{
    [Tooltip("How much time elapses between each wave spawn.")]
    public float timeIntervalBetweenSpawns;
    [Tooltip("A player building must be within this range in order to activate the cave.")]
    public float activeRange;
    [Tooltip("What obstacles need to be destroyed in order to activate the cave?")]
    public List<LootableObject> objectsToBeDestroyed = new List<LootableObject>();
    [Tooltip("What enemies will spawn from the cave?")]
    public List<EnemySpawnChances> enemySpawnTable;
    public Transform spawnPoint;
    public EnemyCharacter boss;
    private Vector3 attackPoint;
    private float timer;
    private bool activated = false;
    private bool foundStructure = false;
    private void Start()
    {
        if (isServer)
        {
            foreach (var item in objectsToBeDestroyed)
            {
                item.Object_Destroyed.AddListener(ReduceRequirement);
            }
            if (objectsToBeDestroyed.Count == 0)
                activated = true;
        }
    }
    private void ReduceRequirement(LootableObject objectDestroyed)
    {
        if (objectsToBeDestroyed.Contains(objectDestroyed))
            objectsToBeDestroyed.Remove(objectDestroyed);
        if (objectsToBeDestroyed.Count == 0)
            activated = true;
    }
    private void Update()
    {
        if (timer < timeIntervalBetweenSpawns)
            timer += Time.deltaTime;
        if (timer > 1 && !foundStructure)
        {
            attackPoint = Vector3.zero;
            foreach (var item in FindObjectsOfType<Structure>(true))
            {
                if (Vector3.Distance(item.transform.position, transform.position) <= activeRange)
                    attackPoint = item.transform.position;
            }
            if (attackPoint == Vector3.zero)
            {
                timer = 0;
                return;
            }
            else
                foundStructure = true;
        }
        if (!isServer || !activated || !foundStructure)
            return;
        
        if (timer >= timeIntervalBetweenSpawns)
            SpawnEnemies();
    }
    private void SpawnEnemies()
    {
        timer = 0;
        attackPoint = Vector3.zero;
        foreach (var item in FindObjectsOfType<Structure>(true))
        {
            if (Vector3.Distance(item.transform.position, transform.position) <= activeRange)
                attackPoint = item.transform.position;
        }
        if (attackPoint == Vector3.zero)
            return;
        
        if (enemySpawnTable.Count <= 0)
            return;
        int random;
        int temp;
        foreach (var item in enemySpawnTable)
        {
            random = Random.Range(0, 100);
            temp = 0;
            foreach (var item2 in item.enemySpawnChances)
            {
                temp += item2.chance;
                if (random >= temp - item2.chance && random < temp)
                {
                    StartCoroutine(CmdSpawnEnemies(item2.enemy, item2.amount));
                }
            }
        }
    }
    private IEnumerator CmdSpawnEnemies(EnemyCharacter enemy, int amount)
    {
        List<EnemyCharacter> spawnedEnemies = new List<EnemyCharacter>();
        if (enemy != null)
        {
            for (int i = 0; i < amount; i++)
            {
                var instance = Instantiate(enemy, spawnPoint.position, Quaternion.identity);
                spawnedEnemies.Add(instance);
                NetworkServer.Spawn(instance.gameObject);
            }
            yield return new WaitForSeconds(1);
            foreach (var item in spawnedEnemies)
            {
                if (item.TryGetComponent(out Wander wander))
                    wander.enabled = false;
                item.GetComponent<CanMove>().MoveTo(attackPoint);
                item.campRadius = 99999;
            }
        }
    }
    public void SpawnBoss()
    {
        var instance = Instantiate(boss, spawnPoint.position, Quaternion.identity);
        NetworkServer.Spawn(instance.gameObject);
        instance.On_Death.AddListener(DestroySpawner);
    }
    private void DestroySpawner()
    {
        var animator = GetComponentInChildren<Animator>();
        if (animator)
            animator.SetTrigger("Destroy");
        else
            CmdDestroy();
    }
    [Command(requiresAuthority = false)]
    public void CmdDestroy()
    {
        NetworkServer.Destroy(gameObject);
    }
}
