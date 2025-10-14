using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuestDescription : MonoBehaviour
{
    public QuestScriptable questData;
    public TextMeshProUGUI label, objectives, rewards;

    public bool complete = false;

    public void Initialize(QuestScriptable data)
    {
        questData = data;
        questData.Quest_Updated.AddListener(UpdateUI);
        UpdateUI();
        QuestAccepted();
    }
    public void QuestAccepted()
    {
        FindObjectOfType<AudioManager>().QuestAccepted();
    }
    public void QuestCompleted()
    {
        FindObjectOfType<AudioManager>().QuestCompleted();
        if (gameObject.activeInHierarchy)
            GetComponent<Animator>().SetTrigger("Completed");
        else
            QuestCompletedAnimationComplete();
        complete = true;
    }
    public void QuestCompletedAnimationComplete()
    {
        Destroy(gameObject);
    }
    public void UpdateUI()
    {
        if (questData.active)
        {
            label.text = questData.label;
            objectives.text = questData.GetObjectivesText();
            rewards.text = questData.GetRewardsText(true);
        }
        else
        {
            label.text = questData.label;
            objectives.text = "Complete!";
            rewards.text = questData.GetRewardsText(true);
            QuestCompleted();
        }
    }
    public void ToggleCollapse()
    {
        objectives.gameObject.SetActive(!objectives.gameObject.activeSelf);
        rewards.gameObject.SetActive(!rewards.gameObject.activeSelf);
    }
    public void ToggleTracking()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
    public void ToggleTracking(bool value)
    {
        gameObject.SetActive(value);
    }
}
