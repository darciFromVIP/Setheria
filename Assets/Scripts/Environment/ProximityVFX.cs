using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProximityVFX : MonoBehaviour
{
    public GameObject vfx;
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerCharacter player))
        {
            vfx.SetActive(true);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out PlayerCharacter player))
        {
            vfx.SetActive(false);
        }
    }
}
