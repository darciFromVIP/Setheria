using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Campfire : MonoBehaviour
{
    protected void Start()
    {
        FindObjectOfType<SystemMessages>().AddMessage("Remember to eat regularly so that you don't starve to death!", MsgType.Notice);
    }

}
