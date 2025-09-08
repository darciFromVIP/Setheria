using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Actions/Reset Diet")]
public class AResetDiet : ActionTemplate
{
    public override void ActionFinished()
    {
        Action_Finished.Invoke();
    }

    public override void Execute()
    {
        var player = FindObjectOfType<GameManager>().localPlayerCharacter.GetComponent<PlayerCharacter>();
        player.ResetDiet();
    }
    public override bool TestExecute()
    {
        return true;
    }
}
