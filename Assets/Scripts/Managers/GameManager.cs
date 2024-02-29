using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using TMPro;
using UnityEngine.Events;
using Steamworks;
using System;

public class GameManager : NetworkBehaviour, NeedsLocalPlayerCharacter
{
    public TextMeshProUGUI resourcesText, knowledgeText;
    [SyncVar(hook = nameof(ResourceHook))]
    [SerializeField]
    private int resources = 0;
    [SyncVar(hook = nameof(KnowledgeHook))]
    [SerializeField]
    private int knowledge = 0;

    private bool inputEnabled = true;

    public UnityEvent<float> Healing_Potions_Cooldown = new();
    public UnityEvent<float> Mana_Potions_Cooldown = new();
    public UnityEvent<float> Corruption_Potions_Cooldown = new();
    public UnityEvent<int> Resources_Added = new();
    public UnityEvent<int> Knowledge_Added = new();
    public EventScriptable Player_Event;

    
    public RecipeDatabase recipeDatabase;
    public PlayerCharacter localPlayerCharacter;
    public void SetLocalPlayerCharacter(PlayerCharacter player)
    {
        localPlayerCharacter = player;
        Player_Event.playerEvent.Invoke(player);
    }
    public void SaveGame()
    {
        FindObjectOfType<NetworkedSaveLoad>().CmdSave();
    }
    public void LoadGame()
    {
        FindObjectOfType<NetworkedSaveLoad>().CmdLoad();
    }
    public void ExitToMainMenu()
    {
        FindObjectOfType<InventoryManager>().DestroyAllItems();
        SceneManager.LoadSceneAsync("Main Menu", LoadSceneMode.Single);
    }
    void ResourceHook(int oldValue, int newValue)
    {
        resourcesText.text = "<sprite=15>" + newValue.ToString();
    }
    void KnowledgeHook(int oldValue, int newValue)
    {
        knowledgeText.text = "<sprite=11>" + newValue.ToString();
    }
    public void ChangeResources(int value)
    {
        CmdChangeResources(value);
    }
    [Command(requiresAuthority = false)]
    private void CmdChangeResources(int value)
    {
        resources += value;
        if (resources < 0)
            resources = 0;
        RpcChangeResources(value);
    }
    [ClientRpc]
    private void RpcChangeResources(int value)
    {
        Resources_Added.Invoke(value);
    }
    public void ChangeKnowledge(int value)
    {
        CmdChangeKnowledge(value);
    }
    [Command(requiresAuthority = false)]
    private void CmdChangeKnowledge(int value)
    {
        knowledge += value;
        if (knowledge < 0)
            knowledge = 0;
        RpcChangeKnowledge(value);
    }
    [ClientRpc]
    private void RpcChangeKnowledge(int value)
    {
        Knowledge_Added.Invoke(value);
    }
    public bool TestSubtractResources(int value)
    {
        if (resources - value < 0)
            return false;
        else
            return true;
    }
    public bool TestSubtractKnowledge(int value)
    {
        if (knowledge - value < 0)
            return false;
        else
            return true;
    }
    public int GetResources()
    {
        return resources;
    }
    public int GetKnowledge()
    {
        return knowledge;
    }
    public void StartCooldown(CooldownGroup group, float cd)
    {
        switch (group)
        {
            case CooldownGroup.HealingPotions:
                StartCoroutine(CooldownCoroHealingPotions(cd));
                break;
            case CooldownGroup.ManaPotions:
                StartCoroutine(CooldownCoroManaPotions(cd));
                break;
            case CooldownGroup.CorruptionPotions:
                StartCoroutine(CooldownCoroCorruptionPotions(cd));
                break;
            default:
                break;
        }
    }
    public void StartIndependentCooldown(UnityAction<float> updateFunc, float cd)
    {
        StartCoroutine(StartCooldown(updateFunc, cd));
    }
    private IEnumerator StartCooldown(UnityAction<float> updateFunc, float cd)
    {
        float timer = cd;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            updateFunc.Invoke(timer);
            yield return null;
        }
    }
    private IEnumerator CooldownCoroHealingPotions(float cd)
    {
        float timer = cd;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            Healing_Potions_Cooldown.Invoke(timer);
            yield return null;
        }
    }
    private IEnumerator CooldownCoroManaPotions(float cd)
    {
        float timer = cd;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            Mana_Potions_Cooldown.Invoke(timer);
            yield return null;
        }
    }
    private IEnumerator CooldownCoroCorruptionPotions(float cd)
    {
        float timer = cd;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            Corruption_Potions_Cooldown.Invoke(timer);
            yield return null;
        }
    }
    public void InviteAFriend()
    {
        SteamFriends.ActivateGameOverlay("friends");
    }
    [Command(requiresAuthority = false)]
    public void CmdUnlockRecipe(string recipeName)
    {
        RpcUnlockRecipe(recipeName);
    }
    [ClientRpc]
    private void RpcUnlockRecipe(string recipeName)
    {
        var recipe = recipeDatabase.GetRecipeByName(recipeName);
        recipe.UnlockRecipe();
        foreach (var item in recipe.componentItems)
        {
            item.itemData.unlocked = true;
        }
    }
    public void DisableInput()
    {
        inputEnabled = false;
    }
    public void EnableInput()
    {
        inputEnabled = true;
    }
    public bool GetInput()
    {
        return inputEnabled;
    }
}
