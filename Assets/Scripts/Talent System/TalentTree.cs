using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TalentTreeType
{
    CombatForestProtector, CombatLycandruid
}
[System.Serializable]
public class TalentTree
{
    public TalentTreeType talentTreeType;
    public List<Talent> talents = new();

    public TalentTree() { }
    public TalentTree(TalentTreeType treeType, List<TalentScriptable> talents)
    {
        talentTreeType = treeType;
        foreach (var item in talents)
        {
            this.talents.Add(new Talent(item.talentType));
        }
    }
}
