using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class TalentScreen : WindowWithCategories, NeedsLocalPlayerCharacter
{
    public List<GameObject> combatTalentTrees = new();
    public TextMeshProUGUI availablePoints, spentPoints;
    private List<TalentButton> talentButtons = new();

    private TalentTreeType currentOpenedTree = TalentTreeType.Special;

    public PlayerCharacter localPlayer;
    public void SetLocalPlayerCharacter(PlayerCharacter player)
    {
        localPlayer = player;
        if (combatTalentTrees.Count > 0)
            combatTalentTrees[(int)player.hero].SetActive(true);
        foreach (var item in GetComponentsInChildren<TalentButton>(true))
        {
            talentButtons.Add(item);
        }
    }
    public void UpdateTalents()
    {
        foreach (var item in talentButtons)
        {
            foreach (var item2 in localPlayer.talentTrees.talentTrees)
            {
                if (item2.talentTreeType == currentOpenedTree)
                {
                    availablePoints.text = "Available Talent Points: " + localPlayer.talentTrees.talentPoints;
                    spentPoints.text = "Spent Talent Points: " + item2.talentPointsSpent;
                }
                foreach (var item3 in item2.talents)
                {
                    if (item3.talentType == item.talent.talentType)
                    {
                        item.UpdateButton(item3, localPlayer.talentTrees, item2);
                    }
                }
            }
        }
    }
    public void UnlockTalent(TalentScriptable talent)
    {
        localPlayer.talentTrees.UnlockTalent(talent);
        localPlayer.UpdateSkills();
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
    public bool CanUpgradeThisTree(TalentTreeType type)
    {
        switch (type)
        {
            case TalentTreeType.Special:
                break;
            case TalentTreeType.Gathering:
                return localPlayer.professions.gathering == localPlayer.professions.maxGathering;
            case TalentTreeType.Cooking:
                return localPlayer.professions.cooking == localPlayer.professions.maxCooking;
            case TalentTreeType.Alchemy:
                return localPlayer.professions.alchemy == localPlayer.professions.maxAlchemy;
            case TalentTreeType.Fishing:
                return localPlayer.professions.fishing == localPlayer.professions.maxFishing;
            case TalentTreeType.Exploration:
                return localPlayer.professions.exploration == localPlayer.professions.maxExploration;
            default:
                break;
        }
        return false;
    }
}
