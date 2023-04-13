using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffList : MonoBehaviour
{
    public BuffUI buffPrefab;

    public void AddBuff(string buffName, Buff buffInstance)
    {
        BuffUI buffUiInstance = Instantiate(buffPrefab, transform);
        buffUiInstance.Initialize(buffName, buffInstance);
    }
}
