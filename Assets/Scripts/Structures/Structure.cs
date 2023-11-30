using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class Structure : Entity, ISaveable, IInteractable
{
    public ItemScriptable structureItem;
    public List<StructureOption> structureOptions;
    public List<Skill> skills = new();
    public EntityDatabase entityDatabase;
    public Transform unitSpawnPoint;
    public StructureScriptable structureData;
    public int demolishCost;

    protected override void Start()
    {
        base.Start();
        structureData.Structure_Built.Invoke(structureData);
        FindObjectOfType<AudioManager>().BuildingFinished(transform.position);
        GetComponent<TooltipTriggerWorld>().objectName = structureData.name;
        if (isServer)
        {
            foreach (var item in skills)
            {
                item.ExecuteOnStart(this);
            }
        }
    }
    public virtual void LoadState(SaveDataWorldObject state)
    {
        transform.position = new Vector3(state.positionX, state.positionY, state.positionZ);
    }
    public virtual SaveDataWorldObject SaveState()
    {
        return new SaveDataWorldObject
        {
            name = structureItem.name,
            positionX = transform.position.x,
            positionY = transform.position.y,
            positionZ = transform.position.z
        };
    }

    public void Interact(PlayerCharacter player = null)
    {
        FindObjectOfType<StructureScreen>().Open(this);
        FindObjectOfType<StructureScreenHPBar>(true).AddListenerToHpEvents(GetComponent<HasHealth>());
    }
    public void SpawnUnit(string unitName)
    {
        CmdSpawnUnit(unitName);
    }
    [Command(requiresAuthority = false)]
    private void CmdSpawnUnit(string unitName)
    {
        var temp = Instantiate(entityDatabase.GetEntityByName(unitName).gameObject, unitSpawnPoint.position, unitSpawnPoint.rotation);
        NetworkServer.Spawn(temp);
    }
    [Command(requiresAuthority = false)]
    public void CmdDemolishStructure()
    {
        RpcDemolishStructure();
        NetworkServer.Destroy(gameObject);
    }
    [ClientRpc]
    private void RpcDemolishStructure()
    {
        FindObjectOfType<AudioManager>().BuildingDestroyed(transform.position);
    }
}
