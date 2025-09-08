using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class TalentTrees
{
    public List<TalentTree> talentTrees = new();
    public int combatTalentPoints;
    public int professionTalentPoints;
    [NonSerialized] public UnityEvent<int> Combat_Talent_Points_Changed = new();
    [NonSerialized] public UnityEvent<int> Profession_Talent_Points_Changed = new();

    public byte IsTalentUnlocked(TalentScriptable talent, byte level)
    {
        if (talent == null)
            return 0;
        foreach (var item in talentTrees)
        {
            foreach (var item2 in item.talents)
            {
                if (item2.name == talent.label)
                {
                    if (item2.currentLevel >= level)
                        return item2.currentLevel;
                }
            }
        }
        return 0;
    }
    public byte IsTalentUnlocked(string talent, byte level)
    {
        if (talent == "")
            return 0;
        foreach (var item in talentTrees)
        {
            foreach (var item2 in item.talents)
            {
                if (item2.name == talent)
                {
                    if (item2.currentLevel >= level)
                        return item2.currentLevel;
                }
            }
        }
        return 0;
    }
    public void UnlockTalent(TalentScriptable talent, PlayerCharacter player)
    {
        foreach (var item in talentTrees)
        {
            foreach (var item2 in item.talents)
            {
                if (item2.name == talent.label)
                {
                    item2.IncreaseCurrentLevel(player);
                    if (talent.talentType == TalentType.Combat)
                        ChangeCombatTalentPoints(-1);
                    else
                        ChangeProfessionTalentPoints(-1);
                    item.ChangeTalentPointsSpent(1);
                }
            }
        }
    }
    public void RefundTalentPoints(TalentTreeType treeType, PlayerCharacter player)
    {
        foreach (var item in talentTrees)
        {
            if (item.talentTreeType == treeType && item.talentPointsSpent > 0)
            {
                foreach (var item2 in item.talents)
                {
                    Debug.Log(item2.name);
                    item2.ResetLevel(player);
                }
                if (treeType == TalentTreeType.Combat)
                    combatTalentPoints += item.talentPointsSpent;
                else
                    professionTalentPoints += item.talentPointsSpent;
                item.talentPointsSpent = 0;
            }
        }
    }
    public void ChangeCombatTalentPoints(int value)
    {
        combatTalentPoints += value;
        if (combatTalentPoints < 0)
            combatTalentPoints = 0;
        Combat_Talent_Points_Changed.Invoke(combatTalentPoints);
    }
    public void ChangeProfessionTalentPoints(int value)
    {
        professionTalentPoints += value;
        if (professionTalentPoints < 0)
            professionTalentPoints = 0;
        Profession_Talent_Points_Changed.Invoke(professionTalentPoints);
    }
}
