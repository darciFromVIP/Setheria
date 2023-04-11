using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffList : MonoBehaviour
{
    public BuffUI buffPrefab;

    public void AddBuff(BuffScriptable buff, Buff buffInstance)
    {
        BuffUI buffUiInstance = Instantiate(buffPrefab, transform);
        buffUiInstance.Initialize(buff, buffInstance);
    }
}
