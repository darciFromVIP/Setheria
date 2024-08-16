using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;
public class Tent : NetworkBehaviour
{
    public List<PlayerCharacter> restingPlayers = new List<PlayerCharacter>();
    public TutorialDataScriptable tutorialAfterBuild;
    public float restCooldown;
    private float restTimer;
    public int stashSlots;
    public bool stashOccupied = false;

    public UnityEvent Stopped_Resting = new();
    private void Update()
    {
        if (restTimer > 0)
        {
            restTimer -= Time.deltaTime;
        }
    }
    protected virtual void Start()
    {
        if (tutorialAfterBuild)
            FindObjectOfType<Tutorial>(true).QueueTentTutorial(tutorialAfterBuild);
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
        playerCharacter.GetComponent<HasMana>().ChangeGearManaRegen(5 + (playerCharacter.GetComponent<HasMana>().GetFinalMaxMana() * 0.01f));
        playerCharacter.ChangeHungerIntervalMultiplier(3);
    }
    [Command(requiresAuthority = false)]
    public void CmdStopRestPlayer(NetworkIdentity player)
    {
        RpcStopRestPlayer(player);
    }
    [ClientRpc]
    private void RpcStopRestPlayer(NetworkIdentity player)
    {
        Stopped_Resting.Invoke();
        var playerCharacter = player.GetComponent<PlayerCharacter>();
        restingPlayers.Remove(playerCharacter);
        playerCharacter.EnableCharacter();
        playerCharacter.GetComponent<HasMana>().ChangeGearManaRegen(-(5 + (playerCharacter.GetComponent<HasMana>().GetFinalMaxMana() * 0.01f)));
        playerCharacter.ChangeHungerIntervalMultiplier(-3);
    }
    public void StartRestCooldown()
    {
        restTimer = restCooldown;
    }
    public float GetRestCooldown()
    {
        return restTimer;
    }
    [Command(requiresAuthority = false)]
    public void CmdStashOccupy(bool value)
    {
        RpcStashOccupy(value);
    }
    [ClientRpc]
    public void RpcStashOccupy(bool value)
    {
        stashOccupied = value;
    }
}
