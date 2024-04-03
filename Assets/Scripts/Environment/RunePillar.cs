using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunePillar : TurnInItemsInteractable
{
    public QuestScriptable questToComplete;
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
}
