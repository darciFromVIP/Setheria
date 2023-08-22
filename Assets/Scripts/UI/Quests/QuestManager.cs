using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : NetworkBehaviour
{
    public List<QuestlineScriptable> questlines = new();
    public Transform contentUI;
    public QuestDescription questDescriptionPrefab;

    private void Start()
    {
        foreach (var item in questlines)
        {
            item.Quest_Complete.AddListener(CmdQuestComplete);
            item.Questline_Complete.AddListener(CmdQuestlineComplete);
            item.New_Quest.AddListener(CmdNewQuest);
            CmdNewQuest(item.quests[item.currentQuestIndex].name);
        }
    }
    [Command(requiresAuthority = false)] 
    private void CmdNewQuest(string questName)
    {
        RpcNewQuest(questName);
    }
    [ClientRpc]
    private void RpcNewQuest(string questName)
    {
        QuestScriptable quest = null;
        foreach (var item in questlines)
        {
            foreach (var item2 in item.quests)
            {
                if (item2.name == questName)
                    quest = item2;
            }
        }
        quest.SetQuestActive(true);
        var questInstance = Instantiate(questDescriptionPrefab, contentUI);
        questInstance.Initialize(quest);
    }
    [Command(requiresAuthority = false)]
    private void CmdQuestComplete(string questName)
    {
        RpcQuestComplete(questName);
    }
    [ClientRpc]
    private void RpcQuestComplete(string questName)
    {
        QuestScriptable quest = null;
        foreach (var item in questlines)
        {
            foreach (var item2 in item.quests)
            {
                if (item2.name == questName)
                    quest = item2;
            }
        }
        if (quest == null)
            return;
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
                    FindObjectOfType<GameManager>().localPlayerCharacter.AddXp(item.rewardAmount);
                    break;
                case QuestRewardType.GatheringXP:
                    FindObjectOfType<GameManager>().localPlayerCharacter.professions.AddGathering(item.rewardAmount);
                    break;
                case QuestRewardType.AlchemyXP:
                    FindObjectOfType<GameManager>().localPlayerCharacter.professions.AddAlchemy(item.rewardAmount);
                    break;
                case QuestRewardType.CookingXP:
                    FindObjectOfType<GameManager>().localPlayerCharacter.professions.AddCooking(item.rewardAmount);
                    break;
                case QuestRewardType.FishingXP:
                    FindObjectOfType<GameManager>().localPlayerCharacter.professions.AddFishing(item.rewardAmount);
                    break;
                default:
                    break;
            }
        }
        foreach (var item in contentUI.GetComponentsInChildren<QuestDescription>(true))
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
        QuestlineScriptable temp = null;
        foreach (var item in questlines)
        {
            if (item.name == questlineName)
                temp = item;
        }
        if (temp is not null)
            questlines.Remove(temp);
    }
    public void NewQuestline(QuestlineScriptable questline)
    {
        questlines.Add(questline);
        CmdNewQuest(questline.quests[questline.currentQuestIndex].name);
    }
}
