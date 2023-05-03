using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
[CreateAssetMenu(menuName = "Structure")]
public class StructureScriptable : ScriptableObject
{
    public UnityEvent<StructureScriptable> Structure_Built = new();
}
