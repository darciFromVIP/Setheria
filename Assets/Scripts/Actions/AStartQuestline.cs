using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Actions/Start Questline")]
public class AStartQuestline : ActionTemplate
{
    public QuestlineScriptable questlineStarted;
    public bool giveQuestlineToAllPlayers;
    public override void ActionFinished()
    {
        
    }

    public override void Execute()
    {
        if (giveQuestlineToAllPlayers)
            FindObjectOfType<QuestManager>().CmdNewQuestline(questlineStarted.name);
        else
            FindObjectOfType<QuestManager>().NewQuestline(questlineStarted.name);
    }

    public override bool TestExecute()
    {
        return true;
    }
}
