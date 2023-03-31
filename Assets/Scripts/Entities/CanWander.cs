using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanWander : MonoBehaviour
{
    private Vector3 startingPosition;
    public float maxWanderRange;
    public LayerMask layerMask;

    private CanMove moveComp;
    private void Start()
    {
        startingPosition = transform.position;
        moveComp = GetComponent<CanMove>();
        StartCoroutine(Wander());
    }
    private IEnumerator Wander()
    {
        while (true)
        {
            Vector3 newPos = Vector3.zero;
            RaycastHit hit;
            Ray ray = new Ray(transform.position + (Vector3.up * 5) + new Vector3(Random.Range(-3f, 4f), 0, Random.Range(-3f, 4f)), Vector3.down);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                newPos = hit.point;
            }
            if (Vector3.Distance(newPos, startingPosition) > maxWanderRange)
            {
                newPos = startingPosition;
            }
            moveComp.MoveTo(newPos);
            yield return new WaitForSeconds(Random.Range(1, 10));
        }
    }
}
