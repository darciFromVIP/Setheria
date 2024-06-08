using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicZonePart : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerCharacter player))
        {
            if (player.isOwned)
                GetComponentInParent<MusicZone>().PlayMusic();
        }
    }
}
