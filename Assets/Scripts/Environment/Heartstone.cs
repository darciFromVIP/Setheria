using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heartstone : MonoBehaviour, ISaveable
{
    public bool respawnActivated;

    public void ActivateRespawn()
    {
        respawnActivated = true;
        FindObjectOfType<SystemMessages>().AddMessage("New respawn point unlocked!", MsgType.Positive);
        FindObjectOfType<GameManager>().localPlayerCharacter.DeathTutorial();
    }
    public void LoadState(SaveDataWorldObject state)
    {
        respawnActivated = state.boolData1;
    }

    public SaveDataWorldObject SaveState()
    {
        return new SaveDataWorldObject
        {
            boolData1 = respawnActivated
        };
    }
}
