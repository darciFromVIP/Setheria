using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunePillar : TurnInItemsInteractable
{
    public QuestScriptable questToComplete;
    protected override void ItemsTurnedIn()
    {
        base.ItemsTurnedIn();
        CmdQuestProgress();
    }
    public override void Interact(PlayerCharacter player)
    {
        if (questToComplete.active)
        {
            base.Interact(player);
            GetComponentInChildren<TooltipTrigger>(true).enabled = false;
        }
        else
            FindObjectOfType<SystemMessages>().AddMessage("The required quest is not active!");
    }
    [Command(requiresAuthority = false)]
    private void CmdQuestProgress()
    {
        RpcQuestProgress();
    }
    [ClientRpc]
    private void RpcQuestProgress()
    {
        questToComplete.ReduceCustom1Requirement(1);
    }
}
