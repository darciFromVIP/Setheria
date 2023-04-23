using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TalentTreeType
{
    Combat
}
[System.Serializable]
public class TalentTree
{
    public TalentTreeType talentTreeType;
    public List<Talent> talents = new();
    public int talentPointsSpent;
    public TalentTree() { }
    public TalentTree(TalentTreeType treeType, List<TalentScriptable> talents)
    {
        talentTreeType = treeType;
        foreach (var item in talents)
        {
            this.talents.Add(new Talent(item.talentType));
        }
    }
    public void ChangeTalentPointsSpent(int value)
    {
        talentPointsSpent += value;
    }
}
