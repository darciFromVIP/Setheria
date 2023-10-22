using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Campfire : Structure
{
    protected override void Start()
    {
        base.Start();
        FindObjectOfType<SystemMessages>().AddMessage("Remember to eat regularly so that you don't starve to death!", MsgType.Notice);
    }

}
