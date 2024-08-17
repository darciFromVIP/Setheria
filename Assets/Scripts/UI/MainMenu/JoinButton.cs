using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JoinButton : MonoBehaviour
{
    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(JoinAFriend);
    }
    public void JoinAFriend()
    {
        SteamFriends.ActivateGameOverlay("friends");
    }
}
