using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestManager : NetworkBehaviour
{
    public List<QuestlineScriptable> questlines = new();
    public Transform contentUI;
    public QuestDescription questDescriptionPrefab;

    private void Start()
    {
        foreach (var item in questlines)
        {
            item.Quest_Complete.AddListener(QuestComplete);
            item.Questline_Complete.AddListener(QuestlineComplete);
            item.New_Quest.AddListener(NewQuest);
            NewQuest(item.quests[item.currentQuestIndex]);
        }
    }
    private void NewQuest(QuestScriptable quest)
    {
        quest.SetQuestActive(true);
        var questInstance = Instantiate(questDescriptionPrefab, contentUI);
        questInstance.Initialize(quest);
        FindObjectOfType<AudioManager>().QuestAccepted();
    }
    private void QuestComplete(QuestScriptable quest)
    {
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
                Destroy(item.gameObject);
                break;
            }
        }
        FindObjectOfType<AudioManager>().QuestCompleted();
    }
    private void QuestlineComplete(QuestlineScriptable questline)
    {
        questlines.Remove(questline);
    }
    public void NewQuestline(QuestlineScriptable questline)
    {
        questlines.Add(questline);
        NewQuest(questline.quests[questline.currentQuestIndex]);
    }
}
