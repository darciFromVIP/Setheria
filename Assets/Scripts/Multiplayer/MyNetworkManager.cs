using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
public struct ClientConnectedMessage : NetworkMessage
{

}
public struct CurrentWorldSetMessage : NetworkMessage
{
    public SaveDataWorld currentWorldData;
}
public class MyNetworkManager : NetworkManager
{
    public SaveLoadSystem saveLoad;
    private int scenesLoaded = 0;
    public override void OnStartServer()
    {
        base.OnStartServer();
        NetworkServer.RegisterHandler<ClientConnectedMessage>(SendCurrentWorldData);
    }
    private void SendCurrentWorldData<ClientConnectedMessage>(NetworkConnectionToClient conn, ClientConnectedMessage msg)
    {
        NetworkServer.SendToAll(new CurrentWorldSetMessage { currentWorldData = FindObjectOfType<SaveLoadSystem>().currentWorldDataServer.worldSaveData });
    }
    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        base.OnServerConnect(conn);
        if (conn.connectionId != 0)
            FindObjectOfType<SaveLoadSystem>().Save();
    }
    public override void OnClientConnect()
    {
        base.OnClientConnect();
        scenesLoaded = 0;
        NetworkClient.ReplaceHandler<CurrentWorldSetMessage>(SetCurrentWorld);
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        NetworkClient.Send(new ClientConnectedMessage());
    }
    private void SetCurrentWorld(CurrentWorldSetMessage msg)
    {
        if (!saveLoad)
            saveLoad = FindObjectOfType<SaveLoadSystem>();
        saveLoad.LoadStateWorld(msg.currentWorldData);
        NetworkClient.UnregisterHandler<CurrentWorldSetMessage>();
        NetworkClient.RegisterHandler<CurrentWorldSetMessage>(EmptyMsgRegister);
    }
    private void EmptyMsgRegister(CurrentWorldSetMessage msg)
    {

    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Additive)
            scenesLoaded++;
        if (scenesLoaded == 1)                          // 4 for 4 scenes
        {
            StartCoroutine(SwitchScenes());
        }
    }
    private IEnumerator SwitchScenes()
    {
        var load = SceneManager.LoadSceneAsync("Game", LoadSceneMode.Additive);
        FindObjectOfType<LoadingScreen>().LoadAsyncOperation("Loading User Interface...", load);
        while (!load.isDone)
        {
            yield return null;
        }
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("Game"));
        var unload = SceneManager.UnloadSceneAsync("Main Menu");
        while (!unload.isDone)
            yield return null;
        yield return null;
        if (!NetworkClient.ready)
            NetworkClient.Ready();
        while (!NetworkClient.ready)
        {
            yield return null;
        }
        NetworkClient.AddPlayer();
    }
}
