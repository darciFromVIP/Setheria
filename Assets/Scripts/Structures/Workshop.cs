using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Workshop : MonoBehaviour
{
    public int tier;

    private void Start()
    {
        FindObjectOfType<ResearchScreen>(true).UnlockTier(tier);
    }
}
