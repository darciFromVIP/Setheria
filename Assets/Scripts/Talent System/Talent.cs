using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Talent
{
    public TalentType talentType;
    public byte currentLevel = 0;
    public Talent() { }
    public Talent(TalentType type)
    {
        talentType = type;
        currentLevel = 0;
    }
    public void IncreaseCurrentLevel()
    {
        currentLevel++;
    }
    public void SetCurrentLevel(byte value)
    {
        currentLevel = value;
    }
}
