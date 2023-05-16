using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
public class BiomeAmbienceArea : MonoBehaviour
{
    public EventReference ambienceEvent;
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerCharacter player))
        {
            if (player.isOwned)
                FindObjectOfType<AudioManager>().PlayAmbience(ambienceEvent);

        }
    }
}
