using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class CanHavePets : NetworkBehaviour
{
    public List<Entity> spawnedPets = new();
    public EntityDatabase entityDatabase;

    public void SpawnPet(string name, Vector3 position, float time, float power)
    {
        var pet = Instantiate(entityDatabase.GetEntityByName(name), position, Quaternion.identity);
        spawnedPets.Add(pet);
        NetworkServer.Spawn(pet.gameObject, gameObject);
        RpcInitializePet(pet.GetComponent<NetworkIdentity>(), time, power);
    }
    [ClientRpc]
    private void RpcInitializePet(NetworkIdentity pet, float time, float power)
    {
        StartCoroutine(WaitForServerLoad(pet, time, power));
    }
    private IEnumerator WaitForServerLoad(NetworkIdentity pet, float time, float power)
    {
        while (!(pet.isClient || pet.isServer))
        {
            yield return null;
        }
        pet.GetComponent<Pet>().StartTimedLife(time, this);
        pet.GetComponent<CanAttack>().SetPower(power);
    }

    public void DespawnPet(Entity pet, bool deathAnimation)
    {
        spawnedPets.Remove(pet);
        if (deathAnimation)
            pet.RpcOnDeath();
        else
            NetworkServer.Destroy(pet.gameObject);
    }
    
}
