using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuestDescription : MonoBehaviour
{
    public QuestScriptable questData;
    public TextMeshProUGUI label, objectives, rewards;

    public Material mainQuestMaterial, loreQuestMaterial;
    public bool complete = false;

    private Transform parent;
    public void Initialize(QuestScriptable data)
    {
        switch (data.questType)
        {
            case QuestType.Main:
                GetComponent<Image>().material = mainQuestMaterial;
                break;
            case QuestType.Side:
                break;
            case QuestType.Lore:
                GetComponent<Image>().material = loreQuestMaterial;
                break;
            default:
                break;
        }
        questData = data;
        questData.Quest_Updated.AddListener(UpdateUI);
        UpdateUI();
        StartCoroutine(DelayedParentNull());
    }
    private IEnumerator DelayedParentNull()
    {
        yield return new WaitForEndOfFrame();
        parent = transform.parent;
        transform.SetParent(parent.parent);
    }
    public void QuestAcceptedAnimationComplete()
    {
        if (parent)
            transform.SetParent(parent);
        FindObjectOfType<AudioManager>().QuestAccepted();
        if (questData.questType == QuestType.Main)
            transform.SetAsFirstSibling();
    }
    public void QuestCompleted()
    {
        FindObjectOfType<AudioManager>().QuestCompleted();
        GetComponent<Animator>().SetTrigger("Completed");
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
