using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class QuestLogElement : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI label;
    public Toggle toggleTracking;

    public QuestScriptable questData;
    private QuestLog questLog;

    public void Initialize(QuestScriptable questData, bool tracked, QuestLog questLog)
    {
        toggleTracking.onValueChanged.AddListener(TrackingToggled);
        this.questData = questData;
        icon.sprite = questData.icon;
        label.text = questData.label;
        toggleTracking.isOn = tracked;
        this.questLog = questLog;
        GetComponent<Button>().onClick.AddListener(OnClick);
    }
    private void TrackingToggled(bool value)
    {
        FindObjectOfType<QuestManager>().ToggleQuestTracking(questData, value);
    }
    private void OnClick()
    {
        questLog.ShowQuestDetails(questData);
    }
}
