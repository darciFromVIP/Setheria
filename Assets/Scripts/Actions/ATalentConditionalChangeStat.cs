using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Actions/Talent Conditional Change Stat")]
public class ATalentConditionalChangeStat : AChangeStat
{
    public TalentScriptable requiredTalent;
    public byte requiredTalentLevel = 1;
    [Tooltip("For example if you need to change HP by 20 per talent level (true) or just by 20 (false).")]
    public bool multiplyStatByTalentLevel = false;

    private byte talentLevel;
    public override void Execute()
    {
        if (TestExecute())
        {
            var player = FindObjectOfType<GameManager>().localPlayerCharacter;
            player.ChangeStat(stat, multiplyStatByTalentLevel ? amount * talentLevel : amount, dietType);
            if (vfx)
                player.CmdSpawnVfx(vfx.name);
            ActionFinished();
        }
        else
            Debug.Log("Insufficient Action Execution");
    }
    public override bool TestExecute()
    {
        talentLevel = FindObjectOfType<GameManager>().localPlayerCharacter.talentTrees.IsTalentUnlocked(requiredTalent, requiredTalentLevel);
        if (talentLevel >= requiredTalentLevel)
            return true;
        else
            return false;
    }
}
