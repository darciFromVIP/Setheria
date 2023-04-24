using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class Pet : NetworkBehaviour
{
    public CanHavePets petOwner;
    public GameObject despawnVfx;
    
    public void StartTimedLife(float time, CanHavePets owner)
    {
        petOwner = owner;
        StartCoroutine(TimedLife(time));
        if (TryGetComponent(out CanMove canMove))
            if (canMove.baseMovementSpeed > 0)
                StartCoroutine(MovementOrder());
    }
    private IEnumerator TimedLife(float time)
    {
        var timedLife = time;
        while (timedLife > 0)
        {
            timedLife -= Time.deltaTime;
            yield return null;
        }
        if (despawnVfx)
        {
            RpcPlayDespawnVFX();
            PlayDespawnVFX();
            petOwner.DespawnPet(GetComponent<Entity>(), false);
        }
        else
            petOwner.DespawnPet(GetComponent<Entity>(), true);
    }
    private IEnumerator MovementOrder()
    {
        GetComponent<CanMove>().MoveTo(petOwner.transform.position + new Vector3(Random.Range(-6, 6), 0, Random.Range(-6, 6)));
        float period = 3;
        float timer = period;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }
        StartCoroutine(MovementOrder());
    }
    [ClientRpc]
    private void RpcPlayDespawnVFX()
    {
        PlayDespawnVFX();   
    }
    private void PlayDespawnVFX()
    {
        despawnVfx.SetActive(true);
        despawnVfx.transform.SetParent(null);
    }
}
