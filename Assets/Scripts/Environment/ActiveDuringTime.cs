using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ActiveDuringTime : MonoBehaviour
{
    public bool activeDuringDay, activeDuringNight;
    public GameObject objectToChange;
    public NavMeshObstacle obstacle;
    private void Start()
    {
        var daynight = FindObjectOfType<DayNightCycle>();
        if (activeDuringDay)
        {
            daynight.Day_Started.AddListener(TurnOn);
            daynight.Night_Started.AddListener(TurnOff);
        }
        if (activeDuringNight)
        {
            daynight.Night_Started.AddListener(TurnOn);
            daynight.Day_Started.AddListener(TurnOff);
        }
        if (daynight.IsNight() && !activeDuringNight)
            TurnOff();
        else if (!daynight.IsNight() && !activeDuringDay)
            TurnOff();
    }

    private void TurnOn()
    {
        objectToChange.SetActive(true);
        if (obstacle)
            obstacle.enabled = false;
    }
    private void TurnOff()
    {
        objectToChange.SetActive(false);
        if (obstacle)
            obstacle.enabled = true;
    }
}
