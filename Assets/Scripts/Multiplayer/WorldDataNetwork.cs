using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class WorldDataNetwork : NetworkBehaviour
{
    public SaveDataWorld worldData;
    public override void OnStartServer()
    {
        base.OnStartServer();
    }
}
