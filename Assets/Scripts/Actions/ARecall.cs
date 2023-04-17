using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Actions/Recall")]
public class ARecall : ActionTemplate
{
    public override void ActionFinished()
    {
    }

    public override void Execute()
    {
        var player = FindObjectOfType<GameManager>().localPlayerCharacter.GetComponent<PlayerController>();
        player.CmdStartWorking(6);
        player.Work_Finished.AddListener(CastFinished);
    }
    private void CastFinished()
    {
        FindObjectOfType<GameManager>().localPlayerCharacter.Recall();
        Action_Finished.Invoke();
    }
    public override bool TestExecute()
    {
        return true;
    }
}
