using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;
using UnityEngine.UIElements;

public enum FloatingTextType
{
    Healing, Damage, Resources, Knowledge, Experience, Hunger, Gathering, Fishing, Cooking, Alchemy, Exploration, CriticalDamage
}

public class FloatingText : NetworkBehaviour
{
    public FloatingTextPrefab textPrefab;

    [Command(requiresAuthority = false)]
    public void CmdSpawnFloatingText(string msg, Vector3 position, FloatingTextType type)
    {
        ServerSpawnFloatingText(msg, position, type);
    }
    public void ServerSpawnFloatingText(string msg, Vector3 position, FloatingTextType type)
    {
        var text = Instantiate(textPrefab, position, Quaternion.identity);
        NetworkServer.Spawn(text.gameObject);
        RpcSetup(text.GetComponent<NetworkIdentity>(), msg, type);
    }
    public void SpawnText(string msg, Vector3 position, FloatingTextType type)
    {
        var text = Instantiate(textPrefab, position, Quaternion.identity);
        Setup(text.GetComponent<NetworkIdentity>(), msg, type);
    }
    [ClientRpc]
    private void RpcSetup(NetworkIdentity text, string msg, FloatingTextType type)
    {
        text.GetComponent<FloatingTextPrefab>().SetUp(msg, type);
    }
    private void Setup(NetworkIdentity text, string msg, FloatingTextType type)
    {
        text.GetComponent<FloatingTextPrefab>().SetUp(msg, type);
    }
}
