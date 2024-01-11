using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Actions/Cleanse Buff")]
public class ACleanseBuff : ActionTemplate
{
    public List<BuffScriptable> buffsToCleanse = new();
    public override void ActionFinished()
    {
    }

    public override void Execute()
    {
        var player = FindObjectOfType<GameManager>().localPlayerCharacter;
        foreach (var item in buffsToCleanse)
        {
            if (player.HasBuff(item.name))
                player.CmdRemoveBuff(item.name);
        }
        ActionFinished();
    }

    public override bool TestExecute()
    {
        var player = FindObjectOfType<GameManager>().localPlayerCharacter;
        foreach (var item in buffsToCleanse)
        {
            if (player.HasBuff(item.name))
                return true;
        }
        FindObjectOfType<SystemMessages>().AddMessage("You don't have any valid buffs to cleanse!");
        return false;
    }
}
