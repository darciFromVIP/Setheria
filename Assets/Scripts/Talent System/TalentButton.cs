using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class TalentButton : MonoBehaviour
{
    public TalentScriptable talent;
    private Image image;
    private TextMeshProUGUI levelText;
    private Button btn;
    private void Awake()
    {
        btn = GetComponent<Button>();
        image = GetComponent<Image>();
        levelText = GetComponentInChildren<TextMeshProUGUI>();
        btn.onClick.AddListener(UnlockTalent);
    }
    public void UpdateButton(Talent currentTalent, TalentTrees playerTalentTrees, TalentTree talentTree)
    {
        levelText.text = currentTalent.currentLevel + "/" + talent.maxLevel;
        if (currentTalent.currentLevel == 0 && (talentTree.talentPointsSpent < talent.requiredTalentPointsSpent || playerTalentTrees.talentPoints <= 0))
        {
            image.color = new Color(0.25f, 0.25f, 0.25f);
            btn.interactable = false;
        }
        else if (currentTalent.currentLevel == 0 && playerTalentTrees.IsTalentUnlocked(talent.requiredTalent, talent.requiredTalentLevel))
        {
            image.color = new Color(0.75f, 0.75f, 0.75f);
            btn.interactable = true;
        }
        if (currentTalent.currentLevel >= 1)
            image.color = Color.white;
        if (currentTalent.currentLevel == talent.maxLevel)
            btn.interactable = false;
    }
    private void UnlockTalent()
    {
        GetComponentInParent<TalentScreen>().UnlockTalent(talent);
    }
}
