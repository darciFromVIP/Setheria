using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class Tent : NetworkBehaviour
{
    public List<PlayerCharacter> restingPlayers = new List<PlayerCharacter>();
    public TutorialDataScriptable tutorialAfterBuild;
    public float restCooldown;
    private float restTimer;
    private void Update()
    {
        if (restTimer > 0)
        {
            restTimer -= Time.deltaTime;
        }
    }
    private void Start()
    {
        FindObjectOfType<Tutorial>(true).QueueNewTutorial(tutorialAfterBuild);
    }

    [Command(requiresAuthority = false)]
    public void CmdRestPlayer(NetworkIdentity player)
    {
        RpcRestPlayer(player);
    }
    [ClientRpc]
    private void RpcRestPlayer(NetworkIdentity player)
    {
        var playerCharacter = player.GetComponent<PlayerCharacter>();
        restingPlayers.Add(playerCharacter);
        playerCharacter.DisableCharacter();
        playerCharacter.GetComponent<HasMana>().ChangeGearManaRegen(3);
        playerCharacter.ChangeHungerIntervalMultiplier(1);
    }
    [Command(requiresAuthority = false)]
    public void CmdStopRestPlayer(NetworkIdentity player)
    {
        RpcStopRestPlayer(player);
    }
    [ClientRpc]
    private void RpcStopRestPlayer(NetworkIdentity player)
    {
        var playerCharacter = player.GetComponent<PlayerCharacter>();
        restingPlayers.Remove(playerCharacter);
        playerCharacter.EnableCharacter();
        playerCharacter.GetComponent<HasMana>().ChangeGearManaRegen(-3);
        playerCharacter.ChangeHungerIntervalMultiplier(-1);
    }
    public void StartRestCooldown()
    {
        restTimer = restCooldown;
    }
    public float GetRestCooldown()
    {
        return restTimer;
    }
}
