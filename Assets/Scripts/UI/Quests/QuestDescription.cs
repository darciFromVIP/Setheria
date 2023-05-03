using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class QuestDescription : MonoBehaviour
{
    public QuestScriptable questData;
    public TextMeshProUGUI label, objectives, rewards;

    public void Initialize(QuestScriptable data)
    {
        questData = data;
        questData.Quest_Updated.AddListener(UpdateUI);
        UpdateUI();
    }
    public void UpdateUI()
    {
        label.text = questData.label;
        objectives.text = questData.GetObjectivesText();
        rewards.text = questData.GetRewardsText();
    }
}
