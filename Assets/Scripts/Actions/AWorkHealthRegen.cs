using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Actions/Work Health Regen")]
public class AWorkHealthRegen : ActionTemplate
{
    private HasHealth playerHealth;
    public int healAmount;
    public float duration;
    public override void ActionFinished()
    {
    }

    public override void Execute()
    {
        var player = FindObjectOfType<GameManager>().localPlayerCharacter.GetComponent<PlayerController>();
        playerHealth = player.GetComponent<HasHealth>();
        player.Work_Tick.RemoveListener(WorkTick);
        player.Work_Tick.AddListener(WorkTick);
        player.CmdStartWorking(duration);
        ActionFinished();
    }
    private void WorkTick()
    {
        if (playerHealth != null)
        {
            playerHealth.CmdHealDamage(healAmount, false);
        }
    }
    public override bool TestExecute()
    {
        return true;
    }
}
