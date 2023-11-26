using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum TalentType
{
    None, GreenDust, Rejuvenation, EntanglingRoots, FlowerPower, SwipeAndBite, UppercutAndPounce, RoarAndBattlecry, WildRageAndCallOfTheWild
}
[CreateAssetMenu(menuName = "Talent System/Talent")]
public class TalentScriptable : ScriptableObject
{
    public TalentType talentType;
    public byte maxLevel;
    public TalentScriptable requiredTalent;
    public byte requiredTalentLevel;
    public byte requiredTalentPointsSpent;
    public string label;
    [TextArea(10, 10)]
    public string description;
    [Tooltip("If this talent unlocks a skill, assign it here to get its description with scalings etc. Leave Description blank in this case")]
    public Skill associatedSkill;
    [Tooltip("2nd skill for Shapeshifters")]
    public Skill associatedSkill2;
    private string skillDescription;
    private string skillDescription2;

    public UnityEvent Talent_Description_Updated = new();
    private void OnEnable()
    {
        if (associatedSkill)
        {
            associatedSkill.Description_Updated.RemoveListener(UpdateSkill1Description);
            associatedSkill.Description_Updated.AddListener(UpdateSkill1Description);
        }
        if (associatedSkill2)
        {
            associatedSkill2.Description_Updated.RemoveListener(UpdateSkill2Description);
            associatedSkill2.Description_Updated.AddListener(UpdateSkill2Description);
        }
    }
    private void UpdateSkill1Description(string desc)
    {
        skillDescription = desc;
        UpdateSkillDescription();
    }
    private void UpdateSkill2Description(string desc)
    {
        skillDescription2 = desc;
        UpdateSkillDescription();
    }
    private void UpdateSkillDescription()
    {
        if (skillDescription2 != "")
            description = skillDescription + "\n\n" + skillDescription2;
        else
            description = skillDescription;
        Talent_Description_Updated.Invoke();
    }
}
