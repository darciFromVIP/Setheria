using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BrokenBridge : MonoBehaviour
{
    public GameObject brokenModel;
    public GameObject repairedModel;

    private void Start()
    {
        GetComponent<TurnInItemsInteractable>().Items_Turned_In.AddListener(RepairBridge);
    }
    private void RepairBridge()
    {
        brokenModel.SetActive(false);
        repairedModel.SetActive(true);
        Destroy(GetComponent<TurnInItemsInteractable>());
        GetComponent<NavMeshObstacle>().enabled = false;
    }
}
