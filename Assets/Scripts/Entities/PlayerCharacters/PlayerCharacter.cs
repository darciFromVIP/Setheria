using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using static UnityEngine.Rendering.DebugUI;

[System.Serializable]
public enum Hero
{
    Lycandruid, ForestProtector
}
public interface LocalPlayerCharacter
{
    public PlayerCharacter GetLocalPlayerCharacter();
}
public interface NeedsLocalPlayerCharacter
{
    public void SetLocalPlayerCharacter(PlayerCharacter player);
}
public class PlayerCharacter : Character, LocalPlayerCharacter
{
    public string heroName;
    public Hero hero;
    [SyncVar]
    private int xp;                                             //We need SyncVars to sync data from server to client when the client connects
    [SyncVar]
    private int maxXp;
    private int attributePoints = 0;
    public int hunger;
    public int maxHunger;
    private float hungerInterval;
    private float hungerTimer = 0;
    private Vector3 returnPoint;

    private int attMaxHealth = 0;
    private int attHealthRegen = 0;
    private int attArmor = 0;
    private int attMaxMana = 0;
    private int attManaRegen = 0;
    private int attPower = 0;
    private int attCriticalChance = 0;
    private int attCriticalDamage = 0;
    private int attAttackSpeed = 0;
    private int attCooldownReduction = 0;

    public TalentTreesReference refTalentTrees;
    [HideInInspector] public TalentTrees talentTrees = new();
    public Professions professions;

    private const float MaxXpMultiplier = 1.2f;
    private const int BaseMaxXpValue = 100;

    [System.NonSerialized] public UnityEvent<int> Level_Up = new();
    [System.NonSerialized] public UnityEvent<int, int> Xp_Changed = new();
    [System.NonSerialized] public UnityEvent<PlayerCharacter> Character_Loaded = new();
    [SyncVar] public bool isLoaded = false;
    [System.NonSerialized] public UnityEvent Hunger_Changed = new();
    [System.NonSerialized] public UnityEvent<int> Attributes_Changed = new();
    [System.NonSerialized] public UnityEvent<List<Skill>> Skills_Changed = new();

    public ItemPrefabDatabase itemDatabase;
    public ItemScriptableDatabase itemScriptableDatabase;

    private HasHealth healthComp;
    private HasMana manaComp;
    private CanMove moveComp;
    private CanAttack attackComp;
    private PlayerController playerController;

    protected override void Start()
    {
        base.Start();
        healthComp = GetComponent<HasHealth>();
        manaComp = GetComponent<HasMana>();
        moveComp = GetComponent<CanMove>();
        attackComp = GetComponent<CanAttack>();
        playerController = GetComponent<PlayerController>();
        professions = new Professions(this);
    }
    private IEnumerator UpdatePlayer()
    {
        while (true)
        {
            hungerTimer += Time.deltaTime;
            if (hungerTimer >= hungerInterval)
            {
                hungerTimer = 0;
                ChangeHunger(-1);
            }
            else if (hunger <= 0 && hungerTimer >= 4)
            {
                hungerTimer = 0;
                healthComp.CmdTakeDamage(healthComp.GetBaseMaxHealth() * 0.2f, true, GetComponent<NetworkIdentity>());
            }
            yield return null;
        }
    }
    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        if (!isOwned)
            return;        

        LoadCharacter();
    }
    [Command]
    private void LoadCharacter()
    {
        StartCoroutine(LoadCharacterCoro());
    }
    private IEnumerator LoadCharacterCoro()
    {
        yield return new WaitUntil(EvaluateLoadData);
        LoadState(connectionToClient.identity.GetComponent<ClientObject>().GetSaveData());
    }
    private bool EvaluateLoadData()
    {
        return connectionToClient.identity.GetComponent<ClientObject>().GetSaveData() != null;
    }
    [ClientRpc]
    public void LoadState(List<SaveDataPlayer> data)
    {
        if (isOwned)
        {
            var arr = SceneManager.GetActiveScene().GetRootGameObjects();
            List<NeedsLocalPlayerCharacter> list = new();
            foreach (var item in arr)
            {
                list.AddRange(item.GetComponentsInChildren<NeedsLocalPlayerCharacter>(true));
            }
            foreach (var item in list)
            {
                item.SetLocalPlayerCharacter(this);
            }
            foreach (var item in skills)
            {
                item.SetCastingEntity(this);
            }
        }
        foreach (var item in data)
        {
            if (item.hero == hero)
            {
                moveComp.StopAgent();
                if (item.positionX == 0 && item.positionY == 0 && item.positionZ == 0)
                    GetComponent<NetworkTransform>().CmdTeleport(FindObjectOfType<WorldGenerator>().globalStartingPoint.position);
                else
                    GetComponent<NetworkTransform>().CmdTeleport(new Vector3(item.positionX, item.positionY, item.positionZ));
                moveComp.ResumeAgent();
                transform.rotation = new Quaternion(item.rotationX, item.rotationY, item.rotationZ, item.rotationW);
                heroName = item.name;
                level = item.level;
                Level_Up.Invoke(level);
                xp = item.xp;
                maxXp = item.maxXp;
                ChangeAttributePoints(item.attributePoints);
                Xp_Changed.Invoke(xp, maxXp);
                var manager = FindObjectOfType<InventoryManager>(true);
                foreach (var item2 in item.inventory)
                {
                    manager.AddItem(item2);
                }
                maxHunger = item.maxHunger;
                ChangeHunger(item.hunger);
                hungerInterval = item.hungerInterval;
                healthComp.SetBaseMaxHealth(item.baseMaxHealth);
                healthComp.SetBonusMaxHealth(item.bonusMaxHealth);
                healthComp.SetHealth(item.health);
                healthComp.SetBaseHealthRegen(item.baseHealthRegen);
                healthComp.SetBonusHealthRegen(item.bonusHealthRegen);
                manaComp.SetMaxMana(item.baseMaxMana);
                manaComp.SetMana(item.mana);
                manaComp.SetBaseManaRegen(item.baseManaRegen);
                manaComp.SetBonusManaRegen(item.bonusManaRegen);
                attackComp.SetPower(item.power);
                attackComp.SetCriticalChance(item.criticalChance);
                attackComp.SetCriticalDamage(item.criticalDamage);
                attackComp.SetBaseAttackSpeed(item.attackSpeed);
                attackComp.SetAttackRange(item.attackRange);
                healthComp.SetArmor(item.armor);
                attackComp.SetCooldownReduction(item.cooldownReduction);
                if (isOwned)
                {
                    if (item.talentTrees != null)
                    {
                        talentTrees = item.talentTrees;
                    }
                    else
                    {
                        foreach (var item2 in refTalentTrees.talentTrees)
                        {
                            talentTrees.talentTrees.Add(new TalentTree(item2.treeType, item2.talents));
                        }
                    }
                    if (item.professions != null)
                        professions = item.professions;
                    professions.player = this;
                }
            }
        }
        if (isOwned)
            UpdateSkills();
        Debug.Log("Character Loaded!");
        isLoaded = true;
        Character_Loaded.Invoke(this);
        if (isOwned)
            StartCoroutine(UpdatePlayer());
    }
    public SaveDataPlayer SaveState()
    {
        var inventory = FindObjectOfType<InventoryManager>(true).GetAllItems();
        List<SaveDataItem> items = new();
        foreach (var item in inventory)
        {
            items.Add(new SaveDataItem { name = item.item.name, stacks = item.stacks });
        }
        return new SaveDataPlayer {
            positionX = transform.position.x,
            positionY = transform.position.y,
            positionZ = transform.position.z,
            rotationW = transform.rotation.w,
            rotationY = transform.rotation.y,
            rotationX = transform.rotation.x,
            rotationZ = transform.rotation.z,
            hero = hero,
            level = level,
            maxXp = maxXp,
            xp = xp,
            attributePoints = attributePoints,
            name = heroName,
            inventory = items,
            hunger = hunger,
            maxHunger = maxHunger,
            hungerInterval = hungerInterval,
            health = healthComp.GetHealth(),
            baseMaxHealth = healthComp.GetBaseMaxHealth(),
            bonusMaxHealth = healthComp.GetBonusMaxHealth(),
            baseHealthRegen = healthComp.GetBaseHealthRegen(),
            bonusHealthRegen = healthComp.GetBonusHealthRegen(),
            mana = manaComp.GetMana(),
            baseMaxMana = manaComp.GetBaseMaxMana(),
            bonusMaxMana = manaComp.GetBonusMaxMana(),
            baseManaRegen = manaComp.GetBaseManaRegen(),
            bonusManaRegen = manaComp.GetBonusManaRegen(),
            power = attackComp.GetPower(),
            criticalChance = attackComp.GetCritChance(),
            criticalDamage = attackComp.GetCritDamage(),
            attackSpeed = attackComp.GetAttackSpeed(),
            attackRange = attackComp.GetAttackRange(),
            armor = healthComp.GetArmor(),
            cooldownReduction = attackComp.GetCooldownReduction(),
            talentTrees = talentTrees,
            professions = professions
        };
    }
    public PlayerCharacter GetLocalPlayerCharacter()
    {
        if (isOwned)
            return this;
        else
            return null;
    }
    public void AddXp(int value)
    {
        xp += value;
        FindObjectOfType<FloatingText>().SpawnFloatingText("+" + value.ToString() + " EXP", transform.position + Vector3.up, FloatingTextType.Experience);

        if (xp >= maxXp)
        {
            xp = xp - maxXp;
            level++;
            maxXp = (int)(BaseMaxXpValue * level * MaxXpMultiplier);
            Level_Up.Invoke(level);
            talentTrees.ChangeTalentPoints(1);
            ChangeAttributePoints(2);
            FindObjectOfType<FloatingText>().SpawnFloatingText("Level Up!", transform.position + Vector3.up * 2, FloatingTextType.Experience);
        }
        Xp_Changed.Invoke(xp, maxXp);
    }
    public void SpawnProfessionFloatingText(TalentTreeType profType, int amount)
    {
        FloatingTextType type = FloatingTextType.Gathering;
        switch (profType)
        {
            case TalentTreeType.Gathering:
                type = FloatingTextType.Gathering;
                break;
            case TalentTreeType.Cooking:
                type = FloatingTextType.Cooking;
                break;
            case TalentTreeType.Alchemy:
                type = FloatingTextType.Alchemy;
                break;
            case TalentTreeType.Fishing:
                type = FloatingTextType.Fishing;
                break;
            default:
                break;
        }
        FindObjectOfType<FloatingText>().SpawnFloatingText(profType.ToString() + " +" + amount.ToString(), transform.position, type);
    }
    public void ChangeAttributePoints(int value)
    {
        attributePoints += value;
        Attributes_Changed.Invoke(attributePoints);
        UpdateSkills();
    }
    [Command (requiresAuthority = false)]
    public void CreateItem(SaveDataItem item, Vector3 destination)
    {
        var newItem = Instantiate(itemDatabase.GetItemByName(item.name), destination, Quaternion.identity);
        NetworkServer.Spawn(newItem.gameObject);
        RpcUpdateCreatedItem(newItem.GetComponent<NetworkIdentity>(), item.stacks);
    }
    [ClientRpc]
    private void RpcUpdateCreatedItem(NetworkIdentity item, int stacks)
    {
        item.GetComponent<Item>().stacks = stacks;
    }
    public IEnumerator GoToDropItem(InventoryItem itemToDrop, Vector3 destination)
    {
        moveComp.MoveTo(destination);
        var originDest = moveComp.agent.destination;
        while (true)
        {
            if (originDest != moveComp.agent.destination)
            {
                yield break;
            }
            if (moveComp.HasReachedDestination())
                break;
            yield return null;    
        }
        moveComp.Stop();
        CreateItem(new SaveDataItem() { name = itemToDrop.item.name, stacks = itemToDrop.stacks }, destination);
        itemToDrop.DestroyItem();
    }
    public IEnumerator GoToGiveItem(InventoryItem itemToGive, PlayerCharacter player)
    {
        moveComp.MoveTo(player.transform.position);
        var originDest = moveComp.agent.destination;
        while (!moveComp.HasReachedDestination())
        {
            if (originDest != moveComp.agent.destination)
            {
                yield break;
            }
            yield return null;
        }
        CmdAddItemToInventory(itemToGive.item.name, itemToGive.stacks, player.netIdentity);
        Destroy(itemToGive.gameObject);
    }
    [Command]
    public void CmdAddItemToInventory(string item, int stacks, NetworkIdentity targetPlayer)
    {
        targetPlayer.GetComponent<PlayerCharacter>().RpcAddItemToInventory(targetPlayer.connectionToClient, item, stacks, targetPlayer);
    }
    [TargetRpc]
    public void RpcAddItemToInventory(NetworkConnection conn, string item, int stacks, NetworkIdentity targetPlayer)
    {
        FindObjectOfType<InventoryManager>(true).AddItem(itemScriptableDatabase.GetItemByName(item), stacks);
    }
    public void ChangeHunger(int amount)
    {
        hunger += amount;
        if (amount > 0)
            FindObjectOfType<FloatingText>().SpawnFloatingText("+" + amount + " Food", transform.position, FloatingTextType.Hunger);
        Hunger_Changed.Invoke();
    }
    public void ChangeStat(PlayerStat playerStat, float modifier)
    {
        switch (playerStat)
        {
            case PlayerStat.Health:
                if (modifier > 0)
                    healthComp.CmdHealDamage(modifier, false);
                else
                    healthComp.CmdTakeDamage(modifier, true, GetComponent<NetworkIdentity>());
                break;
            case PlayerStat.MaxHealth:
                healthComp.ChangeBonusMaxHealth(modifier);
                break;
            case PlayerStat.HealthRegen:
                healthComp.ChangeBonusHealthRegen(modifier);
                break;
            case PlayerStat.Mana:
                if (modifier > 0)
                    manaComp.CmdRestoreMana(modifier);
                else
                    manaComp.CmdSpendMana(modifier);
                break;
            case PlayerStat.MaxMana:
                manaComp.CmdChangeBonusMaxMana(modifier);
                break;
            case PlayerStat.ManaRegen:
                manaComp.CmdChangeBonusManaRegen(modifier);
                break;
            case PlayerStat.Hunger:
                ChangeHunger((int)modifier);
                break;
            case PlayerStat.MaxHunger:
                break;
            case PlayerStat.Resources:
                FindObjectOfType<GameManager>().ChangeResources((int)modifier);
                break;
            case PlayerStat.Knowledge:
                FindObjectOfType<GameManager>().ChangeKnowledge((int)modifier);
                break;
            case PlayerStat.ActiveItemSlot:
                FindObjectOfType<ActiveItemsBar>().UnlockSlot();
                break;
            case PlayerStat.AttributePoint:
                ChangeAttributePoints((int)modifier);
                break;
            case PlayerStat.MovementSpeed:
                GetComponent<CanMove>().ChangeBonusMovementSpeed(modifier);
                break;
            default:
                break;
        }
        Skills_Changed.Invoke(skills);
    }
    public bool TestChangeStat(PlayerStat playerStat, float modifier)
    {
        switch (playerStat)
        {
            case PlayerStat.Health:
                break;
            case PlayerStat.MaxHealth:
                break;
            case PlayerStat.HealthRegen:
                break;
            case PlayerStat.Mana:
                break;
            case PlayerStat.MaxMana:
                break;
            case PlayerStat.ManaRegen:
                break;
            case PlayerStat.Hunger:
                if (hunger + modifier > 100)
                    return false;
                break;
            case PlayerStat.MaxHunger:
                break;
            case PlayerStat.Resources:
                if (modifier < 0)
                    return FindObjectOfType<GameManager>().TestSubtractResources(-(int)modifier);
                break;
            case PlayerStat.Knowledge:
                if (modifier < 0)
                    return FindObjectOfType<GameManager>().TestSubtractKnowledge(-(int)modifier);
                break;
            default:
                break;
        }
        return true;
    }
    protected override void AddBuff(string buff)
    {
        base.AddBuff(buff);
        UpdateSkills();
    }
    public void BoardShip()
    {
        transform.localScale = Vector3.zero;
        moveComp.agent.enabled = false;
        playerController.ChangeState(PlayerState.Busy);
    }
    public void UnboardShip(Vector3 position)
    {
        transform.localScale = Vector3.one;
        transform.position = position;
        moveComp.agent.enabled = true;
        playerController.ChangeState(PlayerState.None);
    }
    public void AddMaxHealthAttribute(int value)
    {
        attMaxHealth += value;
        ChangeAttributePoints(-value);
        healthComp.ChangeBonusMaxHealth(value * 10);
    }
    public void AddHealthRegenAttribute(int value)
    {
        attHealthRegen += value;
        ChangeAttributePoints(-value);
        healthComp.ChangeBonusHealthRegen(value * 0.05f);
    }
    public void AddArmorAttribute(int value)
    {
        attArmor += value;
        ChangeAttributePoints(-value);
        healthComp.ChangeArmor(value * 0.5f);
    }
    public void AddMaxManaAttribute(int value)
    {
        attMaxMana += value;
        ChangeAttributePoints(-value);
        manaComp.CmdChangeBonusMaxMana(value * 10);
    }
    public void AddManaRegenAttribute(int value)
    {
        attManaRegen += value;
        ChangeAttributePoints(-value);
        manaComp.CmdChangeBonusManaRegen(value * 0.05f);
    }
    public void AddPowerAttribute(int value)
    {
        attPower += value;
        ChangeAttributePoints(-value);
        attackComp.CmdChangePower(value * 1);
    }
    public void AddCriticalChanceAttribute(int value)
    {
        attCriticalChance += value;
        ChangeAttributePoints(-value);
        attackComp.CmdChangeCriticalChance(value * 0.5f);
    }
    public void AddCriticalDamageAttribute(int value)
    {
        attCriticalDamage += value;
        ChangeAttributePoints(-value);
        attackComp.CmdChangeCriticalDamage(value * 1);
    }
    public void AddAttackSpeedAttribute(int value)
    {
        attAttackSpeed += value;
        ChangeAttributePoints(-value);
        attackComp.CmdChangeBonusAttackSpeed(value * 0.03f);
    }
    public void AddCooldownReductionAttribute(int value)
    {
        attCooldownReduction += value;
        ChangeAttributePoints(-value);
        attackComp.CmdChangeCooldownReduction(0.5f);
    }
    public void SetReturnPoint()
    {
        returnPoint = transform.position;
    }
    public void Recall()
    {
        moveComp.StopAgent();
        GetComponent<NetworkTransform>().CmdTeleport(returnPoint);
        moveComp.ResumeAgent();
        moveComp.Stop();
    }
}
