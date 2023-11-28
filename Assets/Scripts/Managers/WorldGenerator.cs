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
                Debug.Log("Loading terrain");
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
        Debug.Log("Waiting for Scene to load");
        yield return new WaitUntil(IsMainSceneLoaded);
        if (NetworkServer.active)
            LoadWorldObjects(FindObjectOfType<SaveLoadSystem>().currentWorldDataServer.worldObjects);
        FoW.FogOfWarTeam.GetTeam(0).SetTotalFogValues(state.fogOfWar);
        Debug.Log("Scene Loaded");
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
                NetworkServer.Destroy(item.gameObject);
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
