using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Actions/Sonar")]
public class ASonar : ActionTemplate
{
    public override void ActionFinished()
    {
        Action_Finished.Invoke();
    }

    public override void Execute()
    {
        FindObjectOfType<SonarSpots>().RevealRandomSpot();
        ActionFinished();
    }

    public override bool TestExecute()
    {
        if (FindObjectOfType<SonarSpots>().IsSonarAreaRevealed())
        {
            FindObjectOfType<SystemMessages>().AddMessage("There already is a marine treasure revealed.", MsgType.Error);
            return false;
        }
        else
            return true;
    }
}
