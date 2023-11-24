using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Actions/Show Tutorial")]
public class AShowTutorial : ActionTemplate
{
    public TutorialDataScriptable tutorialToShow;
    private bool alreadyShown = false;

    public override void ActionFinished()
    {
        
    }

    public override void Execute()
    {
        FindObjectOfType<Tutorial>().QueueNewTutorial(tutorialToShow);
        alreadyShown = true;
    }

    public override bool TestExecute()
    {
        return !alreadyShown;
    }
}
