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
    public SettingsManager settingsManager;
    private List<Collider> collidingColliders = new();
    private GameObject clickEffect;
    public InputEnabledScriptable inputEnabled;

    private void Start()
    {
        pickupComp = GetComponent<CanPickup>();
        moveComp = GetComponent<CanMove>();
        shipComp = GetComponent<Ship>();
        settingsManager = FindObjectOfType<SettingsManager>();
        collidingColliders.Clear();
        clickEffect = FindObjectOfType<ClickEffect>().gameObject;
    }
    private void Update()
    {
        if (isOwned && inputEnabled.inputEnabled)
            InputHandle();
    }
    private void OnTriggerEnter(Collider other)
    {
        collidingColliders.Add(other);
    }
    private void OnTriggerStay(Collider other)
    {
        if (!collidingColliders.Contains(other))
            collidingColliders.Add(other);
    }
    private void OnTriggerExit(Collider other)
    {
        collidingColliders.Remove(other);
    }
    public bool ContainsCollider(Collider colliderToCompare)
    {
        return collidingColliders.Contains(colliderToCompare);
    }
    private void InputHandle()
    {
        if (Input.GetKeyDown(settingsManager.settings.move))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, shipMask))
            {
                if (hit.collider.TryGetComponent(out Item item))
                {
                    StartCoroutine(pickupComp.GoToPickup(item));
                }
                else if (hit.collider is TerrainCollider)
                {
                    StartCoroutine(GoToUnload(hit.point));
                }
                else if (hit.collider.TryGetComponent(out IInteractable interactable))
                {
                    if (hit.collider.TryGetComponent(out SchoolOfFish fishing))
                        StartCoroutine(GoToInteract(fishing.interactionCollider, interactable));
                    else
                        StartCoroutine(GoToInteract(hit.collider, interactable));
                }
                else
                {
                    clickEffect.transform.position = hit.point;
                    clickEffect.GetComponent<ParticleSystem>().Play();
                    moveComp.MoveTo(hit.point);
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
        shipComp.UnloadCrew(position);
    }
    private IEnumerator GoToInteract(Collider collider, IInteractable interactable)
    {
        moveComp.MoveTo(collider.transform.position);
        var originDest = moveComp.agent.destination;
        while (true)
        {
            if (Vector3.Distance(originDest, moveComp.agent.destination) > 2.5f)
            {
                yield break;
            }
            if (ContainsCollider(collider))
                break;
            yield return null;
        }
        moveComp.Stop();
        interactable.Interact(shipComp.crew[0]);
    }
}
