using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using FMODUnity;

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
    [SyncVar] [SerializeField] protected int xp;                                             //We need SyncVars to sync data from server to client when the client connects
    [SyncVar] [SerializeField] protected int maxXp;
    protected int attributePoints = 0;
    public int hunger;
    public int maxHunger;
    protected float hungerInterval;
    protected float hungerIntervalMultiplier = 0;
    protected float hungerTimer = 0;
    protected Vector3 returnPoint;

    protected int attMaxHealth = 0;
    protected int attHealthRegen = 0;
    protected int attArmor = 0;
    protected int attMaxMana = 0;
    protected int attManaRegen = 0;
    protected int attPower = 0;
    protected int attCriticalChance = 0;
    protected int attCriticalDamage = 0;
    protected int attCooldownReduction = 0;

    public TalentTreesReference refTalentTrees;
    [HideInInspector] public TalentTrees talentTrees = new();
    public Professions professions;

    protected const float MaxXpMultiplier = 1.2f;
    protected const int BaseMaxXpValue = 100;

    [System.NonSerialized] public UnityEvent<int> Level_Up = new();
    [System.NonSerialized] public UnityEvent<int, int> Xp_Changed = new();
    [System.NonSerialized] public UnityEvent<PlayerCharacter> Character_Loaded = new();
    [SyncVar] public bool isLoaded = false;
    [System.NonSerialized] public UnityEvent Hunger_Changed = new();
    [System.NonSerialized] public UnityEvent<int> Attributes_Changed = new();
    [System.NonSerialized] public UnityEvent<List<Skill>> Skills_Changed = new();

    public ItemPrefabDatabase itemDatabase;
    public ItemScriptableDatabase itemScriptableDatabase;
    public List<TutorialDataScriptable> introductoryTutorial = new();
    public List<TutorialDataScriptable> levelUpTutorial = new();

    protected HasHealth healthComp;
    protected HasMana manaComp;
    protected CanMove moveComp;
    protected CanAttack attackComp;
    protected PlayerController playerController;
    protected GameObject recallVFX;
    [SerializeField] protected GameObject spotlight;
    [SerializeField] protected GameObject levelUpEffect;
    public EventReference levelUpSound;

    protected override void Start()
    {
        base.Start();
        healthComp = GetComponent<HasHealth>();

        manaComp = GetComponent<HasMana>();
        moveComp = GetComponent<CanMove>();
        attackComp = GetComponent<CanAttack>();
        playerController = GetComponent<PlayerController>();
        if (healthComp)
        {
            healthComp.Damage_Taken.AddListener(Provoked);
        }
        if (isOwned)
        {
            foreach (var item in introductoryTutorial)
            {
                FindObjectOfType<Tutorial>().QueueNewTutorial(item);
            }
        }
        professions = new Professions(this);
        foreach (var item in FindObjectsOfType<HeroButton>(true))
        {
            if (item.hero == hero)
                item.SetButtonInteractability(false);
        }
    }
    private void Provoked(NetworkIdentity enemy)
    {
        if (moveComp.agent.velocity.magnitude <= 0.01f && !enemy.GetComponent<PlayerCharacter>() && !GetComponent<CanAttack>().enemyTarget)
            attackComp.CmdTargetAcquired(enemy);
    }
    protected IEnumerator UpdatePlayer()
    {
        while (true)
        {
            hungerTimer += Time.deltaTime;
            if (hungerTimer >= GetHungerInterval())
            {
                hungerTimer = 0;
                ChangeHunger(-1, false);
            }
            else if (hunger <= 0 && hungerTimer >= 4)
            {
                hungerTimer = 0;
                healthComp.CmdTakeDamage(healthComp.GetBaseMaxHealth() * 0.2f, true, GetComponent<NetworkIdentity>(), true, true);
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
    public void LoadCharacterFromServer()
    {
        StartCoroutine(LoadCharacterCoro());
    }
    [Command]
    protected void LoadCharacter()
    {
        StartCoroutine(LoadCharacterCoro());
    }
    protected IEnumerator LoadCharacterCoro()
    {
        yield return new WaitUntil(EvaluateLoadData);
        LoadState(connectionToClient.identity.GetComponent<ClientObject>().GetSaveData());
    }
    protected bool EvaluateLoadData()
    {
        return connectionToClient.identity.GetComponent<ClientObject>().GetSaveData() != null;
    }
    [ClientRpc]
    public void LoadState(List<SaveDataPlayer> data)
    {
        foreach (var item in data)
        {
            if (item.hero == hero)
            {
                if (isOwned)
                {
                    if (item.professions != null)
                        professions = item.professions;
                    professions.player = this;
                    var arr = SceneManager.GetActiveScene().GetRootGameObjects();
                    List<NeedsLocalPlayerCharacter> list = new();
                    foreach (var item1 in arr)
                    {
                        list.AddRange(item1.GetComponentsInChildren<NeedsLocalPlayerCharacter>(true));
                    }
                    foreach (var item1 in list)
                    {
                        item1.SetLocalPlayerCharacter(this);
                    }
                    foreach (var item1 in skills)
                    {
                        item1.SetCastingEntity(this);
                    }
                }
                moveComp.agent.enabled = false;
                GetComponent<NetworkTransformUnreliable>().enabled = false;
                if (item.positionX == 0 && item.positionY == 0 && item.positionZ == 0)
                    transform.position = FindObjectOfType<WorldGenerator>().globalStartingPoint.position;
                else
                    transform.position = new Vector3(item.positionX, item.positionY, item.positionZ);
                transform.rotation = new Quaternion(item.rotationX, item.rotationY, item.rotationZ, item.rotationW);
                GetComponent<NetworkTransformUnreliable>().enabled = true;
                moveComp.agent.enabled = true;
                returnPoint = new Vector3(item.everstonePointX, item.everstonePointY, item.everstonePointZ);
                heroName = item.name;
                level = item.level;
                Level_Up.Invoke(level);
                xp = item.xp;
                maxXp = item.maxXp;
                ChangeAttributePoints(item.attributePoints);
                Xp_Changed.Invoke(xp, maxXp);
                maxHunger = item.maxHunger;
                ChangeHunger(item.hunger, false);
                hungerInterval = item.hungerInterval;
                ChangeHungerIntervalMultiplier(1);
                Debug.Log("Saved health: " + item.baseMaxHealth);
                healthComp.SetBaseMaxHealth(item.baseMaxHealth);
                healthComp.SetHealth(item.health);
                healthComp.SetBaseHealthRegen(item.baseHealthRegen);
                healthComp.ChangeCorruptedHealth(item.corruptedHealth);
                manaComp.SetMaxMana(item.baseMaxMana);
                manaComp.SetMana(item.mana);
                manaComp.SetBaseManaRegen(item.baseManaRegen);
                manaComp.ChangeCorruptedMana(item.corruptedMana);
                FindObjectOfType<CharacterSkillsWindow>().SetHealthMana(item.health, item.baseMaxHealth, item.mana, item.baseMaxMana);
                attackComp.SetPower(item.power);
                attackComp.SetCriticalChance(item.criticalChance);
                attackComp.SetCriticalDamage(item.criticalDamage);
                attackComp.SetAttackRange(item.attackRange);
                healthComp.SetArmor(item.armor);
                attackComp.SetCooldownReduction(item.cooldownReduction);
                var controller = GetComponent<PlayerController>();
                if (item.cooldown1 > 0)
                {
                    controller.StartCooldown1();
                    controller.cooldown1 = item.cooldown1;
                }
                if (item.cooldown2 > 0)
                {
                    controller.StartCooldown2();
                    controller.cooldown2 = item.cooldown2;
                }
                if (item.cooldown3 > 0)
                {
                    controller.StartCooldown3();
                    controller.cooldown3 = item.cooldown3;
                }
                if (item.cooldown4 > 0)
                {
                    controller.StartCooldown4();
                    controller.cooldown4 = item.cooldown4;
                }
                if (item.cooldown5 > 0)
                {
                    controller.StartCooldown5();
                    controller.cooldown5 = item.cooldown5;
                }
                if (isOwned)
                {
                    FindObjectOfType<QuestManager>(true).LoadStateUnsynchronized(item.unsyncedQuestlines);
                    var manager = FindObjectOfType<InventoryManager>(true);
                    foreach (var item3 in item.equippedGear)
                    {
                        var gearItem = manager.AddItem(item3);
                        gearItem.GetComponent<ItemButton>().TryEquip();
                    }
                    foreach (var item2 in item.inventory)
                    {
                        manager.AddItem(item2);
                    }
                    if (item.activeItems.Count > 0)
                    {
                        var activeItemsBar = FindObjectOfType<ActiveItemsBar>(true);
                        for (int i = 0; i < activeItemsBar.transform.childCount; i++)
                        {
                            if (item.activeItems[i] != null)
                            {
                                var inventoryItem = manager.GetItemOfName(item.activeItems[i].name);
                                activeItemsBar.transform.GetChild(i).GetComponent<ActiveItemSlot>().Initialize(inventoryItem);
                            }
                        }
                    }
                    talentTrees.talentPoints = 0;
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
                    foreach (var item5 in item.activebuffs)
                    {
                        AddBuff(item5.name);
                        StartCoroutine(WaitForBuff(item5));
                    }
                    foreach (var item6 in skills)
                    {
                        item6.ExecuteOnStart(this);
                    }
                    if (item.positionX != 0 && item.positionY != 0 && item.positionZ != 0)
                        FindObjectOfType<CameraTarget>().Teleport(new Vector3(item.positionX, item.positionY, item.positionZ));
                    UpdateManualCategories();
                }
            }
        }
        if (isOwned)
            UpdateSkills();
        Debug.Log("Character Loaded!");
        isLoaded = true;
        Character_Loaded.Invoke(this);
        healthComp.ChangeCorruptedHealth(0);
        manaComp.ChangeCorruptedMana(0);
        if (isOwned)
            StartCoroutine(UpdatePlayer());
    }
    private IEnumerator WaitForBuff(BuffSaveable buff)
    {
        bool buffIsThere = false;

        while (!buffIsThere)
        {
            foreach (var item in buffs)
            {
                if (item.name == buff.name)
                {
                    item.durationTimer = buff.remainingDuration;
                    for (int i = 1; i < buff.stacks; i++)
                    {
                        item.IncreaseStacks();
                    }
                    buffIsThere = true;
                }
            }
            yield return null;
        }
    }
    [TargetRpc]
    public void SaveState(NetworkConnection conn, NetworkIdentity player)
    {
        Debug.Log("Saving " + gameObject.name);

        var inventory = FindObjectOfType<InventoryManager>(true).GetAllItems();
        List<SaveDataItem> items = new();
        foreach (var item in inventory)
        {
            items.Add(new SaveDataItem { name = item.item.name, stacks = item.stacks });
        }

        List<SaveDataItem> gear = new();
        var inventoryScreen = FindObjectOfType<InventoryScreen>(true);
        foreach (var item in inventoryScreen.GetComponentsInChildren<CharacterGearSlot>(true))
        {
            if (item.transform.childCount > 0)
            {
                var bag = item.GetComponentInChildren<InventoryItem>(true);
                gear.Add(new SaveDataItem { name = bag.item.name, stacks = bag.stacks });
            }

        }
        var charScreen = FindObjectOfType<CharacterScreen>(true).GetEquippedGear();
        foreach (var item in charScreen)
        {
            gear.Add(new SaveDataItem { name = item.item.name, stacks = item.stacks });
        }
        var questManager = FindObjectOfType<QuestManager>(true);
        List<QuestlineSaveable> questlines = new();
        foreach (var item in questManager.SaveStateUnsynchronized())
        {
            questlines.Add(item);
        }
        List<BuffSaveable> activeBuffs = new();
        foreach (var item in buffs)
        {
            if (item.durationTimer > 0)
            {
                activeBuffs.Add(new BuffSaveable() { name = item.name, stacks = item.stacks, remainingDuration = item.durationTimer });
            }
        }
        List<SaveDataItem> activeItems = new();
        var activeItemsBar = FindObjectOfType<ActiveItemsBar>(true);
        for (int i = 0; i < activeItemsBar.transform.childCount; i++)
        {
            var item = activeItemsBar.transform.GetChild(i).GetComponent<ActiveItemSlot>().reference;
            if (item != null)
            {
                activeItems.Add(new SaveDataItem() { name = item.item.name, stacks = item.stacks });
            }
            else
                activeItems.Add(null);
        }

        var controller = GetComponent<PlayerController>();
        FindObjectOfType<NetworkedSaveLoad>().CmdSavePlayerState( new SaveDataPlayer {
            positionX = transform.position.x,
            positionY = transform.position.y,
            positionZ = transform.position.z,
            rotationW = transform.rotation.w,
            rotationY = transform.rotation.y,
            rotationX = transform.rotation.x,
            rotationZ = transform.rotation.z,
            everstonePointX = returnPoint.x,
            everstonePointY = returnPoint.y,
            everstonePointZ = returnPoint.z,
            hero = hero,
            level = level,
            maxXp = maxXp,
            xp = xp,
            attributePoints = attributePoints,
            name = heroName,
            inventory = items,
            equippedGear = gear,
            hunger = hunger,
            maxHunger = maxHunger,
            hungerInterval = hungerInterval,
            health = healthComp.GetHealth(),
            baseMaxHealth = healthComp.GetBaseMaxHealth(),
            baseHealthRegen = healthComp.GetBaseHealthRegen(),
            corruptedHealth = healthComp.GetCorruptedHealth(),
            mana = manaComp.GetMana(),
            baseMaxMana = manaComp.GetBaseMaxMana(),
            baseManaRegen = manaComp.GetBaseManaRegen(),
            corruptedMana = manaComp.GetCorruptedMana(),
            power = attackComp.GetBasePower(),
            criticalChance = attackComp.GetBaseCritChance(),
            criticalDamage = attackComp.GetBaseCritDamage(),
            attackRange = attackComp.GetAttackRange(),
            armor = healthComp.GetBaseArmor(),
            cooldownReduction = attackComp.GetBaseCooldownReduction(),
            cooldown1 = controller.cooldown1,
            cooldown2 = controller.cooldown2,
            cooldown3 = controller.cooldown3,
            cooldown4 = controller.cooldown4,
            cooldown5 = controller.cooldown5,
            talentTrees = talentTrees,
            professions = professions,
            unsyncedQuestlines = questlines,
            activebuffs = activeBuffs,
            activeItems = activeItems
        });
    }
    public PlayerCharacter GetLocalPlayerCharacter()
    {
        if (isOwned)
            return this;
        else
            return null;
    }
    [Command(requiresAuthority = false)]
    public void CmdAddXp(int value)
    {
        RpcAddXp(value);
    }
    [ClientRpc]
    public void RpcAddXp(int value)
    {
        AddXp(value);
    }
    public void AddXp(int value)
    {
        xp += value;
        FindObjectOfType<FloatingText>().SpawnText("+" + value.ToString() + " <sprite=14>", transform.position + Vector3.up * 0.5f, FloatingTextType.Experience);

        if (xp >= maxXp)
        {
            xp = xp - maxXp;
            level++;
            maxXp = (int)(BaseMaxXpValue * level * MaxXpMultiplier);
            Level_Up.Invoke(level);
            Debug.Log("Level up: " + level);
            if (level == 2)
            {
                foreach (var item in levelUpTutorial)
                {
                    FindObjectOfType<Tutorial>().QueueNewTutorial(item);
                }
            }    
            if (level <= 5)
                talentTrees.ChangeTalentPoints(1);
            ChangeAttributePoints(2);
            FindObjectOfType<FloatingText>().SpawnText("+1 <sprite=13>", transform.position + Vector3.up * 1, FloatingTextType.Experience);
            levelUpEffect.SetActive(true);
            FindObjectOfType<AudioManager>().PlayOneShot(levelUpSound, transform.position);
        }
        Xp_Changed.Invoke(xp, maxXp);
    }    
    public void SpawnProfessionFloatingText(TalentTreeType profType, int amount, int currentProf, int maxProf)
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
            case TalentTreeType.Exploration:
                type = FloatingTextType.Exploration;
                break;
            default:
                break;
        }
        FindObjectOfType<FloatingText>().CmdSpawnFloatingText(profType.ToString() + ": " + currentProf + "/" + maxProf + " (+" + amount + ")", transform.position, type);
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
    protected void RpcUpdateCreatedItem(NetworkIdentity item, int stacks)
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
        while (true)
        {
            if (originDest != moveComp.agent.destination)
            {
                yield break;
            }
            if (playerController.ContainsCollider(player.GetComponent<Collider>()))
                break;
            yield return null;
        }
        moveComp.Stop();
        CmdAddItemToInventory(itemToGive.item.name, itemToGive.stacks, player.netIdentity);
        itemToGive.DestroyItem();
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
    public void ChangeHunger(int amount, bool showTextToOthers)
    {
        hunger += amount;
        if ((hunger == 20 || hunger == 10) && amount < 0)
        {
            FindObjectOfType<SystemMessages>().AddMessage("You are starving!");
        }
        if (amount > 0)
        {
            if (showTextToOthers)
            {
                FindObjectOfType<FloatingText>().CmdSpawnFloatingText("+" + amount + " <sprite=12>", transform.position, FloatingTextType.Hunger);
                FindObjectOfType<AudioManager>().EatFood(transform.position);
            }
            else
                FindObjectOfType<FloatingText>().SpawnText("+" + amount + " <sprite=12>", transform.position, FloatingTextType.Hunger);
        }
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
                    healthComp.CmdTakeDamage(modifier, true, GetComponent<NetworkIdentity>(), false, true);
                break;
            case PlayerStat.MaxHealth:
                healthComp.CmdChangeBaseMaxHealth(modifier);
                break;
            case PlayerStat.HealthRegen:
                healthComp.CmdChangeBaseHealthRegen(modifier);
                break;
            case PlayerStat.Mana:
                if (modifier > 0)
                    manaComp.CmdRestoreMana(modifier);
                else
                    manaComp.CmdSpendMana(modifier);
                break;
            case PlayerStat.MaxMana:
                manaComp.CmdChangeBaseMaxMana(modifier);
                break;
            case PlayerStat.ManaRegen:
                manaComp.CmdChangeBaseManaRegen(modifier);
                break;
            case PlayerStat.Hunger:
                ChangeHunger((int)modifier, true);
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
            case PlayerStat.Power:
                GetComponent<CanAttack>().CmdChangePower(modifier);
                break;
            case PlayerStat.AttackSpeed:
                GetComponent<CanAttack>().CmdChangeAttackSpeedMultiplier(modifier);
                break;
            case PlayerStat.CriticalChance:
                GetComponent<CanAttack>().CmdChangeCriticalChance(modifier);
                break;
            case PlayerStat.CriticalDamage:
                GetComponent<CanAttack>().CmdChangeCriticalDamage(modifier);
                break;
            case PlayerStat.Armor:
                GetComponent<HasHealth>().CmdChangeArmor(modifier);
                break;
            case PlayerStat.CooldownReduction:
                GetComponent<CanAttack>().CmdChangeCooldownReduction(modifier);
                break;
            case PlayerStat.Level:
                CmdAddXp(maxXp - xp);
                break;
            case PlayerStat.CorruptedHealthMana:
                GetComponent<HasHealth>().CmdChangeCorruptedHealth(modifier);
                GetComponent<HasMana>().CmdChangeCorruptedMana(modifier);
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
                if (HasBuff("Poisoned"))
                {
                    FindObjectOfType<SystemMessages>().AddMessage("You can't eat while poisoned! You would throw up immediately...");
                    return false;
                }
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
            case PlayerStat.ActiveItemSlot:
                break;
            case PlayerStat.AttributePoint:
                break;
            case PlayerStat.Power:
                break;
            case PlayerStat.AttackSpeed:
                break;
            case PlayerStat.CriticalChance:
                break;
            case PlayerStat.CriticalDamage:
                break;
            case PlayerStat.Armor:
                break;
            case PlayerStat.CooldownReduction:
                break;
            case PlayerStat.MovementSpeed:
                break;
            case PlayerStat.Level:
                break;
            case PlayerStat.CorruptedHealthMana:
                if (GetComponent<HasHealth>().GetCorruptedHealth() <= 0)
                {
                    FindObjectOfType<SystemMessages>().AddMessage("You are not corrupted!");
                    return false;
                }
                if (GetComponent<HasMana>().GetCorruptedMana() <= 0)
                {
                    FindObjectOfType<SystemMessages>().AddMessage("You are not corrupted!");
                    return false;
                }
                break;
            default:
                break;
        }
        return true;
    }
    public override void AddBuff(string buff)
    {
        base.AddBuff(buff);
        UpdateSkills();
    }
    public void BoardShip()
    {
        GetComponent<ObjectMapIcon>().CmdToggleIconOnMap(false);
        DisableCharacter();
    }
    public void UnboardShip(Vector3 position)
    {
        GetComponent<ObjectMapIcon>().CmdToggleIconOnMap(true);
        transform.position = position;
        EnableCharacter();
    }
    public void DisableCharacter()
    {
        foreach (var item in GetComponentsInChildren<MeshRenderer>())
        {
            item.enabled = false;
        }
        foreach (var item in GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            item.enabled = false;
        }
        GetComponentInChildren<EntityStatusBar>(true).gameObject.SetActive(false);
        moveComp.agent.enabled = false;
        playerController.ChangeState(PlayerState.OutOfGame);
        spotlight.SetActive(false);
    }
    public void EnableCharacter()
    {
        foreach (var item in GetComponentsInChildren<MeshRenderer>())
        {
            item.enabled = true;
        }
        foreach (var item in GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            item.enabled = true;
        }
        GetComponentInChildren<EntityStatusBar>(true).gameObject.SetActive(true);
        moveComp.agent.enabled = true;
        playerController.ChangeState(PlayerState.None);
        spotlight.SetActive(true);
    }
    public void AddMaxHealthAttribute(int value)
    {
        attMaxHealth += value;
        ChangeAttributePoints(-value);
        healthComp.CmdChangeBaseMaxHealth(value * 15);
    }
    public void AddHealthRegenAttribute(int value)
    {
        attHealthRegen += value;
        ChangeAttributePoints(-value);
        healthComp.CmdChangeBaseHealthRegen(value * 0.1f);
    }
    public void AddArmorAttribute(int value)
    {
        attArmor += value;
        ChangeAttributePoints(-value);
        healthComp.CmdChangeArmor(value * 0.5f);
    }
    public void AddMaxManaAttribute(int value)
    {
        attMaxMana += value;
        ChangeAttributePoints(-value);
        manaComp.CmdChangeBaseMaxMana(value * 15);
    }
    public void AddManaRegenAttribute(int value)
    {
        attManaRegen += value;
        ChangeAttributePoints(-value);
        manaComp.CmdChangeBaseManaRegen(value * 0.1f);
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
    public void AddCooldownReductionAttribute(int value)
    {
        attCooldownReduction += value;
        ChangeAttributePoints(-value);
        attackComp.CmdChangeCooldownReduction(1f);
    }
    public void SetReturnPoint()
    {
        returnPoint = transform.position;
    }
    public bool IsReturnPointValid()
    {
        return returnPoint != Vector3.zero;
    }
    [Command(requiresAuthority = false)]
    public void CmdSpawnRecallVFX()
    {
        recallVFX = Instantiate(vfxDatabase.GetVFXByName("Recall"), transform.position, Quaternion.identity);
        GetComponent<PlayerController>().Work_Cancelled.AddListener(DestroyRecallVFX);
        NetworkServer.Spawn(recallVFX);
    }
    private void DestroyRecallVFX()
    {
        NetworkServer.Destroy(recallVFX);
        GetComponent<PlayerController>().Work_Cancelled.RemoveListener(DestroyRecallVFX);
    }
    public void Recall()
    {
        moveComp.agent.enabled = false;
        StartCoroutine(DelayedRecallEnd());
        GetComponent<NetworkTransformUnreliable>().CmdTeleport(returnPoint);
    }
    protected IEnumerator DelayedRecallEnd()
    {
        Vector3 currentPos = transform.position;
        while (currentPos == transform.position)
        {
            yield return null;
        }
        moveComp.agent.enabled = true;
        moveComp.Stop();
        FindObjectOfType<CameraTarget>().Teleport(transform.position);
        CmdPrintLocation();
    }
    [Command(requiresAuthority = false)]
    private void CmdPrintLocation()
    {
        Debug.Log("Server Location of " + name + ": " + transform.position);
        RpcPrintLocation();
    }
    [ClientRpc]
    private void RpcPrintLocation()
    {
        Debug.Log("Client Location of " + name + ": " + transform.position);
    }
    public void UpdateManualCategories()
    {
        FindObjectOfType<ManualScreen>().UpdateCategoryButtons(professions);
    }
    [Command(requiresAuthority = false)]
    public void CmdChangeHungerIntervalMultiplier(float value)
    {
        RpcChangeHungerIntervalMultiplier(value);
    }
    [ClientRpc]
    public void RpcChangeHungerIntervalMultiplier(float value)
    {
        ChangeHungerIntervalMultiplier(value);
    }
    public void ChangeHungerIntervalMultiplier(float value)
    {
        hungerIntervalMultiplier += value;
        Hunger_Changed.Invoke();
    }
    public float GetHungerInterval()
    {
        return hungerInterval * hungerIntervalMultiplier;
    }
}
