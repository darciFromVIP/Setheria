using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEditor;
using UnityEngine.SceneManagement;
using Mirror;
using Steamworks;
using Newtonsoft.Json;
public class SaveLoadSystem : MonoBehaviour
{
    public string dataDirPath = "";
    public string playerDirPath = "";
    public const string WorldDataFileFormat = ".WorldData";
    public SaveDataWorldServer currentWorldDataServer = null;
    private List<SaveDataPlayer> saveDataPlayers = new();


    public bool saveFinished = false;

    private void OnDisable()
    {
        Debug.Log("Disabling SaveLoad");
        Destroy(gameObject);
    }
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        dataDirPath = Application.persistentDataPath + "/Worlds";
    }
    [ContextMenu("Save")]
    public void Save()
    {
        StartCoroutine(SavingCoro());
    }
    private IEnumerator SavingCoro()
    {
        saveFinished = false;
        currentWorldDataServer = SaveStateWorld();
        SaveFileWorld(currentWorldDataServer);
        SavePlayerStates();
        yield return StartCoroutine(SavePlayerFiles());
        FindObjectOfType<NetworkedSaveLoad>().RpcSaveSuccessful();
        saveFinished = true;
    }
    public void SetCurrentWorld(string fullPath)
    {
        currentWorldDataServer = LoadFileWorld(fullPath);
        playerDirPath = dataDirPath + "/" + currentWorldDataServer.worldSaveData.worldName + "/Players/";
    }
    public void SaveFileWorld(SaveDataWorldServer state, bool freshWorld = false)
    {
        if (freshWorld)
            state.fresh = true;
        string fullPath = dataDirPath + "/" + state.worldSaveData.worldName + "/" + state.worldSaveData.worldName + WorldDataFileFormat;
        Debug.Log(fullPath);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            string dataToStore = JsonConvert.SerializeObject(state);
            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataToStore);
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log("Error occured while trying to save data to file: " + fullPath + "\n" + e);
        }
    }
    public IEnumerator SavePlayerFiles()
    {
        if (SteamMatchmaking.GetNumLobbyMembers(SteamLobby.instance.currentLobbyID) != NetworkServer.connections.Count)
            Debug.Log("The Count of Steam users and connections is not equal!");

        while (SteamMatchmaking.GetNumLobbyMembers(SteamLobby.instance.currentLobbyID) != saveDataPlayers.Count)
            yield return null;
        try
        {
            Directory.CreateDirectory(playerDirPath);
            Debug.Log("Current players: " + SteamMatchmaking.GetNumLobbyMembers(SteamLobby.instance.currentLobbyID));
            for (int i = 0; i < SteamMatchmaking.GetNumLobbyMembers(SteamLobby.instance.currentLobbyID); i++)
            {
                if (saveDataPlayers.Count > i)
                {
                    Debug.Log(saveDataPlayers[i].name + " belongs to " + SteamMatchmaking.GetLobbyMemberByIndex(SteamLobby.instance.currentLobbyID, i));
                }
                var fullPath = playerDirPath + SteamMatchmaking.GetLobbyMemberByIndex(SteamLobby.instance.currentLobbyID, i);
                List<SaveDataPlayer> loadedPlayerData = new();
                if (File.Exists(fullPath))
                {
                    loadedPlayerData = LoadFilePlayer(fullPath);
                    for (int k = 0; k < loadedPlayerData.Count; k++)
                    {
                        if (loadedPlayerData[k].hero == saveDataPlayers[i].hero)
                            loadedPlayerData[k] = saveDataPlayers[i];
                    }
                }
                else
                {
                    for (int j = 0; j < Enum.GetValues(typeof(Hero)).Length; j++)
                    {
                        loadedPlayerData.Add(new SaveDataPlayer((Hero)j));
                    }
                }
                string dataToStore = JsonConvert.SerializeObject(loadedPlayerData);
                using (FileStream stream = new FileStream(fullPath, FileMode.Create))
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        writer.Write(dataToStore);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error occured while trying to save data to file: " + playerDirPath + "\n" + e);
        }
    }
    public void SaveNewPlayerFile(List<SaveDataPlayer> playerStates, string worldName)
    {
        playerDirPath = dataDirPath + "/" + worldName + "/Players/";
        try
        {
            Directory.CreateDirectory(playerDirPath);
            var fullPath = playerDirPath + SteamUser.GetSteamID();
            string dataToStore = JsonConvert.SerializeObject(playerStates);
            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataToStore);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error occured while trying to save data to file: " + playerDirPath + "\n" + e);
        }
    }
    public SaveDataWorldServer LoadFileWorld(string fileName)
    {
        string fullPath = Path.Combine(dataDirPath, fileName);
        if (File.Exists(fullPath))
        {
            try
            {
                string dataToLoad;
                using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }
                Debug.Log(dataToLoad);
                return JsonConvert.DeserializeObject<SaveDataWorldServer>(dataToLoad);

            }
            catch (Exception e)
            {
                Debug.LogError("Error occured while trying to load data from file: " + fullPath + "\n" + e);
                return new SaveDataWorldServer();
            }
        }
        return new SaveDataWorldServer();
    }
    public List<SaveDataPlayer> LoadFilePlayer(string fullPath)
    {
        if (File.Exists(fullPath))
        {
            try
            {
                string dataToLoad;
                using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }
                Debug.Log(dataToLoad);
                return JsonConvert.DeserializeObject<List<SaveDataPlayer>>(dataToLoad);

            }
            catch (Exception e)
            {
                Debug.LogError("Error occured while trying to load data from file: " + fullPath + "\n" + e);
                return new List<SaveDataPlayer>();
            }
        }
        var newData = new List<SaveDataPlayer>();
        for (int j = 0; j < Enum.GetValues(typeof(Hero)).Length; j++)
        {
            newData.Add(new SaveDataPlayer((Hero)j));
        }
        return newData;
    }
    public List<SaveDataPlayer> LoadFilePlayer(CSteamID steamUserId)
    {
        return LoadFilePlayer(playerDirPath + steamUserId);
    }
    SaveDataWorldServer SaveStateWorld()
    {
        if (SceneManager.GetActiveScene().name != "Game")
        {
            Debug.Log("The Scene Game isn't loaded!");
            return null;
        }
        SaveDataWorldServer state = new();
        var questManager = FindObjectOfType<QuestManager>(true);
        List<bool> unlockedItems = new();
        foreach (var item in questManager.itemDatabase.items)
        {
            unlockedItems.Add(item.unlocked);
        }
        List<bool> unlockedRecipes = new();
        foreach (var item in FindObjectOfType<GameManager>().recipeDatabase.allRecipes)
        {
            unlockedRecipes.Add(item.unlocked);
        }
        state.worldSaveData.unlockedItems = unlockedItems;
        state.worldSaveData.unlockedRecipes = unlockedRecipes;
        state.worldSaveData.worldName = currentWorldDataServer.worldSaveData.worldName;
        state.worldSaveData.worldSeed = currentWorldDataServer.worldSaveData.worldSeed;
        state.worldSaveData.fogOfWar = new byte[FoW.FogOfWarTeam.GetTeam(0).mapResolution.x * FoW.FogOfWarTeam.GetTeam(0).mapResolution.y];
        FoW.FogOfWarTeam.GetTeam(0).GetTotalFogValues(ref state.worldSaveData.fogOfWar);
        state.worldSaveData.syncedQuestlines = FindObjectOfType<QuestManager>().SaveStateSynchronized();
        var gameManager = FindObjectOfType<GameManager>();
        state.worldSaveData.resources = gameManager.GetResources();
        state.worldSaveData.knowledge = gameManager.GetKnowledge();
        foreach (var item in gameManager.structureUpgradeDatabase.upgrades)
        {
            state.worldSaveData.structureUpgrades.Add(item.currentLevel);
        }
        var stash = FindObjectOfType<StashInventory>(true).GetAllItems();
        List<SaveDataItem> items = new();
        foreach (var item in stash)
        {
            items.Add(new SaveDataItem { name = item.item.name, stacks = item.stacks });
        }
        state.worldSaveData.stash = items;
        foreach (var item in FindObjectsOfType<SaveableBehaviour>())
        {
            state.worldObjects.Add(item.Id, item.SaveState());
        }
        currentWorldDataServer = state;
        return state;
    }
    public void LoadStateWorld(SaveDataWorld state)
    {
        FindObjectOfType<WorldGenerator>().CreateGameWorld(state);
    }
    [ContextMenu("Save Players")]
    public void SavePlayerStates()
    {
        List<NetworkIdentity> list = new();
        foreach (var item in NetworkServer.connections.Values)
        {
            foreach (var item2 in item.owned)
            {
                list.Add(item2);
                Debug.Log(item2 + " is owned by " + item.connectionId);
            }
        }
        saveDataPlayers.Clear();
        foreach (var item in list)
        {
            if (item.TryGetComponent(out PlayerCharacter player))
            {
                Debug.Log(item.name + " is connected to " + item.connectionToClient.connectionId);
                player.SaveState(item.connectionToClient, item);
            }
        }
    }
    public void SavePlayerState(SaveDataPlayer savedata)
    {
        saveDataPlayers.Add(savedata);
    }
#if UNITY_EDITOR
    [ContextMenu("GenerateIdsInScene")]
    public void GenerateIdsInScene()
    {
        foreach (SaveableBehaviour item in FindObjectsOfType<SaveableBehaviour>(true))
        {
            EditorUtility.SetDirty(item);
            item.GenerateId();
        }
    }
#endif
}
