using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial Data")]
public class TutorialDataScriptable : ScriptableObject
{
    public string label;
    public AnimationClip gifAnim;
    [TextArea(5, 5)]
    public string description;
}
