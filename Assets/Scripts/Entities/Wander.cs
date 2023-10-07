using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wander : MonoBehaviour
{
    public float wanderInterval = 5;
    public float wanderRange = 10;
    public bool rangeFromStart = true;
    private Vector3 startingPosition;
    private CanMove moveComp;
    private float baseSpeed;

    private bool paused;
    private void Start()
    {
        moveComp = GetComponent<CanMove>();
        baseSpeed = moveComp.baseMovementSpeed;
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
        moveComp.SetBaseMovementSpeed(1);
        if (rangeFromStart)
            moveComp.MoveTo(new Vector3(startingPosition.x + Random.Range(-wanderRange, wanderRange), startingPosition.y, startingPosition.z + Random.Range(-wanderRange, wanderRange)));
        else
            moveComp.MoveTo(new Vector3(transform.position.x + Random.Range(-wanderRange, wanderRange), transform.position.y, transform.position.z + Random.Range(-wanderRange, wanderRange)));
        StartCoroutine(StartWander());
    }
    private void PauseWander(NetworkIdentity target)
    {
        moveComp.SetBaseMovementSpeed(baseSpeed);
        paused = true;
    }
    private void ResumeWander()
    {
        paused = false;
    }
    private void StopWander()
    {
        moveComp.SetBaseMovementSpeed(baseSpeed);
        StopAllCoroutines();
    }
}
