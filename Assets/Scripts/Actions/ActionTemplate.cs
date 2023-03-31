using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public abstract class ActionTemplate : ScriptableObject
{
    [HideInInspector] public UnityEvent Action_Finished = new();
    public abstract void Execute();
    public abstract bool TestExecute();
    public abstract void ActionFinished();
}
