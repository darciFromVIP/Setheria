using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;

[System.Serializable] 
public struct StructureLevelData
{
    public GameObject model;
    public float hp;
}
public class Structure : Entity, ISaveable, IInteractable
{
    public ItemScriptable structureItem;
    public List<StructureOption> structureOptions;
    public List<Skill> skills = new();
    public EntityDatabase entityDatabase;
    public Transform unitSpawnPoint;
    public StructureScriptable structureData;
    public List<StructureLevelData> structureLevels = new();
    public ParticleSystem upgradeVFX;
    public int demolishCost;
    private float notificationTimer = 0;
    public int currentLevel = 0;

    public UnityEvent<int> Structure_Upgraded = new();

    private void Update()
    {
        if (notificationTimer > 0)
            notificationTimer -= Time.deltaTime;
    }
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
        GetComponent<HasHealth>().Damage_Taken.AddListener(StructureUnderAttack);
    }
    public virtual void LoadState(SaveDataWorldObject state)
    {
        transform.position = new Vector3(state.positionX, state.positionY, state.positionZ);
        transform.rotation = new Quaternion(state.rotationX, state.rotationY, state.rotationZ, state.rotationW);
        UpgradeStructure(state.intData1);
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
            intData1 = currentLevel
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
        RpcUpgradeStructure();
    }
    [ClientRpc]
    public void RpcUpgradeStructure()
    {
        UpgradeStructure();
    }
    public void UpgradeStructure()
    {
        structureLevels[currentLevel].model.SetActive(false);
        currentLevel++;
        OnUpgrade();
    }
    [Command(requiresAuthority = false)]
    public void CmdUpgradeStructure(int level)
    {
        RpcUpgradeStructure(level);
    }
    [ClientRpc]
    public void RpcUpgradeStructure(int level)
    {
        UpgradeStructure(level);
    }
    public void UpgradeStructure(int level)
    {
        structureLevels[currentLevel].model.SetActive(false);
        currentLevel = level;
        OnUpgrade();
    }
    private void OnUpgrade()
    {
        structureLevels[currentLevel].model.SetActive(true);
        GetComponent<HasHealth>().SetBaseMaxHealth(structureLevels[currentLevel].hp);
        GetComponent<HasHealth>().SetHealth(structureLevels[currentLevel].hp);
        upgradeVFX.Play();
        Structure_Upgraded.Invoke(currentLevel);
    }
}
