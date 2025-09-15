using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SonarSpots : MonoBehaviour 
{
    public void RevealRandomSpot()
    {
        int random = Random.Range(0, transform.childCount);
        transform.GetChild(random).GetComponent<SonarLootableObject>().CmdEnableObject();
        FindObjectOfType<SystemMessages>().AddMessage("Your map has been updated with the exact location of a marine treasure.", MsgType.Notice);
    }
    public bool IsSonarAreaRevealed()
    {
        foreach (var item in transform.GetComponentsInChildren<SonarLootableObject>())
        {
            if (item.gameObject.activeSelf)
                return true;
        }
        return false;
    }
}
