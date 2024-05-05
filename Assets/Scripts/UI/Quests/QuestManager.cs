using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestManager : NetworkBehaviour
{
    public List<QuestlineScriptable> questlines = new();
    public List<QuestlineScriptable> beginnerQuestlines = new();
    public Transform contentUI;
    public QuestDescription questDescriptionPrefab;
    public QuestlineDatabase questlineDatabase;
    public ItemScriptableDatabase itemDatabase;
    private void Start()
    {
        if (isClient)
            LoadStateSynchronized(FindObjectOfType<WorldGenerator>().lastLoadedWorldState.syncedQuestlines);
    }
    [Command(requiresAuthority = false)] 
    private void CmdNewQuest(string questName)
    {
        RpcNewQuest(questName);
    }
    [ClientRpc]
    private void RpcNewQuest(string questName)
    {
        NewQuest(questName);
    }
    private void NewQuest(string questName)
    {
        NewQuestReturn(questName);
    }
    private QuestScriptable NewQuestReturn(string questName)
    {
        QuestScriptable quest = null;
        foreach (var item in questlines)
        {
            foreach (var item2 in item.questList)
            {
                if (item2.name == questName)
                    quest = item2;
            }
        }
        quest.SetQuestActive(true);
        var questInstance = Instantiate(questDescriptionPrefab, contentUI);
        questInstance.Initialize(quest);
        int count = 0;
        foreach (var item in contentUI.GetComponentsInChildren<QuestDescription>(true))
        {
            if (item.gameObject.activeSelf)
                count++;
        }
        if (count > 4)
            ToggleQuestTracking(quest, false);
        return quest;
    }
    [Command(requiresAuthority = false)]
    public void CmdReduceItemRequirement(string questName, string itemName, int stacks)
    {
        RpcReduceItemRequirement(questName, itemName, stacks);
    }
    [ClientRpc]
    private void RpcReduceItemRequirement(string questName, string itemName, int stacks)
    {
        ReduceItemRequirement(questName, itemName, stacks);
    }
    public void ReduceItemRequirement(string questName, string itemName, int stacks)
    {
        QuestlineScriptable[] temp = new QuestlineScriptable[questlines.Count];
        questlines.CopyTo(temp);
        foreach (var item in temp)
        {
            foreach (var item2 in item.questList)
            {
                if (item2.name == questName)
                {
                    item2.ReduceItemRequirement(itemName, stacks);
                }
            }
        }
    }
    [Command(requiresAuthority = false)]
    public void CmdReduceStructureRequirement(string questName, string structureName)
    {
        RpcReduceStructureRequirement(questName, structureName);
    }
    [ClientRpc]
    private void RpcReduceStructureRequirement(string questName, string structureName)
    {
        ReduceStructureRequirement(questName, structureName);
    }
    public void ReduceStructureRequirement(string questName, string structureName)
    {
        QuestlineScriptable[] temp = new QuestlineScriptable[questlines.Count];
        questlines.CopyTo(temp);
        foreach (var item in temp)
        {
            foreach (var item2 in item.questList)
            {
                if (item2.name == questName)
                {
                    item2.ReduceStructureRequirement(structureName);
                }
            }
        }
    }
    [Command(requiresAuthority = false)]
    public void CmdIncreaseItemRequirement(string questName, string itemName, int stacks)
    {
        RpcIncreaseItemRequirement(questName, itemName, stacks);
    }
    [ClientRpc]
    private void RpcIncreaseItemRequirement(string questName, string itemName, int stacks)
    {
        IncreaseItemRequirement(questName, itemName, stacks);
    }
    public void IncreaseItemRequirement(string questName, string itemName, int stacks)
    {
        QuestlineScriptable[] temp = new QuestlineScriptable[questlines.Count];
        questlines.CopyTo(temp);
        foreach (var item in temp)
        {
            foreach (var item2 in item.questList)
            {
                if (item2.name == questName)
                {
                    item2.IncreaseItemRequirement(itemName, stacks);
                }
            }
        }
    }
    [Command(requiresAuthority = false)]
    public void CmdReduceCustomRequirement(string questName, string identifierString, int amount)
    {
        RpcReduceCustomRequirement(questName, identifierString, amount);
    }
    [ClientRpc]
    private void RpcReduceCustomRequirement(string questName, string identifierString, int amount)
    {
        ReduceCustomRequirement(questName, identifierString, amount);
    }
    public void ReduceCustomRequirement(string questName, string identifierString, int amount)
    {
        QuestlineScriptable[] temp = new QuestlineScriptable[questlines.Count];
        questlines.CopyTo(temp);
        foreach (var item in temp)
        {
            foreach (var item2 in item.questList)
            {
                if (item2.name == questName)
                {
                    foreach (var item3 in item2.customRequirements)
                    {
                        if (item3.requirementText == identifierString)
                            item3.ReduceCustomRequirement(amount);
                    }
                }
            }
        }
    }
    [Command(requiresAuthority = false)]
    private void CmdQuestComplete(string questName)
    {
        RpcQuestComplete(questName);
    }
    [ClientRpc]
    private void RpcQuestComplete(string questName)
    {
        QuestComplete(questName);
    }
    private void QuestComplete(string questName)
    {
        QuestScriptable quest = null;
        QuestlineScriptable[] temp = new QuestlineScriptable[questlines.Count];
        questlines.CopyTo(temp);
        foreach (var item in temp)
        {
            foreach (var item2 in item.questList)
            {
                if (item2.name == questName)
                    quest = item2;
            }
        }
        if (quest == null)
        {
            return;
        }
        foreach (var item in quest.rewards)
        {
            switch (item.rewardType)
            {
                case QuestRewardType.Resources:
                    if (isServer)
                        FindObjectOfType<GameManager>().ChangeResources(item.rewardAmount);
                    break;
                case QuestRewardType.Knowledge:
                    if (isServer)
                        FindObjectOfType<GameManager>().ChangeKnowledge(item.rewardAmount);
                    break;
                case QuestRewardType.XP:
                    FindObjectOfType<GameManager>().localPlayerCharacter.CmdAddXp(item.rewardAmount);
                    break;
                case QuestRewardType.Item:
                    FindObjectOfType<InventoryManager>(true).AddItem(item.itemReward, item.rewardAmount);
                    break;
                default:
                    break;
            }
        }
        foreach (var item in FindObjectsOfType<QuestDescription>(true))
        {
            if (item.questData == quest)
            {
                item.QuestCompleted();
                break;
            }
        }
    }
    [Command(requiresAuthority = false)]
    private void CmdQuestlineComplete(string questlineName)
    {
        RpcQuestlineComplete(questlineName);
    }
    [ClientRpc]
    private void RpcQuestlineComplete(string questlineName)
    {
        QuestlineComplete(questlineName);
    }
    private void QuestlineComplete(string questlineName)
    {
        QuestlineScriptable temp = null;
        foreach (var item in questlines)
        {
            if (item.name == questlineName)
                temp = item;
        }
        if (temp is not null)
        {
            if (temp.synchronized)
            {
                if (isServer)
                {
                    temp.Quest_Complete.RemoveListener(RpcQuestComplete);
                    temp.Questline_Complete.RemoveListener(RpcQuestlineComplete);
                    temp.New_Quest.RemoveListener(RpcNewQuest);
                }
            }
            else
            {
                temp.Quest_Complete.RemoveListener(QuestComplete);
                temp.Questline_Complete.RemoveListener(QuestlineComplete);
                temp.New_Quest.RemoveListener(NewQuest);
            }
            questlines.Remove(temp);
        }
    }
    [Command(requiresAuthority = false)]
    public void CmdNewQuestline(string questlineName)
    {
        RpcNewQuestline(questlineName);
    }
    [ClientRpc]
    private void RpcNewQuestline(string questlineName)
    {
        NewQuestline(questlineName);
    }
    public QuestScriptable NewQuestline(string questlineName, int questIndex = 0)
    {
        QuestlineScriptable questline = questlineDatabase.GetQuestlineByName(questlineName);
        questlines.Add(questline);
        questline.LoadQuestline(questIndex);
        if (questline.synchronized)
        {
            if (isServer)
            {
                questline.Quest_Complete.AddListener(RpcQuestComplete);
                questline.Questline_Complete.AddListener(RpcQuestlineComplete);
                questline.New_Quest.AddListener(RpcNewQuest);
            }
        }
        else
        {
            questline.Quest_Complete.AddListener(QuestComplete);
            questline.Questline_Complete.AddListener(QuestlineComplete);
            questline.New_Quest.AddListener(NewQuest);
        }
        
        return NewQuestReturn(questline.questList[questIndex].name);
    }
    public List<QuestlineSaveable> SaveStateSynchronized()
    {
        List<QuestlineSaveable> result = new();
        foreach (var item in questlines)
        {
            if (item.synchronized)
            {
                result.Add(SaveQuestline(item));
            }
        }
        return result;
    }
    public void LoadStateSynchronized(List<QuestlineSaveable> saveData)
    {
        if (saveData != null)
        {
            foreach (var item in saveData)
            {
                if (item.synchronized)
                {
                    LoadQuestline(item);
                }
            }
        }
        else
        {
            foreach (var item in beginnerQuestlines)
            {
                NewQuestline(item.name);
            }
        }
    }
    public List<QuestlineSaveable> SaveStateUnsynchronized()
    {
        List<QuestlineSaveable> result = new();
        foreach (var item in questlines)
        {
            if (!item.synchronized)
            {
                result.Add(SaveQuestline(item));
            }
        }
        return result;
    }
    public void LoadStateUnsynchronized(List<QuestlineSaveable> saveData)
    {
        if (saveData != null)
        {
            foreach (var item in saveData)
            {
                if (!item.synchronized)
                {
                    LoadQuestline(item);
                }
            }
        }
    }
    private QuestlineSaveable SaveQuestline(QuestlineScriptable item)
    {
        List<string> names = new();
        List<int> values = new();
        foreach (var item2 in item.questList[item.currentQuestIndex].requiredStructuresDic)
        {
            names.Add(item2.Key);
            values.Add(item2.Value ? 1 : 0);
        }
        foreach (var item2 in item.questList[item.currentQuestIndex].customRequirements)
        {
            names.Add(item2.requirementText);
            values.Add(item2.currentValue);
        }
        names.Add("Resources");
        values.Add(item.questList[item.currentQuestIndex].currentResources);
        names.Add("Knowledge");
        values.Add(item.questList[item.currentQuestIndex].currentKnowledge);
        return new QuestlineSaveable { questlineName = item.name, currentQuestIndex = item.currentQuestIndex, questRequirementsNames = names, questRequirementsValues = values, synchronized = item.synchronized };
    }
    private void LoadQuestline(QuestlineSaveable item)
    {
        var quest = NewQuestline(item.questlineName, item.currentQuestIndex);
        for (int i = 0; i < item.questRequirementsNames.Count; i++)
        {
            foreach (var item3 in quest.requiredStructuresDic.ToList())
            {
                if (item.questRequirementsNames[i] == item3.Key)
                {
                    bool value;
                    if (item.questRequirementsValues[i] == 0)
                        value = false;
                    else
                        value = true;
                    quest.requiredStructuresDic[item3.Key] = value;
                }
            }
            foreach (var item3 in quest.customRequirements)
            {
                if (item.questRequirementsNames[i] == item3.requirementText)
                {
                    item3.currentValue = item.questRequirementsValues[i];
                }
            }
            if (item.questRequirementsNames[i] == "Knowledge")
                quest.currentKnowledge = item.questRequirementsValues[i];
            if (item.questRequirementsNames[i] == "Resources")
                quest.currentResources = item.questRequirementsValues[i];
        }
        quest.Quest_Updated.Invoke();
    }
    public void ToggleQuestTracking(QuestScriptable questData, bool value)
    {
        foreach (var item in contentUI.GetComponentsInChildren<QuestDescription>(true))
        {
            if (item.questData == questData)
            {
                item.ToggleTracking(value);
                Debug.Log(item.name + " is " + value);
            }
        }
    }
    public bool GetQuestTracking(QuestScriptable questData)
    {
        foreach (var item in contentUI.GetComponentsInChildren<QuestDescription>(true))
        {
            if (item.questData == questData)
                return item.gameObject.activeSelf;
        }
        return false;
    }
}
