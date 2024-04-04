using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Actions/Bait Spawner Boss")]
public class ABaitSpawnerBoss : ActionTemplate
{
    public override void ActionFinished()
    {
        Action_Finished.Invoke();
    }

    public override void Execute()
    {
        var player = FindObjectOfType<GameManager>().localPlayerCharacter.GetComponent<PlayerController>();
        foreach (var item in player.GetColliders())
        {
            if (item.TryGetComponent(out EnemySpawner spawner))
            {
                spawner.SpawnBoss();
                ActionFinished();
            }
        }
    }
    public override bool TestExecute()
    {
        var player = FindObjectOfType<GameManager>().localPlayerCharacter.GetComponent<PlayerController>();
        foreach (var item in player.GetColliders())
        {
            if (item != null)
            {
                if (item.TryGetComponent(out EnemySpawner spawner))
                {
                    return true;
                }
            }
        }
        FindObjectOfType<SystemMessages>().AddMessage("You aren't standing near a Cave!");
        return false;
    }
}
