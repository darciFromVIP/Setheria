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
public class QuestScriptable : ScriptableObject
{
    [HideInInspector] public bool active;
    [Tooltip("Name of the quest - this will be displayed in the game.")]
    public string label;
    [Tooltip("List of required items.")]
    public List<ItemRecipeInfo> requiredItems = new();
    [HideInInspector] public Dictionary<string, int> requiredItemsDic = new();
    [Tooltip("Any item of this type will count towards the count.")]
    public ItemType requiredItemType;
    [Tooltip("The count of required items of the same type.")]
    public int requiredItemTypeAmount;
    private int currentItemTypeAmount;
    private List<string> validItemTypeNames;
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
    public string custom1Requirement;

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
            gm.Knowledge_Added.AddListener(ReduceKnowledgeRequirement);
            gm.Resources_Added.AddListener(ReduceResourceRequirement);
            currentResources = gm.GetResources();
            currentKnowledge = gm.GetKnowledge();
            currentItemTypeAmount = 0;
            questManager = FindObjectOfType<QuestManager>();
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
            var gm = FindObjectOfType<GameManager>();
            gm.Knowledge_Added.RemoveListener(ReduceKnowledgeRequirement);
            gm.Resources_Added.RemoveListener(ReduceResourceRequirement);
            foreach (var item in requiredItems)
            {
                item.itemData.Item_Stacks_Acquired.RemoveListener(CmdReduceItemRequirement);
                item.itemData.Item_Stacks_Lost.RemoveListener(CmdIncreaseItemRequirement);
            }
            foreach (var item in requiredStructures)
            {
                item.Structure_Built.RemoveListener(CmdReduceStructureRequirement);
            }
        }
    }
    private void CmdReduceStructureRequirement(StructureScriptable structureBuilt)
    {
        questManager.CmdReduceStructureRequirement(name, structureBuilt.name);
    }
    private void CmdReduceItemRequirement(ItemScriptable itemAcquired, int stacks)
    {
        questManager.CmdReduceItemRequirement(name, itemAcquired.name, stacks);
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
            requiredItemsDic[itemAcquired.name] += stacks;
            CheckQuestCompletion();
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
}
