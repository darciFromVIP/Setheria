using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(CanMove), typeof(CanPickup))]
public class ShipController : NetworkBehaviour
{
    private CanMove moveComp;
    private CanPickup pickupComp;
    private Ship shipComp;
    public LayerMask shipMask;
    private void Start()
    {
        pickupComp = GetComponent<CanPickup>();
        moveComp = GetComponent<CanMove>();
        shipComp = GetComponent<Ship>();
    }
    private void Update()
    {
        if (isOwned)
            InputHandle();
    }
    private void InputHandle()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, shipMask))
            {
                moveComp.MoveTo(hit.point);

                if (hit.collider.TryGetComponent(out Item item))
                {
                    StartCoroutine(pickupComp.GoToPickup(item));
                }
                if (hit.collider is TerrainCollider)
                {
                    StartCoroutine(GoToUnload(hit.point));
                }
            }
        }
    }
    private IEnumerator GoToUnload(Vector3 position)
    {
        moveComp.MoveTo(position);
        var originDest = moveComp.agent.destination;
        while (true)
        {
            if (originDest != moveComp.agent.destination)
            {
                yield break;
            }
            if (Vector3.Distance(transform.position, moveComp.agent.destination) < 2)
                break;
            yield return null;
        }
        moveComp.Stop();
        shipComp.UnloadCrew(moveComp.agent.destination);
    }
}
