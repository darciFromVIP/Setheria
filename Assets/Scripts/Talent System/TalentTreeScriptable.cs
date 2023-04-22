using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Talent System/Talent Tree")]
public class TalentTreeScriptable : ScriptableObject
{
    public TalentTreeType treeType;
    public List<TalentScriptable> talents;
}
