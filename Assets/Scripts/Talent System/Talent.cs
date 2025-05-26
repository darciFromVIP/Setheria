using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Talent
{
    public string name;
    public byte currentLevel = 0;
    public Talent() { }
    public Talent(string name)
    {
        this.name = name;
        currentLevel = 0;
    }
    public void IncreaseCurrentLevel(PlayerCharacter player)
    {
        currentLevel++;
        player.UnlockTalent(name, currentLevel - 1, currentLevel);
    }
    public void ResetLevel(PlayerCharacter player)
    {
        player.ResetTalent(name, currentLevel, 0);
        currentLevel = 0;
    }
    public void SetCurrentLevel(byte value)
    {
        currentLevel = value;
    }
}
