using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISaveable
{
    SaveDataWorldObject SaveState();
    void LoadState(SaveDataWorldObject state);
}
