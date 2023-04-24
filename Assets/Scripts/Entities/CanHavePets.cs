using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class CanHavePets : NetworkBehaviour
{
    private List<Entity> spawnedPets = new();
    public EntityDatabase entityDatabase;

    [Command(requiresAuthority = false)]
    public void CmdSpawnPet(string name, Vector3 position, float time, float power)
    {
        var pet = Instantiate(entityDatabase.GetEntityByName(name), position, Quaternion.identity);
        pet.GetComponent<CanAttack>().SetPower(power);
        spawnedPets.Add(pet);
        pet.GetComponent<Pet>().StartTimedLife(time, this);
        NetworkServer.Spawn(pet.gameObject);
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
