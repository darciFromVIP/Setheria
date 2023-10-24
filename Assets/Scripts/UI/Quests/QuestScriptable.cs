using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum QuestRewardType
{
    Resources, Knowledge, XP, Item, Unknown
}
[System.Serializable]
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
    [Tooltip("Does this quest progress across all players?")]
    public bool synchronizedQuest = true;
    [Tooltip("List of required items.")]
    public List<ItemRecipeInfo> requiredItems = new();
    [HideInInspector] public Dictionary<string, int> requiredItemsDic = new();
    [Tooltip("Any item of this type will count towards the count.")]
    public ItemType requiredItemType;
    [Tooltip("The count of required items of the same type.")]
    public int requiredItemTypeAmount;
    private int currentItemTypeAmount;
    private List<string> validItemTypeNames = new();
    [Tooltip("Structures required to be built.")]
    public List<StructureScriptable> requiredStructures = new();
    [HideInInspector] public Dictionary<string, bool> requiredStructuresDic = new();
    [Tooltip("Required resources to be collected.")]
    public int requiredResources;
    [HideInInspector] public int currentResources;
    [Tooltip("Required knowledge to be collected.")]
    public int requiredKnowledge;
    [HideInInspector] public int currentKnowledge;
    [Tooltip("Required custom value to be acquired by means coded specifically. Do not fill this unless you know what interacts with this.")]
    public int requiredCustom1;
    [HideInInspector] public int currentCustom1;
    [Tooltip("Text representation of the custom requirement (displayed in quest)")]
    public string custom1Requirement;
    [Tooltip("Event to listen to such as Fishing Quest Event")]
    public EventScriptable customEvent;

    [Tooltip("Quest rewards. Always fill only one reward per list element.")]
    public List<QuestReward> rewards;

    private QuestManager questManager;

    [HideInInspector] public UnityEvent Quest_Complete = new();
    [HideInInspector] public UnityEvent Quest_Updated = new();
    public void SetQuestActive(bool value)
    {
        active = value;
        requiredItemsDic.Clear();
        requiredStructuresDic.Clear();
        validItemTypeNames.Clear();
        currentResources = 0;
        currentKnowledge = 0;
        currentCustom1 = 0;
        if (active)
        {
            var gm = FindObjectOfType<GameManager>();
            questManager = FindObjectOfType<QuestManager>();

            currentItemTypeAmount = 0;

            if (requiredResources > 0)
                gm.Resources_Added.AddListener(ReduceResourceRequirement);

            if (requiredKnowledge > 0)
                gm.Knowledge_Added.AddListener(ReduceKnowledgeRequirement);

            if (synchronizedQuest)
            {
                if (customEvent)
                    customEvent.theEvent.AddListener(CmdReduceCustom1Requirement);
                foreach (var item in requiredItems)
                {
                    item.itemData.Item_Stacks_Acquired.AddListener(CmdReduceItemRequirement);
                    item.itemData.Item_Stacks_Lost.AddListener(CmdIncreaseItemRequirement);
                    requiredItemsDic.Add(item.itemData.name, 0);
                }
                foreach (var item in requiredStructures)
                {
                    item.Structure_Built.AddListener(CmdReduceStructureRequirement);
                    requiredStructuresDic.Add(item.name, false);
                }
                if (requiredItemType != ItemType.None)
                    foreach (var item in questManager.itemDatabase.items)
                    {
                        if (item.itemType == requiredItemType)
                        {
                            item.Item_Stacks_Acquired.AddListener(CmdReduceItemRequirement);
                            item.Item_Stacks_Lost.AddListener(CmdIncreaseItemRequirement);
                            validItemTypeNames.Add(item.name);
                        }
                    }
            }
            else
            {
                if (customEvent)
                    customEvent.theEvent.AddListener(ReduceCustom1Requirement);
                foreach (var item in requiredItems)
                {
                    item.itemData.Item_Stacks_Acquired.AddListener(ReduceItemRequirement);
                    item.itemData.Item_Stacks_Lost.AddListener(IncreaseItemRequirement);
                    requiredItemsDic.Add(item.itemData.name, 0);
                }
                foreach (var item in requiredStructures)
                {
                    item.Structure_Built.AddListener(ReduceStructureRequirement);
                    requiredStructuresDic.Add(item.name, false);
                }
                if (requiredItemType != ItemType.None)
                    foreach (var item in questManager.itemDatabase.items)
                    {
                        if (item.itemType == requiredItemType)
                        {
                            item.Item_Stacks_Acquired.AddListener(ReduceItemRequirement);
                            item.Item_Stacks_Lost.AddListener(IncreaseItemRequirement);
                            validItemTypeNames.Add(item.name);
                        }
                    }
            }
            if (requiredItemsDic.Count > 0)
            {
                var playerItems = FindObjectOfType<InventoryManager>().GetAllItems();
                List<KeyValuePair<string, int>> dicCopy = new();
                requiredItemsDic.CopyTo(dicCopy);
                foreach (var requiredItem in dicCopy)
                {
                    foreach (var playerItem in playerItems)
                    {
                        if (requiredItem.Key == playerItem.item.name)
                            requiredItemsDic[requiredItem.Key] += playerItem.stacks;
                    }
                }
                CheckQuestCompletion();
            }
        }
        else
        {
            var gm = FindObjectOfType<GameManager>();
            if (synchronizedQuest)
            {
                gm.Knowledge_Added.RemoveListener(ReduceKnowledgeRequirement);
                gm.Resources_Added.RemoveListener(ReduceResourceRequirement);
                if (customEvent)
                    customEvent.theEvent.RemoveListener(CmdReduceCustom1Requirement);
                foreach (var item in requiredItems)
                {
                    item.itemData.Item_Stacks_Acquired.RemoveListener(CmdReduceItemRequirement);
                    item.itemData.Item_Stacks_Lost.RemoveListener(CmdIncreaseItemRequirement);
                }
                foreach (var item in requiredStructures)
                {
                    item.Structure_Built.RemoveListener(CmdReduceStructureRequirement);
                }
                if (requiredItemType != ItemType.None)
                    foreach (var item in questManager.itemDatabase.items)
                    {
                        if (item.itemType == requiredItemType)
                        {
                            item.Item_Stacks_Acquired.RemoveListener(CmdReduceItemRequirement);
                            item.Item_Stacks_Lost.RemoveListener(CmdIncreaseItemRequirement);
                        }
                    }
            }
            else
            {
                if (customEvent)
                    customEvent.theEvent.RemoveListener(ReduceCustom1Requirement);
                foreach (var item in requiredItems)
                {
                    item.itemData.Item_Stacks_Acquired.RemoveListener(ReduceItemRequirement);
                    item.itemData.Item_Stacks_Lost.RemoveListener(IncreaseItemRequirement);
                }
                foreach (var item in requiredStructures)
                {
                    item.Structure_Built.RemoveListener(ReduceStructureRequirement);
                }
                if (requiredItemType != ItemType.None)
                    foreach (var item in questManager.itemDatabase.items)
                    {
                        if (item.itemType == requiredItemType)
                        {
                            item.Item_Stacks_Acquired.RemoveListener(ReduceItemRequirement);
                            item.Item_Stacks_Lost.RemoveListener(IncreaseItemRequirement);
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
        else if (validItemTypeNames.Contains(itemName))
        {
            currentItemTypeAmount += stacks;
            CheckQuestCompletion();
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
        else if (validItemTypeNames.Contains(itemName))
        {
            currentItemTypeAmount -= stacks;
            CheckQuestCompletion();
        }
    }
    private void ReduceResourceRequirement(int amount)
    {
        currentResources += amount;
        CheckQuestCompletion();
    }
    private void ReduceKnowledgeRequirement(int amount)
    {
        currentKnowledge += amount;
        CheckQuestCompletion();
    }
    private void CmdReduceCustom1Requirement()
    {
        questManager.CmdReduceCustom1Requirement(name, 1);
    }
    private void ReduceCustom1Requirement()
    {
        currentCustom1 += 1;
        CheckQuestCompletion();
    }
    private void CmdReduceCustom1Requirement(int amount)
    {
        questManager.CmdReduceCustom1Requirement(name, amount);
    }
    public void ReduceCustom1Requirement(int amount)
    {
        currentCustom1 += amount;
        CheckQuestCompletion();
    }
    public void AddCustom1Requirement(int amount)
    {
        currentCustom1 -= amount;
        CheckQuestCompletion();
    }
    private void CheckQuestCompletion()
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
        if (currentCustom1 < requiredCustom1)
            return;
        if (currentItemTypeAmount < requiredItemTypeAmount)
            return;
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
            result += "Collect " + item.itemData.name + " " + requiredItemsDic[item.itemData.name] + "/" + item.stacks + "\n";
            if (isCompleted)
                result += "</s>";
        }
        if (requiredItemTypeAmount > 0)
        {
            bool isCompleted = currentItemTypeAmount >= requiredItemTypeAmount;
            if (isCompleted)
                result += "<s>";
            result += "Collect any " + requiredItemType.ToString() + " " + currentItemTypeAmount + "/" + requiredItemTypeAmount + "\n";
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
        if (requiredCustom1 > 0)
        {
            bool isCompleted = currentCustom1 >= requiredCustom1;
            if (isCompleted)
                result += "<s>";
            result += custom1Requirement + currentCustom1 + "/" + requiredCustom1 + "\n";
            if (isCompleted)
                result += "</s>";
        }
        return result;
    }
    public string GetRewardsText()
    {
        string result = "Rewards: ";
        foreach (var item in rewards)
        {
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
                if (rewards.IndexOf(item) != rewards.Count - 1)
                    result += ", ";
            }
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

