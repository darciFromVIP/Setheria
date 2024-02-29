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
    public Structure upgradePrefab;
    public int demolishCost;
    private float notificationTimer = 0;

    private void Update()
    {
        if (notificationTimer > 0)
            notificationTimer -= Time.deltaTime;
    }
    protected override void Start()
    {
        base.Start();
        if (structureData)
        {
            structureData.Structure_Built.Invoke(structureData);
            FindObjectOfType<AudioManager>().BuildingFinished(transform.position);
            GetComponent<TooltipTriggerWorld>().objectName = structureData.name;
        }
        if (isServer)
        {
            foreach (var item in skills)
            {
                item.ExecuteOnStart(this);
            }
        }
        if (TryGetComponent(out HasHealth hp))
            hp.Damage_Taken.AddListener(StructureUnderAttack);
    }
    public virtual void LoadState(SaveDataWorldObject state)
    {
        transform.position = new Vector3(state.positionX, state.positionY, state.positionZ);
        transform.rotation = new Quaternion(state.rotationX, state.rotationY, state.rotationZ, state.rotationW);
    }
    public virtual SaveDataWorldObject SaveState()
    {
        return new SaveDataWorldObject
        {
            name = structureItem.name,
            positionX = transform.position.x,
            positionY = transform.position.y,
            positionZ = transform.position.z,
            rotationX = transform.rotation.x,
            rotationY = transform.rotation.y,
            rotationZ = transform.rotation.z,
            rotationW = transform.rotation.w,
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
    public void RepairStructure(float amount)
    {
        GetComponent<HasHealth>().HealDamage(amount, false);
        FindObjectOfType<GameManager>().ChangeResources(-1);
    }
    private void StructureUnderAttack(NetworkIdentity enemy)
    {
        if (notificationTimer > 0)
            return;
        notificationTimer = 10;
        FindObjectOfType<SystemMessages>().AddMessage("Your base is under attack!", MsgType.Notice);
    }
    [Command(requiresAuthority = false)]
    public void CmdUpgradeStructure()
    {
        if (TryGetComponent(out ObjectMapIcon icon))
            icon.RpcDestroyIcon();
        var instance = Instantiate(upgradePrefab, transform.position, transform.rotation);
        NetworkServer.Spawn(instance.gameObject);
        NetworkServer.Destroy(gameObject);
    }
}
