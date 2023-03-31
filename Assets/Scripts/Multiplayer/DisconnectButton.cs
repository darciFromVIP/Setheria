using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisconnectButton : MonoBehaviour, NeedsLocalPlayer
{
    private ClientObject localPlayer;
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(Disconnect);
    }
    public void SetLocalPlayer(ClientObject player)
    {
        localPlayer = player;
    }
    private void Disconnect()
    {
        localPlayer.DisconnectFromServer();
    }
}
