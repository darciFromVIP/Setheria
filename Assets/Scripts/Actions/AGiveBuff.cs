using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Actions/Give Buff")]
public class AGiveBuff : ActionTemplate
{
    public List<BuffScriptable> buffsToApply = new();
    public override void ActionFinished()
    {
    }

    public override void Execute()
    {
        var player = FindObjectOfType<GameManager>().localPlayerCharacter;
        foreach (var item in buffsToApply)
        {
            player.CmdAddBuff(item.name);
        }
        ActionFinished();
    }

    public override bool TestExecute()
    {
        var player = FindObjectOfType<GameManager>().localPlayerCharacter;
        foreach (var item in buffsToApply)
        {
            if (player.HasBuff(item.name) < item.maxStacks && item.stackable)
                return true;
            else if (player.HasBuff(item.name) <= 0 && item.stackable)
                return true;
            else if (player.HasBuff(item.name) >= item.maxStacks && item.stackable)
            {
                FindObjectOfType<SystemMessages>().AddMessage("This buff is at max stacks.");
                return false;
            }
            else
            {
                FindObjectOfType<SystemMessages>().AddMessage("You already have such non-stackable buff.");
                return false;
            }
        }
        return false;
    }
}
