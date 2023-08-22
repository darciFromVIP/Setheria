using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public enum QuestRewardType
{
    Resources, Knowledge, XP, GatheringXP, AlchemyXP, CookingXP, FishingXP
}
[System.Serializable]
public struct QuestReward
{
    public QuestRewardType rewardType;
    public int rewardAmount;
}
[CreateAssetMenu(menuName = "Quest System/Quest")]
public class QuestScriptable : ScriptableObject
{
    private bool active;
    public string label;
    public List<ItemRecipeInfo> requiredItems = new();
    private Dictionary<ItemScriptable, int> requiredItemsDic = new();
    public List<StructureScriptable> requiredStructures = new();
    private Dictionary<StructureScriptable, bool> requiredStructuresDic = new();
    public List<QuestReward> rewards;

    [HideInInspector] public UnityEvent Quest_Complete = new();
    [HideInInspector] public UnityEvent Quest_Updated = new();
    public void SetQuestActive(bool value)
    {
        active = value;
        requiredItemsDic.Clear();
        requiredStructuresDic.Clear();
        if (active)
        {
            foreach (var item in requiredItems)
            {
                item.itemData.Item_Stacks_Acquired.AddListener(ReduceRequirement);
                item.itemData.Item_Stacks_Lost.AddListener(AddRequirement);
                requiredItemsDic.Add(item.itemData, 0);
            }
            foreach (var item in requiredStructures)
            {
                item.Structure_Built.AddListener(ReduceRequirement);
                requiredStructuresDic.Add(item, false);
            }
        }
        else
        {
            foreach (var item in requiredItems)
            {
                item.itemData.Item_Stacks_Acquired.RemoveListener(ReduceRequirement);
                item.itemData.Item_Stacks_Lost.RemoveListener(AddRequirement);
            }
            foreach (var item in requiredStructures)
            {
                item.Structure_Built.RemoveListener(ReduceRequirement);
            }
        }
    }
    private void ReduceRequirement(StructureScriptable structureBuilt)
    {
        requiredStructuresDic[structureBuilt] = true;
        CheckQuestCompletion();
    }
    private void ReduceRequirement(ItemScriptable itemAcquired, int stacks)
    {
        requiredItemsDic[itemAcquired] += stacks;
        CheckQuestCompletion();
    }
    private void AddRequirement(ItemScriptable itemLost, int stacks)
    {
        requiredItemsDic[itemLost] -= stacks;
        if (requiredItemsDic[itemLost] < 0)
            requiredItemsDic[itemLost] = 0;
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
                case QuestRewardType.GatheringXP:
                    result += "Gathering +";
                    break;
                case QuestRewardType.AlchemyXP:
                    result += "Alchemy +";
                    break;
                case QuestRewardType.CookingXP:
                    result += "Cooking +";
                    break;
                case QuestRewardType.FishingXP:
                    result += "Fishing +";
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
