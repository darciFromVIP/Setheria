using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class TalentTrees
{
    public List<TalentTree> talentTrees = new();
    public int talentPoints;
    [NonSerialized] public UnityEvent<int> Talent_Points_Changed = new();

    public bool IsTalentUnlocked(TalentScriptable talent, byte level)
    {
        if (talent == null)
            return true;
        foreach (var item in talentTrees)
        {
            foreach (var item2 in item.talents)
            {
                if (item2.name == talent.label)
                {
                    if (item2.currentLevel >= level)
                        return true;
                }
            }
        }
        return false;
    }
    public void UnlockTalent(TalentScriptable talent, PlayerCharacter player)
    {
        if (talentPoints <= 0)
            return;
        foreach (var item in talentTrees)
        {
            foreach (var item2 in item.talents)
            {
                if (item2.name == talent.label)
                {
                    item2.IncreaseCurrentLevel(player);
                    ChangeTalentPoints(-1);
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
                talentPoints += item.talentPointsSpent;
                item.talentPointsSpent = 0;
            }
        }
    }
    public void ChangeTalentPoints(int value)
    {
        talentPoints += value;
        if (talentPoints < 0)
            talentPoints = 0;
        Talent_Points_Changed.Invoke(talentPoints);
    }
}
