using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class QuestlineSaveable
{
    public string questlineName;
    public int currentQuestIndex;
    public List<string> questRequirementsNames;
    public List<int> questRequirementsValues;
}
