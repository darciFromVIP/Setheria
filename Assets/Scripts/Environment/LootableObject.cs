using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class LootableObject : NetworkBehaviour, IInteractable
{
    public float lootDuration;
    public float refreshDuration;
    public int xpGranted;
    public int maxCharges;
    private int currentCharges;
    public bool oneTimeLoot = true;
    public bool destroyOnLoot = false;
    public bool lootable = true;
    public ItemType toolRequirement;
    public int toolLevelRequirement;

    public string lootableName;
    public string unlootableName;

    private PlayerController interactingPlayer;
    private TooltipTriggerWorld tooltip;

    public Slider refreshProgressBar;

    private void Start()
    {
        tooltip = GetComponent<TooltipTriggerWorld>();
        tooltip.objectName = lootableName;
        currentCharges = maxCharges;
    }
    public void Interact(PlayerCharacter player)
    {
        if (!lootable)
        {
            FindObjectOfType<SystemMessages>().AddMessage("This object has already been harvested!");
            return;
        }
        if (interactingPlayer)
        {
            FindObjectOfType<SystemMessages>().AddMessage("Someone is already harvesting this object!");
            return;
        }
        if (!FindObjectOfType<CharacterScreen>().CheckToolLevel(toolRequirement, toolLevelRequirement))
        {
            FindObjectOfType<SystemMessages>().AddMessage("You need to equip a " + toolRequirement.ToString() + " of level " + toolLevelRequirement + " to harvest this!");
            return;
        }
        CmdSetInteractingPlayer(player.GetComponent<PlayerController>());
        interactingPlayer = player.GetComponent<PlayerController>();
        interactingPlayer.CmdStartWorking(lootDuration);
        interactingPlayer.Work_Finished.AddListener(GiveLoot);
        interactingPlayer.Work_Cancelled.AddListener(ForgetInteractingPlayer);
    }
    private void ForgetInteractingPlayer()
    {
        CmdSetInteractingPlayer(null);
    }
    [Command(requiresAuthority = false)]
    private void CmdSetInteractingPlayer(NetworkBehaviour player)
    {
        RpcSetInteractingPlayer(player);
    }
    [ClientRpc]
    private void RpcSetInteractingPlayer(NetworkBehaviour player)
    {
        if (player)
            interactingPlayer = player.GetComponent<PlayerController>();
        else
            interactingPlayer = null;
    }
    private void GiveLoot()
    {
        if (!lootable)
        {
            FindObjectOfType<SystemMessages>().AddMessage("This object has already been harvested!");
            return;
        }
        interactingPlayer.GetComponent<PlayerCharacter>().AddXp(xpGranted);
        GetComponent<CanDropItem>().SpawnItemsInInventory(FindObjectOfType<InventoryManager>(true));
        CmdSetInteractingPlayer(null);
        currentCharges--;
        FindObjectOfType<CharacterScreen>(true).ReduceToolDurability(toolRequirement, -1);
        if (currentCharges <= 0)
        {
            CmdUpdateLootability(false);
            if (!oneTimeLoot)
                CmdStartRefreshTimer();
            if (destroyOnLoot)
                CmdDestroyObject();
        }
    }
    [Command(requiresAuthority = false)]
    private void CmdDestroyObject()
    {
        RpcDestroyObject();
    }
    [ClientRpc]
    private void RpcDestroyObject()
    {
        var anim = GetComponentInChildren<Animator>();
        if (anim)
            anim.SetTrigger("Destroy");
        else
            CmdDestroyOnServer();
    }
    public void CmdDestroyOnServer()
    {
        if (isServer)
            NetworkServer.Destroy(gameObject);
    }
    [Command(requiresAuthority = false)]
    private void CmdUpdateLootability(bool value)
    {
        RpcUpdateLootability(value);
    }
    [ClientRpc]
    private void RpcUpdateLootability(bool value)
    {
        lootable = value;
        if (value)
            tooltip.objectName = lootableName;
        else
            tooltip.objectName = unlootableName;
    }
    [Command(requiresAuthority = false)]
    private void CmdStartRefreshTimer()
    {
        RpcShowProgressBar();
    }
    [ClientRpc]
    private void RpcShowProgressBar()
    {
        StartCoroutine(StartRefreshTimer());
    }

    private IEnumerator StartRefreshTimer()
    {
        refreshProgressBar.GetComponentInParent<Canvas>(true).gameObject.SetActive(true);
        refreshProgressBar.maxValue = refreshDuration;
        float timer = refreshDuration;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            refreshProgressBar.value = timer;
            yield return null;
        }
        CmdUpdateLootability(true);
        refreshProgressBar.GetComponentInParent<Canvas>().gameObject.SetActive(false);
        currentCharges = maxCharges;
    }
}
