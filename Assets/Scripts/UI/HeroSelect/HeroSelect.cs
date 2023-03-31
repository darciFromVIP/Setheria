using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class HeroSelect : MonoBehaviour, NeedsLocalPlayer
{
    private ClientObject localPlayer;
    public GameObject window;
    private void Start()
    {
        window.SetActive(true);
    }

    public void SetLocalPlayer(ClientObject player)
    {
        localPlayer = player;
    }

    public void SpawnPlayer(Hero hero)
    {
        if (localPlayer)
            localPlayer.SpawnPlayer(hero);
        else
            Debug.Log("Local Player is not present!");
    }
}
