using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class TalentScreen : WindowWithCategories, NeedsLocalPlayerCharacter
{
    public TextMeshProUGUI availablePoints, spentPoints;
    private List<TalentButton> talentButtons = new();

    private TalentTreeType currentOpenedTree = TalentTreeType.Combat;

    public PlayerCharacter localPlayer;
    public void SetLocalPlayerCharacter(PlayerCharacter player)
    {
        localPlayer = player; 
        foreach (var item in GetComponentsInChildren<TalentButton>(true))
        {
            talentButtons.Add(item);
        }
    }
    private void OnEnable()
    {
        UpdateTalents();
    }
    public void UpdateTalents()
    {
        foreach (var item in talentButtons)
        {
            foreach (var item2 in localPlayer.talentTrees.talentTrees)
            {
                if (item2.talentTreeType == currentOpenedTree)
                {
                    if (currentOpenedTree == TalentTreeType.Combat)
                    {
                        availablePoints.text = "Available <sprite=21>: " + localPlayer.talentTrees.combatTalentPoints;
                        spentPoints.text = "Spent <sprite=21>: " + item2.talentPointsSpent;
                    }
                    else
                    {
                        availablePoints.text = "Available <sprite=22>: " + localPlayer.talentTrees.professionTalentPoints;
                        spentPoints.text = "Spent <sprite=22>: " + item2.talentPointsSpent;
                    }
                }
                foreach (var item3 in item2.talents)
                {
                    if (item3.name == item.talent.label)
                    {
                        item.UpdateButton(item3, localPlayer.talentTrees, item2);
                    }
                }
            }
        }
    }
    public void UnlockTalent(TalentScriptable talent)
    {
        if ((talent.talentType == TalentType.Combat && localPlayer.talentTrees.combatTalentPoints <= 0) ||
            (talent.talentType == TalentType.Profession && localPlayer.talentTrees.professionTalentPoints <= 0))
            return;

        localPlayer.talentTrees.UnlockTalent(talent, localPlayer);
        UpdateTalents();
    }
    public override void OpenAnotherWindow(GameObject window)
    {
        base.OpenAnotherWindow(window);
        UpdateTalents();
    }
    public void ChangeCurrentOpenedTalentTree(TalentTreeType type)
    {
        currentOpenedTree = type;
    }
}
