using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

public enum FloatingTextType
{
    Healing, Damage, Resources, Knowledge, Experience, Hunger, Gathering, Fishing, Cooking, Alchemy
}

public class FloatingText : NetworkBehaviour
{
    public FloatingTextPrefab textPrefab;

    [Command(requiresAuthority = false)]
    public void CmdSpawnFloatingText(string msg, Vector3 position, FloatingTextType type)
    {
        SpawnText(msg, position, type);
    }
    private void SpawnText(string msg, Vector3 position, FloatingTextType type)
    {
        var text = Instantiate(textPrefab, position, Quaternion.identity);
        NetworkServer.Spawn(text.gameObject);
        Setup(text.GetComponent<NetworkIdentity>(), msg, type);
    }
    [ClientRpc]
    private void Setup(NetworkIdentity text, string msg, FloatingTextType type)
    {
        text.GetComponent<FloatingTextPrefab>().SetUp(msg, type);
    }
}
