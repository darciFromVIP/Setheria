using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wander : MonoBehaviour
{
    public float wanderInterval;
    public float wanderRange;
    private Vector3 startingPosition;
    private CanMove moveComp;
    private void Start()
    {
        moveComp = GetComponent<CanMove>();
        startingPosition = transform.position;
        GetComponent<Entity>().On_Death.AddListener(StopWander);
        StartCoroutine(StartWander());
    }
    private IEnumerator StartWander()
    {
        float wanderTimer = wanderInterval;
        while (wanderTimer > 0)
        {
            wanderTimer -= Time.deltaTime;
            yield return null;
        }
        moveComp.MoveTo(new Vector3(startingPosition.x + Random.Range(-wanderRange, wanderRange), startingPosition.y, startingPosition.z + Random.Range(-wanderRange, wanderRange)));
        StartCoroutine(StartWander());
    }
    private void StopWander()
    {
        StopAllCoroutines();
    }
}
