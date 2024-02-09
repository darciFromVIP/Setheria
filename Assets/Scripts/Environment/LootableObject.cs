using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;
using HighlightPlus;
using UnityEngine.Events;

public class LootableObject : NetworkBehaviour, IInteractable, NeedsLocalPlayerCharacter, ISaveable
{
    public float lootDuration;
    public float refreshDuration;
    public int xpGranted;
    public int maxCharges;
    private int currentCharges;
    private float refreshTimer;
    public bool oneTimeLoot = true;
    public bool destroyOnLoot = false;
    public bool lootable = true;
    public ItemType toolRequirement;
    public int toolLevelRequirement;
    public TalentTreeType professionRequired;
    public int professionExperienceRequired;
    public List<GameObject> effectsToHide = new();
    public Animator anim;

    public string lootableName;
    public string unlootableName;

    private PlayerController interactingPlayer;
    private TooltipTriggerWorld tooltip;

    public Slider refreshProgressBar;
    public TextMeshProUGUI remainingChargesText;
    public EventScriptable Player_Event;
    [Tooltip("Only fill this if this object is involved in a quest.")]
    public EventScriptable Quest_Event;
    public UnityEvent<LootableObject> Object_Destroyed = new();

    private void Start()
    {
        tooltip = GetComponent<TooltipTriggerWorld>();
        Player_Event.playerEvent.AddListener(SetLocalPlayerCharacter);
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
        if (toolRequirement != ItemType.None && player.TryGetComponent(out Shapeshifter shapeshifter))
        {
            if (shapeshifter.shapeshiftedModel.gameObject.activeSelf)
            {
                FindObjectOfType<SystemMessages>().AddMessage("You can't harvest this in shapeshifted form!");
                return;
            }
        }
        switch (professionRequired)
        {
            case TalentTreeType.Special:
                break;
            case TalentTreeType.Gathering:
                if (player.professions.gathering < professionExperienceRequired)
                {
                    FindObjectOfType<SystemMessages>().AddMessage("You need " + professionRequired.ToString() + " experience of at least " + professionExperienceRequired + " to harvest this!");
                    return;
                }
                break;
            case TalentTreeType.Cooking:
                if (player.professions.cooking < professionExperienceRequired)
                {
                    FindObjectOfType<SystemMessages>().AddMessage("You need " + professionRequired.ToString() + " experience of at least " + professionExperienceRequired + " to harvest this!");
                    return;
                }
                break;
            case TalentTreeType.Alchemy:
                if (player.professions.alchemy < professionExperienceRequired)
                {
                    FindObjectOfType<SystemMessages>().AddMessage("You need " + professionRequired.ToString() + " experience of at least " + professionExperienceRequired + " to harvest this!");
                    return;
                }
                break;
            case TalentTreeType.Fishing:
                if (player.professions.fishing < professionExperienceRequired)
                {
                    FindObjectOfType<SystemMessages>().AddMessage("You need " + professionRequired.ToString() + " experience of at least " + professionExperienceRequired + " to harvest this!");
                    return;
                }
                break;
            case TalentTreeType.Exploration:
                if (player.professions.exploration < professionExperienceRequired)
                {
                    FindObjectOfType<SystemMessages>().AddMessage("You need " + professionRequired.ToString() + " experience of at least " + professionExperienceRequired + " to harvest this!");
                    return;
                }
                break;
            default:
                break;
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
        interactingPlayer.GetComponent<PlayerCharacter>().CmdAddXp(xpGranted);
        switch (professionRequired)
        {
            case TalentTreeType.Gathering:
                interactingPlayer.GetComponent<PlayerCharacter>().professions.AddGathering(1);
                break;
            case TalentTreeType.Cooking:
                interactingPlayer.GetComponent<PlayerCharacter>().professions.AddCooking(1);
                break;
            case TalentTreeType.Alchemy:
                interactingPlayer.GetComponent<PlayerCharacter>().professions.AddAlchemy(1);
                break;
            case TalentTreeType.Fishing:
                interactingPlayer.GetComponent<PlayerCharacter>().professions.AddFishing(1);
                break;
            case TalentTreeType.Exploration:
                interactingPlayer.GetComponent<PlayerCharacter>().professions.AddExploration(1);
                break;
            default:
                break;
        }
        GetComponent<CanDropItem>().SpawnItemsInInventory(FindObjectOfType<InventoryManager>(true));
        CmdSetInteractingPlayer(null);
        currentCharges--;
        remainingChargesText.gameObject.SetActive(true);
        remainingChargesText.text = currentCharges + "/" + maxCharges;
        if (currentCharges <= 0)
        {
            if (Quest_Event)
                Quest_Event.voidEvent.Invoke();
            remainingChargesText.gameObject.SetActive(false);
            CmdUpdateLootability(false);
            if (!oneTimeLoot)
                CmdStartRefreshTimer();
            else if (TryGetComponent(out ObjectMapIcon mapIcon))
                mapIcon.DestroyIcon();
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
        if (anim)
            anim.SetTrigger("Destroy");
        else
            CmdDestroyOnServer();
    }
    public void CmdDestroyOnServer()
    {
        Object_Destroyed.Invoke(this);
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
        if (effectsToHide.Count > 0)
            foreach (var item in effectsToHide)
            {
                if (item != null)
                    item.SetActive(lootable);
            }
        GetComponent<Collider>().enabled = lootable;
        var players = FindObjectsOfType<PlayerController>();
        foreach (var item in players)
        {
            item.RemoveCollider(GetComponent<Collider>());
        }
        var outline = GetComponentInChildren<HighlightTrigger>();
        if (outline)
        {
            outline.highlightEffect.highlighted = false;
            outline.enabled = lootable;
        }
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

    private IEnumerator StartRefreshTimer(bool isContinuing = false)
    {
        refreshProgressBar.gameObject.SetActive(true);
        refreshProgressBar.maxValue = refreshDuration;
        if (!isContinuing)
            refreshTimer = refreshDuration;
        while (refreshTimer > 0)
        {
            refreshTimer -= Time.deltaTime;
            refreshProgressBar.value = refreshTimer;
            yield return null;
        }
        CmdUpdateLootability(true);
        refreshProgressBar.gameObject.SetActive(false);
        currentCharges = maxCharges;
    }

    public void SetLocalPlayerCharacter(PlayerCharacter player)
    {
        switch (professionRequired)
        {
            case TalentTreeType.Special:
                tooltip.objectName = lootableName;
                break;
            case TalentTreeType.Gathering:
                player.professions.Gathering_Changed.AddListener(UpdateTooltip);
                player.professions.AddGathering(0);
                break;
            case TalentTreeType.Cooking:
                player.professions.Cooking_Changed.AddListener(UpdateTooltip);
                player.professions.AddCooking(0);
                break;
            case TalentTreeType.Alchemy:
                player.professions.Alchemy_Changed.AddListener(UpdateTooltip);
                player.professions.AddAlchemy(0);
                break;
            case TalentTreeType.Fishing:
                player.professions.Fishing_Changed.AddListener(UpdateTooltip);
                player.professions.AddFishing(0);
                break;
            case TalentTreeType.Exploration:
                player.professions.Exploration_Changed.AddListener(UpdateTooltip);
                player.professions.AddExploration(0);
                break;
            default:
                break;
        }
    }
    private void UpdateTooltip(int profession)
    {
        if (profession >= professionExperienceRequired)
        {
            tooltip.objectName = lootableName + " <color=green>(" + professionExperienceRequired + ")";
        }
        else
        {
            tooltip.objectName = lootableName + " <color=red>(" + professionExperienceRequired + ")";
        }
    }

    public SaveDataWorldObject SaveState()
    {
        if (!lootable && oneTimeLoot)
            return null;
        return new SaveDataWorldObject
        {
             intData1 = currentCharges,
             boolData1 = lootable,
             floatData1 = refreshTimer
        };
    }

    public void LoadState(SaveDataWorldObject state)
    {
        if (state == null)
            if (isServer)
            {
                CmdDestroyOnServer();
                return;
            }
            else
                return;
        CmdUpdateLootability(state.boolData1);
        currentCharges = state.intData1;
        if (currentCharges <= 0 && state.floatData1 > 0)
        {
            Debug.Log(refreshTimer);
            refreshTimer = state.floatData1;
            if (refreshTimer > 0)
                StartCoroutine(StartRefreshTimer(true));
        }
    }
}
