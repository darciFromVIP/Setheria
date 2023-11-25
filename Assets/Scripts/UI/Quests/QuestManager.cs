using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestManager : NetworkBehaviour
{
    private List<QuestlineScriptable> questlines = new();
    public List<QuestlineScriptable> beginnerQuestlines = new();
    public Transform contentUI;
    public QuestDescription questDescriptionPrefab;
    public QuestlineDatabase questlineDatabase;
    public ItemScriptableDatabase itemDatabase;
    private void Start()
    {
        if (isClient)
            LoadState(FindObjectOfType<WorldGenerator>().lastLoadedWorldState.questlines);
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
    public void CmdReduceCustom1Requirement(string questName, int amount)
    {
        RpcReduceCustom1Requirement(questName, amount);
    }
    [ClientRpc]
    private void RpcReduceCustom1Requirement(string questName, int amount)
    {
        ReduceCustom1Requirement(questName, amount);
    }
    public void ReduceCustom1Requirement(string questName, int amount)
    {
        QuestlineScriptable[] temp = new QuestlineScriptable[questlines.Count];
        questlines.CopyTo(temp);
        foreach (var item in temp)
        {
            foreach (var item2 in item.questList)
            {
                if (item2.name == questName)
                {
                    item2.ReduceCustom1Requirement(amount);
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
    public List<QuestlineSaveable> SaveState()
    {
        List<QuestlineSaveable> result = new();
        foreach (var item in questlines)
        {
            List<string> names = new();
            List<int> values = new();
            foreach (var item2 in item.questList[item.currentQuestIndex].requiredItemsDic)
            {
                names.Add(item2.Key);
                values.Add(item2.Value);
            }
            foreach (var item2 in item.questList[item.currentQuestIndex].requiredStructuresDic)
            {
                names.Add(item2.Key);
                values.Add(item2.Value ? 1 : 0);
            }
            result.Add(new QuestlineSaveable { questlineName = item.name, currentQuestIndex = item.currentQuestIndex, questRequirementsNames = names, questRequirementsValues = values });
        }
        return result;
    }
    public void LoadState(List<QuestlineSaveable> saveData)
    {
        if (saveData != null)
        {
            foreach (var item in saveData)
            {
                var quest = NewQuestline(item.questlineName, item.currentQuestIndex);
                for (int i = 0; i < item.questRequirementsNames.Count; i++)
                {
                    foreach (var item3 in quest.requiredItemsDic.ToList())
                    {
                        if (item.questRequirementsNames[i] == item3.Key)
                        {
                            quest.requiredItemsDic[item3.Key] = item.questRequirementsValues[i];
                        }
                    }
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
                }
                quest.Quest_Updated.Invoke();
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
}
