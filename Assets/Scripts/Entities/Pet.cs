using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class Pet : NetworkBehaviour
{
    private CanHavePets petOwner;
    public GameObject despawnVfx;
    
    public void StartTimedLife(float time, CanHavePets owner)
    {
        petOwner = owner;
        StartCoroutine(TimedLife(time));
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
        RpcPlayDespawnVFX();
        PlayDespawnVFX();
        petOwner.DespawnPet(GetComponent<Entity>());
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
