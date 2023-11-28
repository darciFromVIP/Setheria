using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShutdownServerClient : MonoBehaviour
{
    private void Start()
    {
        if (NetworkServer.active)
            NetworkServer.Shutdown();
        if (NetworkClient.active)
            NetworkClient.Shutdown();
    }
}
