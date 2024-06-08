using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicZone : MonoBehaviour
{
    public MusicParameter dayParameter, nightParameter;
    public void PlayMusic()
    {
        FindObjectOfType<AudioManager>().ChangeMusicParameter(FindObjectOfType<DayNightCycle>().IsNight() ? nightParameter : dayParameter);
    }
}
