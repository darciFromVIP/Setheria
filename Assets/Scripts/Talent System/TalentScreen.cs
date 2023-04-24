using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class TalentScreen : MonoBehaviour, NeedsLocalPlayerCharacter
{
    public GameObject window;
    public List<GameObject> combatTalentTrees = new();
    public TextMeshProUGUI availablePoints, spentPoints;
    private List<TalentButton> talentButtons = new();

    public GameObject currentOpenedWindow;
    private TalentTreeType currentOpenedTree = TalentTreeType.Special;

    private PlayerCharacter localPlayer;
    public void SetLocalPlayerCharacter(PlayerCharacter player)
    {
        localPlayer = player;
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
    public void OpenAnotherWindow(GameObject window, TalentTreeType treeType)
    {
        if (currentOpenedWindow)
            currentOpenedWindow.SetActive(false);
        window.SetActive(true);
        currentOpenedWindow = window;
        UpdateTalents();
    }
}
