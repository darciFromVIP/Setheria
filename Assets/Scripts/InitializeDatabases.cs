using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitializeDatabases : MonoBehaviour
{
    public QuestlineDatabase questlineDatabase;

    private void Start()
    {
        foreach (var item in questlineDatabase.questlines)
        {
            item.Initialize();
        }
    }
}
