using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;
public class Tent : NetworkBehaviour
{
    public List<PlayerCharacter> restingPlayers = new List<PlayerCharacter>();
    public TutorialDataScriptable tutorialAfterBuild;
    public List<int> stashSlotsPerLevel = new();
    public float restCooldown;
    private float restTimer;

    public UnityEvent Stopped_Resting = new();
    private void Update()
    {
        if (restTimer > 0)
        {
            restTimer -= Time.deltaTime;
        }
    }
    private void Start()
    {
        FindObjectOfType<Tutorial>(true).QueueTentTutorial(tutorialAfterBuild);
        GetComponent<Structure>().Structure_Upgraded.AddListener(CmdUpgradeStash);
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
        Stopped_Resting.Invoke();
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
    [Command(requiresAuthority = false)]
    private void CmdUpgradeStash(int level)
    {
        RpcUpgradeStash(level);
    }
    [ClientRpc]
    private void RpcUpgradeStash(int level)
    {
        FindObjectOfType<StashInventory>().ExtendInventoryUpTo(stashSlotsPerLevel[level]);
    }
}
