using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureMapSpots : MonoBehaviour
{
    public Transform GetRandomSpot()
    {
        int random = Random.Range(0, transform.childCount);
        return transform.GetChild(random);
    }
}
