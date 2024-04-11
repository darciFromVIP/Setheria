using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Unity.VisualScripting;
[System.Serializable]
public struct EnemySpawnChancesWithDay
{
    public List<EnemySpawnChances> enemySpawnTable;
    public int endingDay;
}
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
public class EnemySpawner : NetworkBehaviour, ISaveable
{
    [Tooltip("How much time elapses before the wave spawns once night starts.")]
    public float timeInterval;
    [Tooltip("A player building must be within this range in order to activate the cave.")]
    public float activeRange;
    [Tooltip("What obstacles need to be destroyed in order to activate the cave?")]
    public List<LootableObject> objectsToBeDestroyed = new List<LootableObject>();              // May need to save this as well
    [Tooltip("What enemies will spawn from the cave?")]
    public List<EnemySpawnChancesWithDay> enemySpawns;
    public Transform spawnPoint;
    public EnemyCharacter boss;
    private Vector3 attackPoint;
    private float timer;
    private bool activated = false;
    private bool foundStructure = false;
    private DayNightCycle dayNight;
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
            dayNight = FindObjectOfType<DayNightCycle>();
            dayNight.Night_Started.AddListener(NightStarted);
        }
    }
    private void ReduceRequirement(LootableObject objectDestroyed)
    {
        if (objectsToBeDestroyed.Contains(objectDestroyed))
            objectsToBeDestroyed.Remove(objectDestroyed);
        if (objectsToBeDestroyed.Count == 0)
            activated = true;
    }
    private void NightStarted()
    {
        if (dayNight.daysAlive >= 2)
            StartCoroutine(StartTimer());
    }
    private IEnumerator StartTimer()
    {
        while (timer < timeInterval)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        timer = 0;
        if (!foundStructure)
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
                yield break;
            }
            else
                foundStructure = true;
        }

        if (!isServer || !activated || !foundStructure)
            yield break;

        SpawnEnemies();
    }
    private void SpawnEnemies()
    {
        attackPoint = Vector3.zero;
        foreach (var item in FindObjectsOfType<Structure>(true))
        {
            if (Vector3.Distance(item.transform.position, transform.position) <= activeRange)
                attackPoint = item.transform.position;
        }
        if (attackPoint == Vector3.zero)
            return;
        
        if (enemySpawns.Count <= 0)
            return;
        int random;
        int temp;
        foreach (var item in enemySpawns)
        {
            if (dayNight.daysAlive <= item.endingDay)
            {
                foreach (var item2 in item.enemySpawnTable)
                {
                    random = Random.Range(0, 100);
                    temp = 0;
                    foreach (var item3 in item2.enemySpawnChances)
                    {
                        temp += item3.chance;
                        if (random >= temp - item3.chance && random < temp)
                        {
                            StartCoroutine(CmdSpawnEnemies(item3.enemy, item3.amount));
                        }
                    }
                }
                break;
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
                    Destroy(wander);
                item.GetComponent<CanMove>().MoveTo(attackPoint);
                item.campRadius = 99999;
                item.GetComponent<HasAggro>().CheckForStructures();
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

    public SaveDataWorldObject SaveState()
    {
        return new SaveDataWorldObject
        {
            floatData1 = timer,
        };
    }

    public void LoadState(SaveDataWorldObject state)
    {
        timer = state.floatData1;
        if (dayNight.IsNight())
            NightStarted();
        foreach (var item in FindObjectsOfType<EnemyCharacter>())
        {
            if (item.GetComponent<HasHealth>().GetBaseMaxHealth() == boss.GetComponent<HasHealth>().GetBaseMaxHealth() && Vector3.Distance(transform.position, item.transform.position) <= 5)
                item.On_Death.AddListener(DestroySpawner);
        }
    }
}
