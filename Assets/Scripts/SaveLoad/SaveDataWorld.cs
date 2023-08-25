using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[System.Serializable]
public class SaveDataWorld
{
    public string worldName;
    public int worldSeed;
    public byte[] fogOfWar;
    public List<QuestlineSaveable> questlines;
}
