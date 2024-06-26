using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using Mirror;
using UnityEngine.AI;

public class WorldGenerator : MonoBehaviour
{
    [Scene] 
    public string[] biomes;

    public Transform globalStartingPoint;

    public ItemPrefabDatabase itemDatabase;
    public StructureDatabase structureDatabase;
    public EntityDatabase entityDatabase;

    public SaveDataWorld lastLoadedWorldState;

    public static WorldGenerator instance;
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
            instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public void CreateGameWorld(SaveDataWorld state)
    {
        StartCoroutine(CreateGameWorldCoro(state));
    }
    private IEnumerator CreateGameWorldCoro(SaveDataWorld state)
    {
        lastLoadedWorldState = state;
        int random;
        UnityEngine.Random.InitState(state.worldSeed);
        List<Vector3> dic = new() { new Vector3(0, 0, 0), new Vector3(0, 0, 600), new Vector3(600, 0, 0), new Vector3(600, 0, 600) };
        string[] biomeCopy = new string[biomes.Length];
        biomes.CopyTo(biomeCopy, 0);
        for (int i = 0; i < 1; i++)                         // i < 4 for 4 biomes - ALSO CHANGE IN NETWORK MANAGER
        {
            do 
                random = UnityEngine.Random.Range(0, biomeCopy.Length);
            while (biomeCopy[random] == null);

            var operation = SceneManager.LoadSceneAsync(biomeCopy[random], LoadSceneMode.Additive);
            FindObjectOfType<LoadingScreen>().LoadAsyncOperation("Loading Terrain...", operation);
            while (!operation.isDone)
            {
                yield return null;
            }
            biomeCopy[random] = null;
        }
        for (int i = 1; i < 2; i++)                         // i < 5 for 4 biomes
        {
            foreach (var item in SceneManager.GetSceneAt(i).GetRootGameObjects())
            {
                if (i == 1 && item.CompareTag("Starting Position"))
                    globalStartingPoint = item.transform;
                if (item.TryGetComponent(out NavMeshAgent agent))
                    agent.Warp(item.transform.position + dic[i - 1]);
                else
                    item.transform.position += dic[i - 1];
            } 
        }
        yield return new WaitUntil(IsMainSceneLoaded);
        yield return StartCoroutine(LoadWorldState(state));
        Debug.Log("Scene Loaded");
    }
    public IEnumerator LoadWorldState(SaveDataWorld state)
    {
        var gameManager = FindObjectOfType<GameManager>();
        if (NetworkServer.active)
        {
            while (gameManager == null)
                yield return null;
            gameManager.ChangeResources(state.resources);
            gameManager.ChangeKnowledge(state.knowledge);
        }
        Debug.Log(gameManager == null);
        if (state.structureUpgrades != null && gameManager != null)
        {
            if (state.structureUpgrades.Count == gameManager.structureUpgradeDatabase.upgrades.Count)
            {
                for (int i = 0; i < gameManager.structureUpgradeDatabase.upgrades.Count; i++)
                {
                    gameManager.structureUpgradeDatabase.upgrades[i].currentLevel = state.structureUpgrades[i];
                }
            }
        }
        if (NetworkServer.active)
            LoadWorldObjects(FindObjectOfType<SaveLoadSystem>().currentWorldDataServer.worldObjects);
        FoW.FogOfWarTeam.GetTeam(0).SetTotalFogValues(state.fogOfWar);
        
        var manager = FindObjectOfType<InventoryManager>(true);
        if (state.unlockedItems.Count > 0)
        {
            for (int i = 0; i < manager.itemDatabase.items.Count; i++)
            {
                if (state.unlockedItems.Count <= i)
                    break;
                manager.itemDatabase.items[i].unlocked = state.unlockedItems[i];
            }
        }
        if (state.unlockedRecipes.Count > 0)
        {
            while (gameManager == null)
            {
                gameManager = FindObjectOfType<GameManager>();
                yield return null;
            }
            var recipes = gameManager.recipeDatabase.allRecipes;
            for (int i = 0; i < recipes.Count; i++)
            {
                recipes[i].unlocked = state.unlockedRecipes[i];
                recipes[i].visible = state.unlockedRecipes[i];
            }
        }
        FindObjectOfType<StashInventory>(true).LoadStash();
    }
    private void LoadWorldObjects(Dictionary<string, Dictionary<string, SaveDataWorldObject>> worldObjects)
    {
        foreach (var item in FindObjectsOfType<SaveableBehaviour>())
        {
            if (worldObjects.TryGetValue(item.Id, out Dictionary<string, SaveDataWorldObject> value))
            {
                item.LoadState(value);
                worldObjects.Remove(item.Id);
            }
            else if (!FindObjectOfType<SaveLoadSystem>().currentWorldDataServer.fresh)
            {
                if (item.TryGetComponent(out NetworkBehaviour net))
                    NetworkServer.Destroy(item.gameObject);
                else
                    Destroy(item.gameObject);
            }
        }
        foreach (var item in worldObjects)
        {
            foreach (var item2 in item.Value)
            {
                GameObject temp = null;
                if (item2.Key == typeof(Item).ToString())
                    temp = itemDatabase.GetItemByName(item2.Value.name).gameObject;
                if (item2.Key == typeof(Structure).ToString())
                    temp = structureDatabase.GetStructureByName(item2.Value.name).gameObject;
                if (item2.Key == typeof(Ship).ToString() || item2.Key == typeof(EnemyCharacter).ToString())
                    temp = entityDatabase.GetEntityByName(item2.Value.name).gameObject;
                if (temp)
                {
                    var spawnedObject = Instantiate(temp, Vector3.zero, temp.transform.rotation);
                    spawnedObject.GetComponent<SaveableBehaviour>().LoadState(item.Value);
                    NetworkServer.Spawn(spawnedObject);
                    break;
                }
            }
        }
    }
    private bool IsMainSceneLoaded()
    {
        return SceneManager.GetActiveScene().name == "Game";
    }
}
