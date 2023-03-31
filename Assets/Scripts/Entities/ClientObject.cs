using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using UnityEngine.SceneManagement;
public interface LocalPlayer
{
    public ClientObject GetLocalPlayer();
}
public interface NeedsLocalPlayer 
{
    public void SetLocalPlayer(ClientObject player);
}
public class ClientObject : NetworkBehaviour, LocalPlayer
{
    [SerializeField] private List<PlayerCharacter> playerCharacters;
    private List<SaveDataPlayer> playerData;
    public override void OnStartAuthority()
    {
        if (!isOwned)
            return;
        base.OnStartAuthority();
        var arr = SceneManager.GetSceneByName("Game").GetRootGameObjects();
        List<NeedsLocalPlayer> list = new();
        foreach (var item in arr)
        {
            list.AddRange(item.GetComponentsInChildren<NeedsLocalPlayer>(true));
        }
        foreach (var item in list)
        {
            item.SetLocalPlayer(this);
        }
        CmdGetSaveData(SteamUser.GetSteamID());
    }
    public void SpawnPlayer(Hero hero)
    {
        CmdSpawnPlayer(hero);
    }
    [Command]
    public void CmdSpawnPlayer(Hero hero, NetworkConnectionToClient conn = null)        //Commands must have Conn parameter on null to receive the conn from the client
    {
        foreach (var item in playerCharacters)
        {
            if (item.hero == hero)
            {
                var character = Instantiate(item, FindObjectOfType<WorldGenerator>().globalStartingPoint.position, Quaternion.identity, null);
                NetworkServer.Spawn(character.gameObject, conn);
                Debug.Log("Hero Spawned!");
            }
        }
    }

    public List<SaveDataPlayer> GetSaveData()
    {
        return playerData;
    }
    [Command]
    private void CmdGetSaveData(CSteamID id)
    {
        LoadPlayerData(FindObjectOfType<SaveLoadSystem>().LoadFilePlayer(id));
    }
    [ClientRpc]
    private void LoadPlayerData(List<SaveDataPlayer> data)
    {
        playerData = data;
    }

    public ClientObject GetLocalPlayer()
    {
        if (isOwned)
            return this;
        else
            return null;
    }
    public void DisconnectFromServer()
    {
        CmdSaveGame(); 
    }
    [Command]
    private void CmdSaveGame(NetworkConnectionToClient conn = null)
    {
        StartCoroutine(SaveGame(conn));
    }
    [TargetRpc]
    private void DisconnectClient(NetworkConnection conn)
    {
        if (isServer)
        {
            NetworkManager.singleton.StopHost();
        }
        NetworkManager.singleton.StopClient();
    }
    private IEnumerator SaveGame(NetworkConnectionToClient conn)
    {
        Debug.Log("Saving Before Disconnecting");
        FindObjectOfType<SaveLoadSystem>().Save();
        yield return new WaitUntil(SaveFinished);
        DisconnectClient(conn);
    }
    private bool SaveFinished()
    {
        return FindObjectOfType<SaveLoadSystem>().saveFinished;
    }
}
