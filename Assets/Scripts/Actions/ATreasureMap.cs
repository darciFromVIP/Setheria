using FoW;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Actions/Treasure Map")]
public class ATreasureMap : ActionTemplate
{
    private Transform treasureSpot;
    private PlayerController localPlayer;
    private SystemMessages messages;

    public override void ActionFinished()
    {
        Action_Finished.Invoke();
    }

    public override void Execute()
    {
        if (treasureSpot)
        {
            if (localPlayer.ContainsCollider(treasureSpot.GetComponent<Collider>()))
            {
                FindObjectOfType<WorldMap>().DestroyTreasureMapArea();
                treasureSpot.GetComponent<CanDropItem>().SpawnItemsInInventory(FindObjectOfType<InventoryManager>(true));
                ActionFinished();
                treasureSpot = null;
            }
            else
            {
                messages.AddMessage("You are " + (int)Vector3.Distance(treasureSpot.position, localPlayer.transform.position) + " meters away from the treasure.", MsgType.Notice);
            }
        }
        else
        {
            treasureSpot = FindObjectOfType<TreasureMapSpots>().GetRandomSpot();
            FindObjectOfType<WorldMap>().CreateTreasureMapArea(FogOfWarTeam.GetTeam(0).WorldPositionToFogPosition(treasureSpot.position));
            messages = FindObjectOfType<SystemMessages>();
            messages.AddMessage("Your map has been updated with the rough location of the treasure.", MsgType.Notice);
            localPlayer = FindObjectOfType<GameManager>().localPlayerCharacter.GetComponent<PlayerController>();
        }
    }

    public override bool TestExecute()
    {
        return true;
    }
}
