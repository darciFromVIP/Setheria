using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static UnityEditor.Progress;

public enum QuestRewardType
{
    Resources, Knowledge, XP, Item
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
    private bool active;
    public string label;
    public List<ItemRecipeInfo> requiredItems = new();
    [HideInInspector] public Dictionary<ItemScriptable, int> requiredItemsDic = new();
    public List<StructureScriptable> requiredStructures = new();
    [HideInInspector] public Dictionary<StructureScriptable, bool> requiredStructuresDic = new();
    public int requiredResources;
    [HideInInspector] public int currentResources;
    public int requiredKnowledge;
    [HideInInspector] public int currentKnowledge;
    public List<QuestReward> rewards;

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
        if (active)
        {
            var gm = FindObjectOfType<GameManager>();
            gm.Knowledge_Added.AddListener(ReduceKnowledgeRequirement);
            gm.Resources_Added.AddListener(ReduceResourceRequirement);
            currentResources = gm.GetResources();
            currentKnowledge = gm.GetKnowledge();
            questManager = FindObjectOfType<QuestManager>();
            foreach (var item in requiredItems)
            {
                item.itemData.Item_Stacks_Acquired.AddListener(CmdReduceItemRequirement);
                item.itemData.Item_Stacks_Lost.AddListener(CmdIncreaseItemRequirement);
                requiredItemsDic.Add(item.itemData, 0);
            }
            foreach (var item in requiredStructures)
            {
                item.Structure_Built.AddListener(CmdReduceStructureRequirement);
                requiredStructuresDic.Add(item, false);
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
            requiredStructuresDic[structureBuilt] = true;
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
            requiredItemsDic[itemAcquired] += stacks;
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
            requiredItemsDic[itemLost] -= stacks;
            if (requiredItemsDic[itemLost] < 0)
                requiredItemsDic[itemLost] = 0;
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
    private void CheckQuestCompletion()
    {
        Quest_Updated.Invoke();
        foreach (var item in requiredItems)
        {
            if (requiredItemsDic[item.itemData] < item.stacks)
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
            bool isCompleted = requiredItemsDic[item.itemData] >= item.stacks;
            if (isCompleted)
                result += "<s>";
            result += "Collect " + item.itemData.name + " " + requiredItemsDic[item.itemData] + "/" + item.stacks + "\n";
            if (isCompleted)
                result += "</s>";
        }
        foreach (var item in requiredStructures)
        {
            bool isCompleted = requiredStructuresDic[item] == true;
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
                default:
                    break;
            }
            result += item.rewardAmount;
            if (rewards.IndexOf(item) != rewards.Count - 1)
                result += ", ";
        }
        return result;
    }
}
