using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class QuestDescription : MonoBehaviour
{
    public QuestScriptable questData;
    public TextMeshProUGUI label, objectives, rewards;

    private Transform parent;

    public void Initialize(QuestScriptable data)
    {
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
        transform.SetParent(parent);
        FindObjectOfType<AudioManager>().QuestAccepted();
    }
    public void QuestCompleted()
    {
        FindObjectOfType<AudioManager>().QuestCompleted();
        GetComponent<Animator>().SetTrigger("Completed");
    }
    public void QuestCompletedAnimationComplete()
    {
        Destroy(gameObject);
    }
    public void UpdateUI()
    {
        label.text = questData.label;
        objectives.text = questData.GetObjectivesText();
        rewards.text = questData.GetRewardsText();
    }
}
