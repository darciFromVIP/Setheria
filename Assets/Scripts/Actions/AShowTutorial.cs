using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Actions/Show Tutorial")]
public class AShowTutorial : ActionTemplate
{
    public TutorialDataScriptable tutorialToShow;

    public override void ActionFinished()
    {
        
    }

    public override void Execute()
    {
        FindObjectOfType<Tutorial>().QueueNewTutorial(tutorialToShow);
    }

    public override bool TestExecute()
    {
        return true;
    }
}
