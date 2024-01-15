using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorruptionTumorAnimation : MonoBehaviour
{
    void Awake()
    {
         GetComponent<Animator>().speed = Random.Range(0.4f, 1.5f);
    }
}
