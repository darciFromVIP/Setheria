using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TalentType
{
    None, GreenDust, Rejuvenation, EntanglingRoots
}
[CreateAssetMenu(menuName = "Talent System/Talent")]
public class TalentScriptable : ScriptableObject
{
    public TalentType talentType;
    public byte maxLevel;
    public TalentScriptable requiredTalent;
    public byte requiredTalentLevel;
}
