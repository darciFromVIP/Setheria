using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Mirror;
using Mirror.FizzySteam;
using UnityEngine.SceneManagement;
using System;

public class SteamLobby : MonoBehaviour
{
    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;

    private const string HostAddressKey = "HostAddress";

    public CSteamID currentLobbyID;

    public static SteamLobby instance;

    [SerializeField] private NetworkManager networkManager;
    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        else
            instance = this;
        networkManager = GetComponent<NetworkManager>();
        Transport.active = FindObjectOfType<FizzySteamworks>();
    }
    private void Start()
    {
        if (!SteamManager.Initialized)
            return;

        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
    }
    public void HostLobby()
    {
        Transport.active = FindObjectOfType<FizzySteamworks>();
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, networkManager.maxConnections);
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
            return;
        networkManager.StartHost();
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey, SteamUser.GetSteamID().ToString());
        currentLobbyID = new CSteamID(callback.m_ulSteamIDLobby);
        Debug.Log(new CSteamID(callback.m_ulSteamIDLobby));
    }
    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        FindObjectOfType<LoadingScreen>().LoadOperation("Connecting...");
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }
    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        if (NetworkServer.active)
            return;
        string hostAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey);
        FindObjectOfType<LoadingScreen>().LoadOperation("Connecting...");

        networkManager.networkAddress = hostAddress;
        networkManager.StartClient();
    }
    private void OnDestroy()
    {
        lobbyCreated.Dispose();
        gameLobbyJoinRequested.Dispose();
        lobbyEntered.Dispose();
    }
}
