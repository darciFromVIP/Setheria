using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class TalentButton : MonoBehaviour
{
    public TalentScriptable talent;
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Button btn;
    private void Start()
    {
        btn.onClick.AddListener(UnlockTalent);
        talent.Talent_Description_Updated.AddListener(UpdateTalentDescription);
        UpdateTalentDescription();
    }
    public void UpdateTalentDescription()
    {
        string talentDescription = talent.description;
        if (talent.requiredTalent || talent.requiredTalentPointsSpent > 0 || talent.requiredPlayerLevel > 0)
            talentDescription += "\n\n";
        if (talent.requiredPlayerLevel > 0)
            talentDescription += "Required Player Level: " + talent.requiredPlayerLevel;
        if (talent.requiredTalent)
            talentDescription += "\nRequired Talent: " + talent.requiredTalent.label + " of level " + talent.requiredTalentLevel;
        if (talent.requiredTalentPointsSpent > 0)
            talentDescription += "\nRequired Talent Points spent: " + talent.requiredTalentPointsSpent;
        if (talent.excludingTalents.Count > 0)
        {
            talentDescription += "\nExcludes these talents: ";
            for (int i = 0; i < talent.excludingTalents.Count; i++)
            {
                talentDescription += talent.excludingTalents[i].label;
                if (talent.excludingTalents.Count - i > 1)
                    talentDescription += "\n";
            }
        }
        GetComponent<TooltipTrigger>().SetText(talent.label, talentDescription, image.sprite);
    }
    public void UpdateButton(Talent currentTalent, TalentTrees playerTalentTrees, TalentTree talentTree)
    {
        var localPlayer = FindObjectOfType<GameManager>().localPlayerCharacter;
        levelText.text = currentTalent.currentLevel + "/" + talent.maxLevel;
        if (currentTalent.currentLevel == 0 && (talentTree.talentPointsSpent < talent.requiredTalentPointsSpent 
            || (talentTree.talentTreeType == TalentTreeType.Combat ? playerTalentTrees.combatTalentPoints <= 0 : playerTalentTrees.professionTalentPoints <= 0)
            || localPlayer.level < talent.requiredPlayerLevel) )
        {
            image.color = new Color(0.25f, 0.25f, 0.25f);
            btn.interactable = false;
        }
        else if (currentTalent.currentLevel == 0 && (playerTalentTrees.IsTalentUnlocked(talent.requiredTalent, talent.requiredTalentLevel) >= talent.requiredTalentLevel))
        {
            image.color = new Color(0.75f, 0.75f, 0.75f);
            btn.interactable = true;
        }
        else if (currentTalent.currentLevel <= 0)
        {
            image.color = new Color(0.25f, 0.25f, 0.25f);
            btn.interactable = false;
        }
        if (currentTalent.currentLevel >= 1)
            image.color = Color.white;
        if (currentTalent.currentLevel == talent.maxLevel)
            btn.interactable = false;
        foreach (var item in talent.excludingTalents)
        {
            if (playerTalentTrees.IsTalentUnlocked(item, 1) >= 1)
            {
                image.color = new Color(0.25f, 0.25f, 0.25f);
                btn.interactable = false;
            }
        }
    }
    private void UnlockTalent()
    {
        GetComponentInParent<TalentScreen>().UnlockTalent(talent);
    }
}
