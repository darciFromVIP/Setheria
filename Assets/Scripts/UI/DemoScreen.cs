using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoScreen : MonoBehaviour
{
    public GameObject window;
    public QuestScriptable questRequired;

    private void Start()
    {
        questRequired.Quest_Complete.AddListener(GameOver);
    }
    private void GameOver()
    {
        window.SetActive(true);
    }
}
