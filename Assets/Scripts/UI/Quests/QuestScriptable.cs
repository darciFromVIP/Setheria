﻿using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum QuestRewardType
{
    Resources, Knowledge, XP, Item, Unknown
}
public enum QuestType
{
    Main, Side, Lore
}
[Serializable]
public class RequiredItemType
{
    [Tooltip("Any item of this type will count towards the count.")]
    public ItemType requiredItemType;
    [Tooltip("The count of required items of the same type.")]
    public int requiredItemTypeAmount;
    [HideInInspector] public int currentItemTypeAmount;
    [HideInInspector] public List<string> validItemTypeNames = new();
}
[Serializable]
public class CustomRequirement
{
    [Tooltip("Required custom value to be acquired by means coded specifically.")]
    public int requiredValue;
    [HideInInspector] public int currentValue;
    [Tooltip("Text representation of the custom requirement (displayed in quest)")]
    public string requirementText;
    [Tooltip("Events to listen to such as Fishing Quest Event")]
    public List<EventScriptable> customEvents;

    private QuestManager questManager;
    private QuestScriptable questScriptable;

    public void SetUpReferences(QuestManager questManager, QuestScriptable questScriptable)
    {
        this.questScriptable = questScriptable;
        this.questManager = questManager;
    }
    public void CmdReduceCustomRequirement()
    {
        questManager.CmdReduceCustomRequirement(questScriptable.name, requirementText, 1);
    }
    public void ReduceCustomRequirement()
    {
        currentValue += 1;
        questScriptable.CheckQuestCompletion();
    }
    public void CmdReduceCustomRequirement(int amount)
    {
        questManager.CmdReduceCustomRequirement(questScriptable.name, requirementText, amount);
    }
    public void ReduceCustomRequirement(int amount)
    {
        currentValue += amount;
        questScriptable.CheckQuestCompletion();
    }
    public void AddCustomRequirement(int amount)
    {
        currentValue -= amount;
        questScriptable.CheckQuestCompletion();
    }
}
[Serializable]
public struct QuestReward
{
    public QuestRewardType rewardType;
    public int rewardAmount;
    public ItemScriptable itemReward;
}
[CreateAssetMenu(menuName = "Quest System/Quest")]
public class QuestScriptable : ScriptableObject, IComparable
{
    [HideInInspector] public bool active;
    [Tooltip("Name of the quest - this will be displayed in the game.")]
    public string label;
    [TextArea(6, 6)]
    [Tooltip("Description shown in the quest log.")]
    public string description;
    [Tooltip("Icon shown in the quest log.")]
    public Sprite icon;
    public QuestType questType;
    [Tooltip("Does this quest progress across all players?")]
    public bool synchronizedQuest = true;
    [Tooltip("List of required items.")]
    public List<ItemRecipeInfo> requiredItems = new();
    [HideInInspector] public Dictionary<string, int> requiredItemsDic = new();
    [Tooltip("List of required item types.")]
    public List<RequiredItemType> requiredItemTypes = new();
    [Tooltip("Structures required to be built.")]
    public List<StructureScriptable> requiredStructures = new();
    [HideInInspector] public Dictionary<string, bool> requiredStructuresDic = new();
    [Tooltip("Required resources to be collected.")]
    public int requiredResources;
    [HideInInspector] public int currentResources;
    [Tooltip("Required knowledge to be collected.")]
    public int requiredKnowledge;
    [HideInInspector] public int currentKnowledge;
    [Tooltip("Custom requirements such as lootable objects looted, monsters killed or story events.")]
    public List<CustomRequirement> customRequirements = new();

    [Tooltip("Quest rewards. Always fill only one reward per list element.")]
    public List<QuestReward> rewards;
    [Tooltip("Which recipes will unlock upon quest completion.")]
    public List<RecipeScriptable> recipesToUnlock = new();

    [Tooltip("Does this quest introduce new mechanic? Add the corresponding tutorial data here.")]
    public TutorialDataScriptable tutorialToShow;

    private QuestManager questManager;

    [HideInInspector] public UnityEvent Quest_Complete = new();
    [HideInInspector] public UnityEvent Quest_Updated = new();
    public void SetQuestActive(bool value)
    {
        active = value;
        requiredItemsDic.Clear();
        requiredStructuresDic.Clear();
        currentResources = 0;
        currentKnowledge = 0;
        foreach (var item in customRequirements)
        {
            item.currentValue = 0;
        }
        foreach (var item in requiredItemTypes)
        {
            item.currentItemTypeAmount = 0;
            item.validItemTypeNames.Clear();
        }
        if (active)
        {
            var gm = FindObjectOfType<GameManager>();
            questManager = FindObjectOfType<QuestManager>();

            foreach (var item in customRequirements)
            {
                item.SetUpReferences(questManager, this);
            }

            if (requiredResources > 0)
            {
                gm.Resources_Added.RemoveListener(ReduceResourceRequirement);
                gm.Resources_Added.AddListener(ReduceResourceRequirement);
            }

            if (requiredKnowledge > 0)
            {
                gm.Knowledge_Added.RemoveListener(ReduceKnowledgeRequirement);
                gm.Knowledge_Added.AddListener(ReduceKnowledgeRequirement);
            }

            if (synchronizedQuest)
            {
                foreach (var item in customRequirements)
                {
                    foreach (var item2 in item.customEvents)
                    {
                        item2.voidEvent.RemoveListener(item.CmdReduceCustomRequirement);
                        item2.voidEvent.AddListener(item.CmdReduceCustomRequirement);
                    }
                }
                foreach (var item in requiredItems)
                {
                    item.itemData.Item_Stacks_Acquired.RemoveListener(CmdReduceItemRequirement);
                    item.itemData.Item_Stacks_Lost.RemoveListener(CmdIncreaseItemRequirement);
                    item.itemData.Item_Stacks_Acquired.AddListener(CmdReduceItemRequirement);
                    item.itemData.Item_Stacks_Lost.AddListener(CmdIncreaseItemRequirement);
                    requiredItemsDic.Add(item.itemData.name, 0);
                }
                foreach (var item in requiredStructures)
                {
                    item.Structure_Built.RemoveListener(CmdReduceStructureRequirement);
                    item.Structure_Built.AddListener(CmdReduceStructureRequirement);
                    requiredStructuresDic.Add(item.name, false);
                }
                foreach (var item2 in requiredItemTypes)
                {
                    foreach (var item in questManager.itemDatabase.items)
                    {
                        if (item.itemType == item2.requiredItemType)
                        {
                            item.Item_Stacks_Acquired.RemoveListener(CmdReduceItemRequirement);
                            item.Item_Stacks_Lost.RemoveListener(CmdIncreaseItemRequirement);
                            item.Item_Stacks_Acquired.AddListener(CmdReduceItemRequirement);
                            item.Item_Stacks_Lost.AddListener(CmdIncreaseItemRequirement);
                            item2.validItemTypeNames.Add(item.name);
                        }
                    }
                }                    
            }
            else
            {
                foreach (var item in customRequirements)
                {
                    foreach (var item2 in item.customEvents)
                    {
                        item2.voidEvent.RemoveListener(item.ReduceCustomRequirement);
                        item2.voidEvent.AddListener(item.ReduceCustomRequirement);
                    }
                }
                foreach (var item in requiredItems)
                {
                    item.itemData.Item_Stacks_Acquired.RemoveListener(ReduceItemRequirement);
                    item.itemData.Item_Stacks_Lost.RemoveListener(IncreaseItemRequirement);
                    item.itemData.Item_Stacks_Acquired.AddListener(ReduceItemRequirement);
                    item.itemData.Item_Stacks_Lost.AddListener(IncreaseItemRequirement);
                    requiredItemsDic.Add(item.itemData.name, 0);
                }
                foreach (var item in requiredStructures)
                {
                    item.Structure_Built.RemoveListener(ReduceStructureRequirement);
                    item.Structure_Built.AddListener(ReduceStructureRequirement);
                    requiredStructuresDic.Add(item.name, false);
                }
                foreach (var item2 in requiredItemTypes)
                {
                    foreach (var item in questManager.itemDatabase.items)
                    {
                        if (item.itemType == item2.requiredItemType)
                        {
                            item.Item_Stacks_Acquired.RemoveListener(ReduceItemRequirement);
                            item.Item_Stacks_Lost.RemoveListener(IncreaseItemRequirement);
                            item.Item_Stacks_Acquired.AddListener(ReduceItemRequirement);
                            item.Item_Stacks_Lost.AddListener(IncreaseItemRequirement);
                            item2.validItemTypeNames.Add(item.name);
                        }
                    }
                }
            }
            var playerItems = FindObjectOfType<InventoryManager>(true).GetAllItems();
            if (requiredItemsDic.Count > 0)
            {
                foreach (var requiredItem in requiredItemsDic)
                {
                    foreach (var playerItem in playerItems)
                    {
                        if (requiredItem.Key == playerItem.item.name)
                        {
                            if (synchronizedQuest)
                                CmdReduceItemRequirement(playerItem.item, playerItem.stacks);
                            else
                                ReduceItemRequirement(playerItem.item, playerItem.stacks);
                        }
                    }
                }
            }
            foreach (var playerItem in playerItems)
            {
                foreach (var item in requiredItemTypes)
                {
                    if (item.validItemTypeNames.Contains(playerItem.item.name))
                    {
                        if (synchronizedQuest)
                            CmdReduceItemRequirement(playerItem.item, playerItem.stacks);
                        else
                            ReduceItemRequirement(playerItem.item, playerItem.stacks);
                    }
                }
            }
            var gear = FindObjectOfType<CharacterScreen>(true).GetEquippedGear();
            foreach (var piece in gear)
            {
                foreach (var item in requiredItemTypes)
                {
                    if (item.validItemTypeNames.Contains(piece.item.name))
                    {
                        if (synchronizedQuest)
                            CmdReduceItemRequirement(piece.item, piece.stacks);
                        else
                            ReduceItemRequirement(piece.item, piece.stacks);
                    }
                }
            }
            CheckQuestCompletion();
            if (tutorialToShow != null)
                FindObjectOfType<Tutorial>().QueueNewTutorial(tutorialToShow);
        }
        else
        {
            var gm = FindObjectOfType<GameManager>();
            if (synchronizedQuest)
            {
                gm.Knowledge_Added.RemoveListener(ReduceKnowledgeRequirement);
                gm.Resources_Added.RemoveListener(ReduceResourceRequirement);
                foreach (var item in customRequirements)
                {
                    foreach (var item2 in item.customEvents)
                    {
                        item2.voidEvent.RemoveListener(item.CmdReduceCustomRequirement);
                    }
                }
                foreach (var item in requiredItems)
                {
                    item.itemData.Item_Stacks_Acquired.RemoveListener(CmdReduceItemRequirement);
                    item.itemData.Item_Stacks_Lost.RemoveListener(CmdIncreaseItemRequirement);
                }
                foreach (var item in requiredStructures)
                {
                    item.Structure_Built.RemoveListener(CmdReduceStructureRequirement);
                }
                foreach (var item2 in requiredItemTypes)
                {
                    foreach (var item in questManager.itemDatabase.items)
                    {
                        if (item.itemType == item2.requiredItemType)
                        {
                            item.Item_Stacks_Acquired.RemoveListener(CmdReduceItemRequirement);
                            item.Item_Stacks_Lost.RemoveListener(CmdIncreaseItemRequirement);
                        }
                    }
                }
            }
            else
            {
                foreach (var item in customRequirements)
                {
                    foreach (var item2 in item.customEvents)
                    {
                        item2.voidEvent.RemoveListener(item.ReduceCustomRequirement);
                    }
                }
                foreach (var item in requiredItems)
                {
                    item.itemData.Item_Stacks_Acquired.RemoveListener(ReduceItemRequirement);
                    item.itemData.Item_Stacks_Lost.RemoveListener(IncreaseItemRequirement);
                }
                foreach (var item in requiredStructures)
                {
                    item.Structure_Built.RemoveListener(ReduceStructureRequirement);
                }
                foreach (var item2 in requiredItemTypes)
                {
                    foreach (var item in questManager.itemDatabase.items)
                    {
                        if (item.itemType == item2.requiredItemType)
                        {
                            item.Item_Stacks_Acquired.RemoveListener(ReduceItemRequirement);
                            item.Item_Stacks_Lost.RemoveListener(IncreaseItemRequirement);
                        }
                    }
                }
            }
        }
    }
    private void CmdReduceItemRequirement(ItemScriptable itemAcquired, int stacks)
    {
        questManager.CmdReduceItemRequirement(name, itemAcquired.name, stacks);
    }
    private void ReduceItemRequirement(ItemScriptable itemAcquired, int stacks)
    {
        questManager.ReduceItemRequirement(name, itemAcquired.name, stacks);
    }
    private void CmdReduceStructureRequirement(StructureScriptable structureBuilt)
    {
        questManager.CmdReduceStructureRequirement(name, structureBuilt.name);
    }
    public void ReduceStructureRequirement(StructureScriptable structureBuilt)
    {
        questManager.ReduceStructureRequirement(name, structureBuilt.name);
    }
    public void ReduceStructureRequirement(string structureName)
    {
        StructureScriptable structureBuilt = null;
        foreach (var item in requiredStructures)
        {
            if (item.name == structureName)
                structureBuilt = item;
        }
        if (structureBuilt != null)
        {
            requiredStructuresDic[structureBuilt.name] = true;
            CheckQuestCompletion();
        }
    }
    public void ReduceItemRequirement(string itemName, int stacks)
    {
        ItemScriptable itemAcquired = null;
        foreach (var item in requiredItems)
        {
            if (item.itemData.name == itemName)
                itemAcquired = item.itemData;
        }
        if (itemAcquired != null)
        {
            if (requiredItemsDic.ContainsKey(itemAcquired.name))
            {
                requiredItemsDic[itemAcquired.name] += stacks;
                CheckQuestCompletion();
            }
        }
        else
        {
            foreach (var item in requiredItemTypes)
            {
                if (item.validItemTypeNames.Contains(itemName))
                {
                    item.currentItemTypeAmount += stacks;
                    CheckQuestCompletion();
                }
            }
        }
    }
    private void CmdIncreaseItemRequirement(ItemScriptable itemLost, int stacks)
    {
        questManager.CmdIncreaseItemRequirement(name, itemLost.name, stacks);
    }
    private void IncreaseItemRequirement(ItemScriptable itemLost, int stacks)
    {
        questManager.IncreaseItemRequirement(name, itemLost.name, stacks);
    }
    public void IncreaseItemRequirement(string itemName, int stacks)
    {
        ItemScriptable itemLost = null;
        foreach (var item in requiredItems)
        {
            if (item.itemData.name == itemName)
                itemLost = item.itemData;
        }
        if (itemLost != null)
        {
            requiredItemsDic[itemLost.name] -= stacks;
            if (requiredItemsDic[itemLost.name] < 0)
                requiredItemsDic[itemLost.name] = 0;
            CheckQuestCompletion();
        }
        else
        {
            foreach (var item in requiredItemTypes)
            {
                if (item.validItemTypeNames.Contains(itemName))
                {
                    item.currentItemTypeAmount -= stacks;
                    CheckQuestCompletion();
                }
            }
        }
    }
    private void ReduceResourceRequirement(int amount)
    {
        if (amount > 0)
            currentResources += amount;
        CheckQuestCompletion();
    }
    private void ReduceKnowledgeRequirement(int amount)
    {
        if (amount > 0)
            currentKnowledge += amount;
        CheckQuestCompletion();
    }
    public void CheckQuestCompletion()
    {
        Quest_Updated.Invoke();
        foreach (var item in requiredItems)
        {
            if (requiredItemsDic[item.itemData.name] < item.stacks)
                return;
        }
        foreach (var item in requiredStructuresDic)
        {
            if (item.Value == false)
                return;
        }
        if (currentKnowledge < requiredKnowledge)
            return;
        if (currentResources < requiredResources)
            return;
        foreach (var item in customRequirements)
        {
            if (item.currentValue < item.requiredValue)
                return;
        }
        foreach (var item in requiredItemTypes)
        {
            if (item.currentItemTypeAmount < item.requiredItemTypeAmount)
                return;
        }
        QuestComplete();
    }
    public void QuestComplete()
    {
        SetQuestActive(false);
        Quest_Complete.Invoke();
    }
    public string GetObjectivesText()
    {
        string result = "";
        foreach (var item in requiredItems)
        {
            bool isCompleted = requiredItemsDic[item.itemData.name] >= item.stacks;
            if (isCompleted)
                result += "<s>";
            result += "Obtain " + item.itemData.name + " " + requiredItemsDic[item.itemData.name] + "/" + item.stacks + "\n";
            if (isCompleted)
                result += "</s>";
        }
        foreach (var item in requiredItemTypes)
        {
            bool isCompleted = item.currentItemTypeAmount >= item.requiredItemTypeAmount;
            if (isCompleted)
                result += "<s>";
            result += "Obtain any " + item.requiredItemType.ToString() + " " + item.currentItemTypeAmount + "/" + item.requiredItemTypeAmount + "\n";
            if (isCompleted)
                result += "</s>";
        }
        foreach (var item in requiredStructures)
        {
            bool isCompleted = requiredStructuresDic[item.name] == true;
            if (isCompleted)
                result += "<s>";
            result += "Build a " + item.name + ".\n";
            if (isCompleted)
                result += "</s>";
        }
        if (requiredResources > 0)
        {
            bool isCompleted = currentResources >= requiredResources;
            if (isCompleted)
                result += "<s>";
            result += "Gather Resources " + currentResources + "/" + requiredResources + "\n";
            if (isCompleted)
                result += "</s>";
        }
        if (requiredKnowledge > 0)
        {
            bool isCompleted = currentKnowledge >= requiredKnowledge;
            if (isCompleted)
                result += "<s>";
            result += "Gather Knowledge " + currentKnowledge + "/" + requiredKnowledge + "\n";
            if (isCompleted)
                result += "</s>";
        }
        foreach (var item in customRequirements)
        {
            bool isCompleted = item.currentValue >= item.requiredValue;
            if (isCompleted)
                result += "<s>";
            result += item.requirementText + item.currentValue + "/" + item.requiredValue + "\n";
            if (isCompleted)
                result += "</s>";
        }
        return result;
    }
    public string GetRewardsText(bool withCommas)
    {
        string result = "Rewards: ";
        if (!withCommas)
            result += "\n";
        foreach (var item in rewards)
        {
            if (!withCommas)
                result += "■ ";
            switch (item.rewardType)
            {
                case QuestRewardType.Resources:
                    result += "<sprite=15>";
                    break;
                case QuestRewardType.Knowledge:
                    result += "<sprite=11>";
                    break;
                case QuestRewardType.XP:
                    result += "<sprite=14>";
                    break;
                case QuestRewardType.Item:
                    result += item.itemReward.name + " x";
                    break;
                case QuestRewardType.Unknown:
                    result += "Unknown";
                    break;
                default:
                    break;
            }
            if (item.rewardType != QuestRewardType.Unknown)
            {
                result += item.rewardAmount;
                if (rewards.IndexOf(item) != rewards.Count - 1 || recipesToUnlock.Count > 0)
                    if (withCommas)
                        result += ", ";
                    else
                        result += "\n";
            }
        }
        foreach (var item in recipesToUnlock)
        {
            if (!withCommas)
                result += "■ ";
            result += "Recipe: " + item.name;
            if (recipesToUnlock.IndexOf(item) != recipesToUnlock.Count - 1)
                if (withCommas)
                    result += ", ";
                else
                    result += "\n";
        }
        return result;
    }

    public int CompareTo(object obj)
    {
        if (obj == null)
        {
            // If 'other' is null, then this object is greater.
            return 1;
        }

        // Split the names into parts based on numbers.
        string[] thisParts = SplitName(name);
        string[] otherParts = SplitName(((QuestScriptable)obj).name);

        // Compare each part of the names.
        int minParts = Math.Min(thisParts.Length, otherParts.Length);
        for (int i = 0; i < minParts; i++)
        {
            int thisPartNumber, otherPartNumber;
            bool thisIsNumber = int.TryParse(thisParts[i], out thisPartNumber);
            bool otherIsNumber = int.TryParse(otherParts[i], out otherPartNumber);

            // If both parts are numbers, compare them as integers.
            if (thisIsNumber && otherIsNumber)
            {
                int result = thisPartNumber.CompareTo(otherPartNumber);
                if (result != 0)
                {
                    return result;
                }
            }
            else
            {
                // If one part is a number and the other is not, compare them as strings.
                int result = thisParts[i].CompareTo(otherParts[i]);
                if (result != 0)
                {
                    return result;
                }
            }
        }

        // If all common parts are equal, the longer name is greater.
        return thisParts.Length.CompareTo(otherParts.Length);
    }

    // Helper method to split the name into parts based on numbers.
    private string[] SplitName(string name)
    {
        return System.Text.RegularExpressions.Regex.Split(name, "([0-9]+)");
    }
}

