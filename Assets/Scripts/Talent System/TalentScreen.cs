using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class TalentScreen : MonoBehaviour, NeedsLocalPlayerCharacter
{
    public GameObject window;
    public List<GameObject> combatTalentTrees = new();
    private List<TalentButton> talentButtons = new();

    private PlayerCharacter localPlayer;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            window.SetActive(!window.activeSelf);
            UpdateTalents();
        }
    }
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
                foreach (var item3 in item2.talents)
                {
                    if (item3.talentType == item.talent.talentType)
                    {
                        item.UpdateButton(item3, localPlayer.talentTrees);
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
}
