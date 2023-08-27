using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wander : MonoBehaviour
{
    public float wanderInterval = 5;
    public float wanderRange = 10;
    private Vector3 startingPosition;
    private CanMove moveComp;

    private bool paused;
    private void Start()
    {
        moveComp = GetComponent<CanMove>();
        startingPosition = transform.position;
        GetComponent<Entity>().On_Death.AddListener(StopWander);
        var canAttackComp = GetComponent<CanAttack>();
        if (canAttackComp)
        {
            canAttackComp.Target_Acquired.AddListener(PauseWander);
            canAttackComp.Target_Lost.AddListener(ResumeWander);
        }
        StartCoroutine(StartWander());
    }
    private IEnumerator StartWander()
    {
        float wanderTimer = wanderInterval;
        while (wanderTimer > 0)
        {
            if (!paused)
                wanderTimer -= Time.deltaTime;
            yield return null;
        }
        moveComp.MoveTo(new Vector3(startingPosition.x + Random.Range(-wanderRange, wanderRange), startingPosition.y, startingPosition.z + Random.Range(-wanderRange, wanderRange)));
        StartCoroutine(StartWander());
    }
    private void PauseWander(NetworkIdentity target)
    {
        paused = true;
    }
    private void ResumeWander()
    {
        paused = false;
    }
    private void StopWander()
    {
        StopAllCoroutines();
    }
}
