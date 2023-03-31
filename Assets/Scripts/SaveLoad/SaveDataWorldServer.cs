using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveDataWorldServer
{
    public bool fresh = false;
    public SaveDataWorld worldSaveData = new();
    public Dictionary<string, Dictionary<string, SaveDataWorldObject>> worldObjects = new();


    public SaveDataWorldServer() { }
    public SaveDataWorldServer(string name, int seed)
    {
        worldSaveData.worldName = name;
        worldSaveData.worldSeed = seed;
    }
}
