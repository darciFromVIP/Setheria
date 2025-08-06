using FMODUnity;
using Mirror;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using static UnityEditor.Progress;

[System.Serializable]
public enum Hero
{
    Lycandruid, ForestProtector, KnowledgeDragon
}
public enum DietType
{
    None, Carnivore, Herbivore, Omnivore
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
    [SyncVar] public int hunger;
    [SyncVar] public int maxHunger;
    protected float hungerInterval;
    protected float hungerIntervalMultiplier = 0;
    protected float hungerTimer = 0;
    [SyncVar] public int water;
    [SyncVar] public int maxWater;
    protected float waterInterval;
    protected float waterIntervalMultiplier = 0;
    protected float waterTimer = 0;
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
    protected float carnivorePercentage = 50;
    protected int hungerCount = 0;
    protected float hungerBonus = 1;

    protected const float MaxXpMultiplier = 1.2f;
    protected const int BaseMaxXpValue = 100;

    [System.NonSerialized] public UnityEvent<int> Level_Up = new();
    [System.NonSerialized] public UnityEvent<int, int> Xp_Changed = new();
    [System.NonSerialized] public UnityEvent<PlayerCharacter> Character_Loaded = new();
    [SyncVar] public bool isLoaded = false;
    [System.NonSerialized] public UnityEvent Hunger_Changed = new();
    [System.NonSerialized] public UnityEvent Water_Changed = new();
    [System.NonSerialized] public UnityEvent<int> Attributes_Changed = new();
    [System.NonSerialized] public UnityEvent<int> AttHealth_Changed = new();
    [System.NonSerialized] public UnityEvent<int> AttHealthRegen_Changed = new();
    [System.NonSerialized] public UnityEvent<int> AttArmor_Changed = new();
    [System.NonSerialized] public UnityEvent<int> AttMana_Changed = new();
    [System.NonSerialized] public UnityEvent<int> AttManaRegen_Changed = new();
    [System.NonSerialized] public UnityEvent<int> AttPower_Changed = new();
    [System.NonSerialized] public UnityEvent<int> AttCritChance_Changed = new();
    [System.NonSerialized] public UnityEvent<int> AttCritDmg_Changed = new();
    [System.NonSerialized] public UnityEvent<int> AttCDR_Changed = new();
    [System.NonSerialized] public UnityEvent<List<Skill>> Skills_Changed = new();
    [System.NonSerialized] public UnityEvent<float> DietPercentageChanged = new();


    public ItemPrefabDatabase itemDatabase;
    public ItemScriptableDatabase itemScriptableDatabase;
    public List<TutorialDataScriptable> introductoryTutorial = new();
    public List<TutorialDataScriptable> levelUpTutorial = new();
    public TutorialDataScriptable deathTutorial;

    protected HasHealth healthComp;
    protected HasMana manaComp;
    protected CanMove moveComp;
    protected CanAttack attackComp;
    protected PlayerController playerController;
    protected GameObject recallVFX;
    [SerializeField] protected GameObject spotlight;
    [SerializeField] protected GameObject levelUpEffect;
    public EventReference levelUpSound;
    public TextMeshProUGUI nameTag;

    protected override void Start()
    {
        base.Start();
        healthComp = GetComponent<HasHealth>();
        if (isOwned)
        {
            healthComp.Target_Received.AddListener(TargetReceived);
            healthComp.Received_Target_Lost.AddListener(ReceivedTargetLost);
        }
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
        TalentTreeScriptable talentTree = null;
        foreach (var item in refTalentTrees.talentTrees)
        {
            if (item.treeType == TalentTreeType.Special)
                talentTree = item;
        }
        if (talentTree != null)
        {
            foreach (var item in skillInstances)
            {
                foreach (var item2 in talentTree.talents)
                {
                    item2.SetupEvents(item);
                }
            }
        }
        if (!isOwned && !NetworkServer.active)                      // Sync other players' characters when the client connects
            LoadCharacter();
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
            if (healthComp.GetHealth() > 0)
            {
                hungerTimer += Time.deltaTime;
                if (hungerTimer >= GetHungerInterval())
                {
                    hungerTimer = 0;
                    CmdChangeHunger(-1, false, DietType.None);
                    CmdRemoveBuff("Starving", connectionToClient);
                }
                else if (hunger <= 0 && hungerTimer >= 10)
                {
                    hungerTimer = 0;
                    if (HasBuff("Starving") == 0)
                        CmdAddBuff("Starving", connectionToClient);
                }
                waterTimer += Time.deltaTime;
                if (waterTimer >= GetWaterInterval())
                {
                    waterTimer = 0;
                    CmdChangeWater(-1, false);
                    CmdRemoveBuff("Dehydrated", connectionToClient);
                }
                else if (water <= 0 && waterTimer >= 4)
                {
                    waterTimer = 0;
                    if (HasBuff("Dehydrated") == 0)
                        CmdAddBuff("Dehydrated", connectionToClient);
                    manaComp.CmdSpendMana(manaComp.GetFinalMaxMana() * 0.2f);
                }
            }
            yield return null;
        }
    }
    public override void OnStartAuthority()
    {
        base.OnStartAuthority();    

        LoadCharacter();
    }
    public void LoadCharacterFromServer()
    {
        StartCoroutine(LoadCharacterCoro());
    }
    [Command(requiresAuthority = false)]
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
    [Command(requiresAuthority = false)]
    private void CmdSetName(string name)
    {
        RpcSetName(name);
    }
    [ClientRpc]
    private void RpcSetName(string name)
    {
        nameTag.text = name;
    }
    [ClientRpc]
    public void LoadState(List<SaveDataPlayer> data)
    {
        if (!isLoaded)
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
                        foreach (var item1 in skillInstances)
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
                    carnivorePercentage = item.carnivorePercentage;
                    DietPercentageChanged.Invoke(carnivorePercentage);
                    hungerCount = item.hungerCount;
                    attPower = item.attPower;
                    attMaxMana = item.attMana;
                    attManaRegen = item.attManaRegen;
                    attMaxHealth = item.attHealth;
                    attHealthRegen = item.attHealthRegen;
                    attCooldownReduction = item.attCooldownReduction;
                    attArmor = item.attArmor;
                    attCriticalChance = item.attCritChance;
                    attCriticalDamage = item.attCritDamage;
                    Xp_Changed.Invoke(xp, maxXp);
                    maxHunger = item.maxHunger;
                    CmdSetHunger(item.hunger);
                    hungerInterval = item.hungerInterval;
                    ChangeHungerIntervalMultiplier(1);
                    maxWater = item.maxWater;
                    CmdSetWater(item.water);
                    waterInterval = item.waterInterval;
                    ChangeWaterIntervalMultiplier(1);
                    healthComp.SetBaseMaxHealth(item.baseMaxHealth);
                    healthComp.SetHealth(item.health);
                    healthComp.SetBaseHealthRegen(item.baseHealthRegen);
                    healthComp.SetCorruptedHealth(item.corruptedHealth);
                    manaComp.SetMaxMana(item.baseMaxMana);
                    manaComp.SetMana(item.mana);
                    manaComp.SetBaseManaRegen(item.baseManaRegen);
                    manaComp.SetCorruptedMana(item.corruptedMana);
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
                        AttPower_Changed.Invoke(attPower);
                        AttArmor_Changed.Invoke(attArmor);
                        AttCDR_Changed.Invoke(attCooldownReduction);
                        AttCritChance_Changed.Invoke(attCriticalChance);
                        AttCritDmg_Changed.Invoke(attCriticalDamage);
                        AttHealthRegen_Changed.Invoke(attHealthRegen);
                        AttHealth_Changed.Invoke(attMaxHealth);
                        AttManaRegen_Changed.Invoke(attManaRegen);
                        AttMana_Changed.Invoke(attMaxMana);

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
                        foreach (var item2 in talentTrees.talentTrees)
                        {
                            foreach (var item3 in item2.talents)
                            {
                                UnlockTalent(item3.name, 0, item3.currentLevel);
                            }
                        }
                        foreach (var item5 in item.activebuffs)
                        {
                            AddBuff(item5.name);
                            StartCoroutine(WaitForBuff(item5));
                        }
                        for (int i = 0; i < 5; i++)
                        {
                            skillInstances[i].ExecuteOnStart(this);
                        }
                        if (item.positionX != 0 && item.positionY != 0 && item.positionZ != 0)
                            FindObjectOfType<CameraTarget>().Teleport(new Vector3(item.positionX, item.positionY, item.positionZ));
                        UpdateManualCategories();
                    }
                }
            }
            if (isOwned)
                UpdateSkills();
            isLoaded = true;
            Character_Loaded.Invoke(this);
            healthComp.ChangeCorruptedHealth(0);
            manaComp.ChangeCorruptedMana(0);
            if (isOwned)
                StartCoroutine(UpdatePlayer());
        }
        if (isOwned)
        {
            CmdSetName(SteamFriends.GetPersonaName());
            FindObjectOfType<PartyList>().CmdAddPartyMember(netIdentity);
        }
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
            attArmor = attArmor,
            attCritChance = attCriticalChance,
            attCritDamage = attCriticalDamage,
            attHealth = attMaxHealth,
            attCooldownReduction = attCooldownReduction,
            attHealthRegen = attHealthRegen,
            attMana = attMaxMana,
            attManaRegen = attManaRegen,
            attPower = attPower,
            name = heroName,
            inventory = items,
            equippedGear = gear,
            hunger = hunger,
            maxHunger = maxHunger,
            hungerInterval = hungerInterval,
            hungerCount = hungerCount,
            carnivorePercentage = carnivorePercentage,
            water = water,
            maxWater = maxWater,
            waterInterval = waterInterval,
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
            if (level == 2 && isOwned)
            {
                foreach (var item in levelUpTutorial)
                {
                    FindObjectOfType<Tutorial>().QueueNewTutorial(item);
                }
            }    
            talentTrees.ChangeTalentPoints(1);
            ChangeAttributePoints(1);
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
        FindObjectOfType<RecipeDetail>(true).UpdateCurrentDetails();
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
        FindObjectOfType<RecipeDetail>(true).UpdateCurrentDetails();
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
    [Command(requiresAuthority = false)]
    public void CmdChangeHunger(int amount, bool showText, DietType dietType)
    {
        if (amount > 0)
            amount = (int)(hungerBonus * amount);
        if (showText)
        {
            if (amount > 0)
            {
                if ((carnivorePercentage >= 70 && dietType == DietType.Herbivore) || (carnivorePercentage <= 30 && dietType == DietType.Carnivore))
                    amount = (int)(amount * 0.15f);
                if ((carnivorePercentage >= 70 || carnivorePercentage <= 30) && dietType == DietType.Omnivore)
                    amount = (int)(amount * 0.75f);

                FindObjectOfType<FloatingText>().ServerSpawnFloatingText("+" + amount + " <sprite=12>", transform.position, FloatingTextType.Hunger);
            }
            else
                FindObjectOfType<FloatingText>().ServerSpawnFloatingText(amount + " <sprite=12>", transform.position, FloatingTextType.Hunger);
        }
        RpcChangeHunger(amount, dietType);
    }
    [ClientRpc]
    public void RpcChangeHunger(int amount, DietType dietType)
    {
        hunger += amount;
        if (isOwned)
        {
            if ((hunger == 20 || hunger == 10) && amount < 0)
            {
                FindObjectOfType<SystemMessages>().AddMessage("You are starving.");
            }
            if (amount > 0)
            {
                switch (dietType)
                {
                    case DietType.Carnivore:
                        carnivorePercentage += amount / 2;
                        break;
                    case DietType.Herbivore:
                        carnivorePercentage -= amount / 2;
                        break;
                    case DietType.Omnivore:
                        if (carnivorePercentage > 50)
                        {
                            carnivorePercentage -= amount / 2;
                            if (carnivorePercentage < 50)
                                carnivorePercentage = 50;
                        }
                        if (carnivorePercentage < 50)
                        {
                            carnivorePercentage += amount / 2;
                            if (carnivorePercentage > 50)
                                carnivorePercentage = 50;
                        }
                        break;
                    default:
                        break;
                }
                if (carnivorePercentage > 100)
                    carnivorePercentage = 100;
                if (carnivorePercentage < 0)
                    carnivorePercentage = 0;
                DietPercentageChanged.Invoke(carnivorePercentage);
                FindObjectOfType<AudioManager>().EatFood(transform.position);
                hungerCount += amount;
                if (hungerCount >= 50)
                {
                    hungerCount = 0;
                    talentTrees.ChangeTalentPoints(1);
                    FindObjectOfType<SystemMessages>().AddMessage("Talent Point acquired!", MsgType.Positive);
                }
            }
        }
        Hunger_Changed.Invoke();
    }
    [Command(requiresAuthority = false)]
    public void CmdChangeMaxHunger(int amount)
    {
        RpcChangeMaxHunger(amount);
    }
    [ClientRpc]
    public void RpcChangeMaxHunger(int amount)
    {
        maxHunger += amount;
        if (hunger > maxHunger)
            hunger = maxHunger;
        Hunger_Changed.Invoke();
    }
    [Command(requiresAuthority = false)]
    public void CmdSetHunger(int amount)
    {
        RpcSetHunger(amount);
    }
    [ClientRpc]
    public void RpcSetHunger(int amount)
    {
        hunger = amount;
        Hunger_Changed.Invoke();
    }
    [Command(requiresAuthority = false)]
    public void CmdChangeWater(int amount, bool showText)
    {
        if (showText)
        {
            if (amount > 0)
            {
                FindObjectOfType<FloatingText>().ServerSpawnFloatingText("+" + amount + " <sprite=20>", transform.position + Vector3.down * 0.5f, FloatingTextType.Hydration);
            }
            else
                FindObjectOfType<FloatingText>().ServerSpawnFloatingText(amount + " <sprite=20>", transform.position + Vector3.down * 0.5f, FloatingTextType.Hydration);
        }
        RpcChangeWater(amount);
    }
    [ClientRpc]
    public void RpcChangeWater(int amount)
    {
        water += amount;
        if (water > maxWater)
            water = maxWater;
        if (isOwned)
        {
            if ((water == 20 || water == 10) && amount < 0)
            {
                FindObjectOfType<SystemMessages>().AddMessage("You are dehydrated.");
            }
            if (amount > 0)
                FindObjectOfType<AudioManager>().DrinkWater(transform.position);
        }
        Water_Changed.Invoke();
    }
    [Command(requiresAuthority = false)]
    public void CmdSetWater(int amount)
    {
        RpcSetWater(amount);
    }
    [ClientRpc]
    public void RpcSetWater(int amount)
    {
        water = amount;
        Water_Changed.Invoke();
    }
    public void ChangeStat(PlayerStat playerStat, float modifier, DietType dietType = DietType.None)
    {
        switch (playerStat)
        {
            case PlayerStat.Health:
                if (modifier > 0)
                    healthComp.CmdHealDamage(modifier, false);
                else
                    healthComp.CmdTakeDamage(modifier, true, GetComponent<NetworkIdentity>(), false, true, false);
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
                CmdChangeHunger((int)modifier, true, dietType);
                break;
            case PlayerStat.MaxHunger:
                break;
            case PlayerStat.Hydration:
                CmdChangeWater((int)modifier, true);
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
            case PlayerStat.PowerMultiplier:
                GetComponent<CanAttack>().ChangePowerMultiplier(modifier);
                break;
            default:
                break;
        }
        Skills_Changed.Invoke(skillInstances);
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
                if (HasBuff("Poisoned") > 0)
                {
                    FindObjectOfType<SystemMessages>().AddMessage("You can't eat while poisoned. You would throw up immediately...");
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
                    FindObjectOfType<SystemMessages>().AddMessage("You are not corrupted.");
                    return false;
                }
                if (GetComponent<HasMana>().GetCorruptedMana() <= 0)
                {
                    FindObjectOfType<SystemMessages>().AddMessage("You are not corrupted.");
                    return false;
                }
                break;
            case PlayerStat.Hydration:
                break;
            case PlayerStat.PowerMultiplier:
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
        nameTag.gameObject.SetActive(false);
        moveComp.agent.enabled = false;
        playerController.CmdChangeState(PlayerState.OutOfGame);
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
        nameTag.gameObject.SetActive(true);
        moveComp.agent.enabled = true;
        playerController.CmdChangeState(PlayerState.None);
        spotlight.SetActive(true);
    }
    public void AddMaxHealthAttribute(int value)
    {
        attMaxHealth += value;
        ChangeAttributePoints(-value);
        healthComp.CmdChangeBaseMaxHealth(value * 15);
        AttHealth_Changed.Invoke(attMaxHealth);
    }
    public void AddHealthRegenAttribute(int value)
    {
        attHealthRegen += value;
        ChangeAttributePoints(-value);
        healthComp.CmdChangeBaseHealthRegen(value * 0.1f);
        AttHealthRegen_Changed.Invoke(attHealthRegen);
    }
    public void AddArmorAttribute(int value)
    {
        attArmor += value;
        ChangeAttributePoints(-value);
        healthComp.CmdChangeArmor(value * 0.5f);
        AttArmor_Changed.Invoke(attArmor);
    }
    public void AddMaxManaAttribute(int value)
    {
        attMaxMana += value;
        ChangeAttributePoints(-value);
        manaComp.CmdChangeBaseMaxMana(value * 15);
        AttMana_Changed.Invoke(attMaxMana);
    }
    public void AddManaRegenAttribute(int value)
    {
        attManaRegen += value;
        ChangeAttributePoints(-value);
        manaComp.CmdChangeBaseManaRegen(value * 0.1f);
        AttManaRegen_Changed.Invoke(attManaRegen);
    }
    public void AddPowerAttribute(int value)
    {
        attPower += value;
        ChangeAttributePoints(-value);
        attackComp.CmdChangePower(value * 1);
        AttPower_Changed.Invoke(attPower);
    }
    public void AddCriticalChanceAttribute(int value)
    {
        attCriticalChance += value;
        ChangeAttributePoints(-value);
        attackComp.CmdChangeCriticalChance(value * 0.5f);
        AttCritChance_Changed.Invoke(attCriticalChance);
    }
    public void AddCriticalDamageAttribute(int value)
    {
        attCriticalDamage += value;
        ChangeAttributePoints(-value);
        attackComp.CmdChangeCriticalDamage(value * 1);
        AttCritDmg_Changed.Invoke(attCriticalDamage);
    }
    public void AddCooldownReductionAttribute(int value)
    {
        attCooldownReduction += value;
        ChangeAttributePoints(-value);
        attackComp.CmdChangeCooldownReduction(1f);
        AttCDR_Changed.Invoke(attCooldownReduction);
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
    [Command(requiresAuthority = false)]
    public void CmdChangeWaterIntervalMultiplier(float value)
    {
        RpcChangeWaterIntervalMultiplier(value);
    }
    [ClientRpc]
    public void RpcChangeWaterIntervalMultiplier(float value)
    {
        ChangeWaterIntervalMultiplier(value);
    }
    public void ChangeWaterIntervalMultiplier(float value)
    {
        waterIntervalMultiplier += value;
        Water_Changed.Invoke();
    }
    public float GetWaterInterval()
    {
        return waterInterval * waterIntervalMultiplier;
    }
    protected override void OnDeath()
    {
        base.OnDeath();
        if (isOwned)
        {
            FindObjectOfType<RespawnUI>().Show();
            FindObjectOfType<AudioManager>().StopCombatMusic();
        }
    }
    public override void Revive(Vector3 position, float hpPercentage)
    {
        base.Revive(position, hpPercentage);
        animator.SetTrigger(animHash_Revive);
        if (isOwned)
        {
            if (hunger > 40)
                CmdChangeHunger(-20, true, DietType.None);
            else
                CmdSetHunger(20);
        }
        if (isOwned)
            FindObjectOfType<CameraTarget>().CenterCamera(false);
    }
    public void DeathTutorial()
    {
        if (deathTutorial)
        {
            FindObjectOfType<Tutorial>().QueueNewTutorial(deathTutorial);
            deathTutorial = null;
        }
    }
    private void TargetReceived(HasHealth target)
    {
        FindObjectOfType<AudioManager>().TargetReceived(target);
    }
    private void ReceivedTargetLost(HasHealth target)
    {
        FindObjectOfType<AudioManager>().ReceivedTargetLost(target);
    }
    public bool IsTentNearby()
    {
        foreach (var item in FindObjectsOfType<Tent>(true))
        {
            if (Vector3.Distance(item.transform.position, transform.position) <= 10)
                return true;
        }
        return false;
    }
    // Talent Unlock and Reset
    public void UnlockTalent(string name, int previousLevel, int currentLevel)
    {
        // Nirri Talents
        if (name == "Nature Attunement")
            NatureAttunement(previousLevel, currentLevel);
        if (name == "Mana Mastery")
            ManaMastery(previousLevel, currentLevel);
        if (name == "Defilement")
            Defilement(previousLevel, currentLevel);
        if (name == "Prolonged Magic")
            ProlongedMagic(previousLevel, currentLevel);
        if (name == "Healing Dust")
            HealingDust(previousLevel, currentLevel);
        if (name == "Persistent Roots")
            PersistentRoots(previousLevel, currentLevel);
        if (name == "Corrupted Dust")
            CorruptedDust(previousLevel, currentLevel);
        if (name == "Deep Roots")
            DeepRoots(previousLevel, currentLevel);
        if (name == "Photosynthesis Level 1")
            Photosynthesis1(previousLevel, currentLevel);
        if (name == "Photosynthesis Level 2")
            Photosynthesis2(previousLevel, currentLevel);
        if (name == "Photosynthesis Level 3")
            Photosynthesis3(previousLevel, currentLevel);
        if (name == "Photosynthesis Level 4")
            Photosynthesis4(previousLevel, currentLevel);
        if (name == "Regenerating Dust")
            RegeneratingDust(previousLevel, currentLevel);
        if (name == "Glass Cannon")
            GlassCannon(previousLevel, currentLevel);

        // Wolferius Talents
        if (name == "Adrenaline Rush")
            AdrenalineRush(previousLevel, currentLevel);
        if (name == "Disarming Uppercut")
            DisarmingUppercut(previousLevel, currentLevel);
        if (name == "Endurance")
            Endurance(previousLevel, currentLevel); 
        if (name == "Hasted Attacks")
            HastedAttacks(previousLevel, currentLevel);
        if (name == "Piercing Uppercut")
            PiercingUppercut(previousLevel, currentLevel);
        if (name == "Regenerative Rage")
            RegenerativeRage(previousLevel, currentLevel);  
        if (name == "Reinforcements of the Wild")
            ReinforcementsOfTheWild(previousLevel, currentLevel);   
        if (name == "Restorative Cuts")
            RestorativeCuts(previousLevel, currentLevel);
        if (name == "Toughness")
            Toughness(previousLevel, currentLevel); 
        if (name == "Way of the Lupine")
            WayOfTheLupine(previousLevel, currentLevel);    
        if (name == "Way of the Sapiens")
            WayOfTheSapiens(previousLevel, currentLevel);
        if (name == "Wild Companion")
            WildCompanion(previousLevel, currentLevel);

        // Diet Talents
        if (name == "Protein Rush")
            ProteinRush(previousLevel, currentLevel);
        if (name == "Tough Body")
            ToughBody(previousLevel, currentLevel);
        if (name == "Animal Feeder")
            AnimalFeeder(previousLevel, currentLevel);

        if (name == "Chlorophyll Surge")
            ChlorophyllSurge(previousLevel, currentLevel);
        if (name == "Energy Reserves")
            EnergyReserves(previousLevel, currentLevel);
        if (name == "Potato Seed")
            PotatoSeed(previousLevel, currentLevel);

        if (name == "Balanced Bite")
            BalancedBite(previousLevel, currentLevel);
        if (name == "Energic Body")
            EnergicBody(previousLevel, currentLevel);
        if (name == "Feast")
            Feast(previousLevel, currentLevel);
        if (name == "Blood Extractor")
            BloodExtractor(previousLevel, currentLevel);

        if (name.Contains("Expanded Stomach"))
            ExpandedStomach(previousLevel, currentLevel);
        if (name.Contains("Efficient Metabolism"))
            CmdEfficientMetabolism(previousLevel, currentLevel);

        UpdateSkills();
    }
    public void ResetTalent(string name, int previousLevel, int currentLevel)
    {
        // Forest Protector Talents
        if (name == "Nature Attunement")
            NatureAttunementReduce(previousLevel, currentLevel);
        if (name == "Mana Mastery")
            ManaMasteryReduce(previousLevel, currentLevel);
        if (name == "Defilement")
            DefilementReduce(previousLevel, currentLevel);
        if (name == "Prolonged Magic")
            ProlongedMagicReduce(previousLevel, currentLevel);
        if (name == "Healing Dust")
            HealingDustReduce(previousLevel, currentLevel);
        if (name == "Persistent Roots")
            PersistentRootsReduce(previousLevel, currentLevel);
        if (name == "Corrupted Dust")
            RestoreGreenDust(previousLevel, currentLevel);
        if (name == "Deep Roots")
            DeepRootsReduce(previousLevel, currentLevel);
        if (name == "Photosynthesis Level 1")
            Photosynthesis1Reduce(previousLevel, currentLevel);
        if (name == "Photosynthesis Level 2")
            Photosynthesis2Reduce(previousLevel, currentLevel);
        if (name == "Photosynthesis Level 3")
            Photosynthesis3Reduce(previousLevel, currentLevel);
        if (name == "Photosynthesis Level 4")
            Photosynthesis4Reduce(previousLevel, currentLevel);
        if (name == "Regenerating Dust")
            RestoreGreenDust(previousLevel, currentLevel);
        if (name == "Glass Cannon")
            GlassCannonReduce(previousLevel, currentLevel);

        // Lycandruid Talents
        if (name == "Adrenaline Rush")
            AdrenalineRushReduce(previousLevel, currentLevel);
        if (name == "Disarming Uppercut")
            DisarmingUppercutReduce(previousLevel, currentLevel);
        if (name == "Endurance")
            EnduranceReduce(previousLevel, currentLevel);
        if (name == "Hasted Attacks")
            HastedAttacksReduce(previousLevel, currentLevel);
        if (name == "Piercing Uppercut")
            PiercingUppercutReduce(previousLevel, currentLevel);
        if (name == "Regenerative Rage")
            RegenerativeRageReduce(previousLevel, currentLevel);
        if (name == "Reinforcements of the Wild")
            ReinforcementsOfTheWildReduce(previousLevel, currentLevel);
        if (name == "Restorative Cuts")
            RestorativeCutsReduce(previousLevel, currentLevel);
        if (name == "Toughness")
            ToughnessReduce(previousLevel, currentLevel);
        if (name == "Way of the Lupine" || name == "Way of the Sapiens")
            RevertPermanentShapeshift(previousLevel, currentLevel);
        if (name == "Wild Companion")
            WildCompanionReduce(previousLevel, currentLevel);

        // Diet Talents
        if (name == "Protein Rush")
            ProteinRushReduce(previousLevel, currentLevel);
        if (name == "Tough Body")
            ToughBodyReduce(previousLevel, currentLevel);
        if (name == "Animal Feeder")
            AnimalFeederReduce(previousLevel, currentLevel);

        if (name == "Chlorophyll Surge")
            ChlorophyllSurgeReduce(previousLevel, currentLevel);
        if (name == "Energy Reserves")
            EnergyReservesReduce(previousLevel, currentLevel);
        if (name == "Potato Seed")
            PotatoSeedReduce(previousLevel, currentLevel);

        if (name == "Balanced Bite")
            BalancedBiteReduce(previousLevel, currentLevel);
        if (name == "Energic Body")
            EnergicBodyReduce(previousLevel, currentLevel);
        if (name == "Feast")
            FeastReduce(previousLevel, currentLevel);
        if (name == "Blood Extractor")
            BloodExtractorReduce(previousLevel, currentLevel);

        if (name.Contains("Expanded Stomach"))
            ExpandedStomachReduce(previousLevel, currentLevel);
        if (name.Contains("Efficient Metabolism"))
            CmdEfficientMetabolism(previousLevel, currentLevel);
    }
    // Forest Protector Talents
    private void NatureAttunement(int previousLevel, int currentLevel)
    {
        for (int i = previousLevel; i < currentLevel; i++)
        {
            manaComp.CmdChangeGearManaRegen(0.5f);
        }
    }
    private void NatureAttunementReduce(int previousLevel, int currentLevel)
    {
        for (int i = previousLevel; i > currentLevel; i--)
        {
            manaComp.CmdChangeGearManaRegen(-0.5f);
        }
    }
    private void ManaMastery(int previousLevel, int currentLevel)
    {
        for (int i = previousLevel; i < currentLevel; i++)
        {
            GetComponent<ForestProtector>().LearnManaMastery();
        }
    }
    private void ManaMasteryReduce(int previousLevel, int currentLevel)
    {
        for (int i = previousLevel; i > currentLevel; i--)
        {
            GetComponent<ForestProtector>().UnlearnManaMastery();
        }
    }
    private void Defilement(int previousLevel, int currentLevel)
    {
        if (currentLevel >= 1)
        {
            skillInstances.RemoveAt(3);
            skillInstances.Insert(3, skills.Find((x) => x is SDefilement).GetInstance());
            skillInstances[3].ExecuteOnStart(this);
            UpdateSkills();
        }
    }
    private void DefilementReduce(int previousLevel, int currentLevel)
    {
        if (currentLevel <= 0)
        {
            skillInstances.RemoveAt(3);
            skillInstances.Insert(3, skills.Find((x) => x is SRejuvenation).GetInstance());
            skillInstances[3].ExecuteOnStart(this);
            UpdateSkills();
        }
    }
    private void HealingDust(int previousLevel, int currentLevel)
    {
        for (int i = previousLevel; i < currentLevel; i++)
        {
            GetComponent<ForestProtector>().LearnHealingDust();
            skillInstances[2].manaCost += 10;
        }
    }
    private void HealingDustReduce(int previousLevel, int currentLevel)
    {
        for (int i = previousLevel; i > currentLevel; i--)
        {
            GetComponent<ForestProtector>().UnlearnHealingDust();
            skillInstances[2].manaCost -= 10;
        }
    }
    private void ProlongedMagic(int previousLevel, int currentLevel)
    {
        for (int i = previousLevel; i < currentLevel; i++)
        {
            var rejSkillInst = skillInstances.Find((x) => x is SRejuvenation);
            if (rejSkillInst)
                (rejSkillInst as SRejuvenation).baseDuration += 2;
            var defSkillInst = skillInstances.Find((x) => x is SDefilement);
            if (defSkillInst)
                (defSkillInst as SDefilement).baseDuration += 2;
        }
    }
    private void ProlongedMagicReduce(int previousLevel, int currentLevel)
    {
        for (int i = previousLevel; i > currentLevel; i--)
        {
            var rejSkillInst = skillInstances.Find((x) => x is SRejuvenation);
            if (rejSkillInst)
                (rejSkillInst as SRejuvenation).baseDuration -= 2;
            var defSkillInst = skillInstances.Find((x) => x is SDefilement);
            if (defSkillInst)
                (defSkillInst as SDefilement).baseDuration -= 2;
        }
    }
    private void PersistentRoots(int previousLevel, int currentLevel)
    {
        for (int i = previousLevel; i < currentLevel; i++)
        {
            (skillInstances.Find((x) => x is SEntanglingRoots) as SEntanglingRoots).baseDuration += 1;
        }
    }
    private void PersistentRootsReduce(int previousLevel, int currentLevel)
    {
        for (int i = previousLevel; i > currentLevel; i--)
        {
            (skillInstances.Find((x) => x is SEntanglingRoots) as SEntanglingRoots).baseDuration -= 1;
        }
    }
    private void CorruptedDust(int previousLevel, int currentLevel)
    {
        if (currentLevel >= 1)
        {
            skillInstances.RemoveAt(2);
            skillInstances.Insert(2, skills.Find((x) => x is SCorruptedDust).GetInstance());
            skillInstances[2].ExecuteOnStart(this);
            UpdateSkills();
        }
    }
    private void RestoreGreenDust(int previousLevel, int currentLevel)
    {
        if (currentLevel <= 0)
        {
            skillInstances.RemoveAt(2);
            skillInstances.Insert(2, skills.Find((x) => x is SGreenDust).GetInstance());
            skillInstances[2].ExecuteOnStart(this);
            UpdateSkills();
        }
    }
    private void RegeneratingDust(int previousLevel, int currentLevel)
    {
        if (currentLevel >= 1)
        {
            skillInstances.RemoveAt(2);
            skillInstances.Insert(2, skills.Find((x) => x is SRegeneratingDust).GetInstance());
            skillInstances[2].ExecuteOnStart(this);
            UpdateSkills();
        }
    }
    private void DeepRoots(int previousLevel, int currentLevel)
    {
        for (int i = previousLevel; i < currentLevel; i++)
        {
            (skillInstances.Find((x) => x is SFlowerPower) as SFlowerPower).timedLife += 3;
        }
    }
    private void DeepRootsReduce(int previousLevel, int currentLevel)
    {
        for (int i = previousLevel; i > currentLevel; i--)
        {
            (skillInstances.Find((x) => x is SFlowerPower) as SFlowerPower).timedLife -= 3;
        }
    }
    private void Photosynthesis1(int previousLevel, int currentLevel)
    {
        if (currentLevel >= 1)
        {
            PhotosynthesisUnlock(new List<int> { 0, 1 });
        }
    }
    private void Photosynthesis1Reduce(int previousLevel, int currentLevel)
    {
        if (currentLevel <= 0)
        {
            PhotosynthesisLock(new List<int> { 0, 1 });
        }
    }
    private void Photosynthesis2(int previousLevel, int currentLevel)
    {
        if (currentLevel >= 1)
        {
            PhotosynthesisUnlock(new List<int> { 2, 3 });
        }
    }
    private void Photosynthesis2Reduce(int previousLevel, int currentLevel)
    {
        if (currentLevel <= 0)
        {
            PhotosynthesisLock(new List<int> { 2, 3 });
        }
    }
    private void Photosynthesis3(int previousLevel, int currentLevel)
    {
        if (currentLevel >= 1)
        {
            PhotosynthesisUnlock(new List<int> { 4, 5 });
        }
    }
    private void Photosynthesis3Reduce(int previousLevel, int currentLevel)
    {
        if (currentLevel <= 0)
        {
            PhotosynthesisLock(new List<int> { 4, 5 });
        }
    }
    private void Photosynthesis4(int previousLevel, int currentLevel)
    {
        if (currentLevel >= 1)
        {
            PhotosynthesisUnlock(new List<int> { 6 });
        }
    }
    private void Photosynthesis4Reduce(int previousLevel, int currentLevel)
    {
        if (currentLevel <= 0)
        {
            PhotosynthesisLock(new List<int> { 6 });
        }
    }
    private void PhotosynthesisUnlock(List<int> indexes)
    {
        var photosynthesis = skills[1] as SPhotosynthesis;
        foreach (var item in FindObjectsOfType<PhotosynthesisButton>(true))
        {
            foreach (var index in indexes)
                if (item.plant == photosynthesis.possiblePlants[index])
                    item.UnlockButton();
        }
    }
    private void PhotosynthesisLock(List<int> indexes)
    {
        var photosynthesis = skills[1] as SPhotosynthesis;
        foreach (var item in FindObjectsOfType<PhotosynthesisButton>(true))
        {
            foreach (var index in indexes)
                if (item.plant == photosynthesis.possiblePlants[index])
                    item.LockButton();
        }
    }
    private void GlassCannon(int previousLevel, int currentLevel)
    {
        if (currentLevel >= 1)
        {
            GetComponent<HasHealth>().CmdChangeMaxHealthMultiplier(-0.5f);
            GetComponent<CanAttack>().CmdChangePowerMultiplier(0.5f);
        }
    }
    private void GlassCannonReduce(int previousLevel, int currentLevel)
    {
        if (currentLevel <= 0)
        {
            GetComponent<HasHealth>().CmdChangeMaxHealthMultiplier(0.5f);
            GetComponent<CanAttack>().CmdChangePowerMultiplier(-0.5f);
        }
    }
    // Lycandruid Talents
    private void Endurance(int previousLevel, int currentLevel)
    {
        for (int i = previousLevel; i < currentLevel; i++)
        {
            GetComponent<HasHealth>().CmdChangeGearHealthRegen(0.2f);
        }
    }
    private void EnduranceReduce(int previousLevel, int currentLevel)
    {
        for (int i = previousLevel; i > currentLevel; i--)
        {
            GetComponent<HasHealth>().CmdChangeGearHealthRegen(-0.2f);
        }
    }
    private void Toughness(int previousLevel, int currentLevel)
    {
        for (int i = previousLevel; i < currentLevel; i++)
        {
            GetComponent<HasHealth>().CmdChangeGearMaxHealth(50);
        }
    }
    private void ToughnessReduce(int previousLevel, int currentLevel)
    {
        for (int i = previousLevel; i > currentLevel; i--)
        {
            GetComponent<HasHealth>().CmdChangeGearMaxHealth(-50);
        }
    }
    private void AdrenalineRush(int previousLevel, int currentLevel)
    {
        for (int i = previousLevel; i < currentLevel; i++)
        {
            GetComponent<Lycandruid>().CmdLearnAdrenalineRush();
        }
    }
    private void AdrenalineRushReduce(int previousLevel, int currentLevel)
    {
        for (int i = previousLevel; i > currentLevel; i--)
        {
            GetComponent<Lycandruid>().CmdUnlearnAdrenalineRush();
        }
    }
    private void DisarmingUppercut(int previousLevel, int currentLevel)
    {
        for (int i = previousLevel; i < currentLevel; i++)
        {
            (GetComponent<Shapeshifter>().defaultSkillInstances[3] as SUppercut).damageBaseReduction += 5;
        }
    }
    private void DisarmingUppercutReduce(int previousLevel, int currentLevel)
    {
        for (int i = previousLevel; i > currentLevel; i--)
        {
            (GetComponent<Shapeshifter>().defaultSkillInstances[3] as SUppercut).damageBaseReduction -= 5;
        }
    }
    private void PiercingUppercut(int previousLevel, int currentLevel)
    {
        for (int i = previousLevel; i < currentLevel; i++)
        {
            (GetComponent<Shapeshifter>().defaultSkillInstances[3] as SUppercut).armorBaseReduction += 5;
        }
    }
    private void PiercingUppercutReduce(int previousLevel, int currentLevel)
    {
        for (int i = previousLevel; i > currentLevel; i--)
        {
            (GetComponent<Shapeshifter>().defaultSkillInstances[3] as SUppercut).armorBaseReduction -= 5;
        }
    }
    private void HastedAttacks(int previousLevel, int currentLevel)
    {
        for (int i = previousLevel; i < currentLevel; i++)
        {
            var shapeshift = GetComponent<Shapeshifter>();
            (shapeshift.defaultSkillInstances[2] as SSwipe).cooldown -= 1;
            (shapeshift.shapeshiftedSkillInstances[2] as SBite).cooldown -= 1;
        }
    }
    private void HastedAttacksReduce(int previousLevel, int currentLevel)
    {
        for (int i = previousLevel; i > currentLevel; i--)
        {
            var shapeshift = GetComponent<Shapeshifter>();
            (shapeshift.defaultSkillInstances[2] as SSwipe).cooldown += 1;
            (shapeshift.shapeshiftedSkillInstances[2] as SBite).cooldown += 1;
        }
    }
    private void RegenerativeRage(int previousLevel, int currentLevel)
    {
        for (int i = previousLevel; i < currentLevel; i++)
        {
            GetComponent<Lycandruid>().CmdLearnRegenerativeRage();
        }
    }
    private void RegenerativeRageReduce(int previousLevel, int currentLevel)
    {
        for (int i = previousLevel; i > currentLevel; i--)
        {
            GetComponent<Lycandruid>().CmdUnlearnRegenerativeRage();
        }
    }
    private void RestorativeCuts(int previousLevel, int currentLevel)
    {
        for (int i = previousLevel; i < currentLevel; i++)
        {
            GetComponent<Lycandruid>().CmdLearnCriticalSustain();
        }
    }
    private void RestorativeCutsReduce(int previousLevel, int currentLevel)
    {
        for (int i = previousLevel; i > currentLevel; i--)
        {
            GetComponent<Lycandruid>().CmdUnlearnCriticalSustain();
        }
    }
    private void ReinforcementsOfTheWild(int previousLevel, int currentLevel)
    {
        for (int i = previousLevel; i < currentLevel; i++)
        {
            var shapeshift = GetComponent<Shapeshifter>();
            (shapeshift.shapeshiftedSkillInstances[5] as SCallOfTheWild).duration += 1;
            (shapeshift.shapeshiftedSkillInstances[5] as SCallOfTheWild).baseNumberOfWolves += 1;
        }
    }
    private void ReinforcementsOfTheWildReduce(int previousLevel, int currentLevel)
    {
        for (int i = previousLevel; i < currentLevel; i++)
        {
            var shapeshift = GetComponent<Shapeshifter>();
            (shapeshift.shapeshiftedSkillInstances[5] as SCallOfTheWild).duration -= 1;
            (shapeshift.shapeshiftedSkillInstances[5] as SCallOfTheWild).baseNumberOfWolves -= 1;
        }
    }
    private void WayOfTheLupine(int previousLevel, int currentLevel)
    {
        for (int i = previousLevel; i < currentLevel; i++)
        {
            GetComponent<Shapeshifter>().CmdPermanentShapeshift(false);
        }
    }
    private void WayOfTheSapiens(int previousLevel, int currentLevel)
    {
        for (int i = previousLevel; i < currentLevel; i++)
        {
            GetComponent<Shapeshifter>().CmdPermanentShapeshift(true);
        }
    }
    private void RevertPermanentShapeshift(int previousLevel, int currentLevel)
    {
        for (int i = previousLevel; i > currentLevel; i--)
        {
            GetComponent<Shapeshifter>().CmdRevertPermanentShapeshift();
        }
    }
    private void WildCompanion(int previousLevel, int currentLevel)
    {
        for (int i = previousLevel; i < currentLevel; i++)
        {
            GetComponent<Lycandruid>().CmdLearnWildCompanion();
        }
    }
    private void WildCompanionReduce(int previousLevel, int currentLevel)
    {
        for (int i = previousLevel; i > currentLevel; i--)
        {
            GetComponent<Lycandruid>().CmdUnlearnWildCompanion();
        }
    }
    // Diet Talents
    // Carnivore
    private void ProteinRush(int previousLevel, int currentLevel)
    {
        for (int i = previousLevel; i < currentLevel; i++) 
        {
            attackComp.CmdChangeGearPower(1);
        }
    }
    private void ProteinRushReduce(int previousLevel, int currentLevel)
    {
        for (int i = previousLevel; i > currentLevel; i--)
        {
            attackComp.CmdChangeGearPower(-1);
        }
    }
    private void ToughBody(int previousLevel, int currentLevel)
    {
        for (int i = previousLevel; i < currentLevel; i++)
        {
            healthComp.CmdChangeGearMaxHealth(20);
        }
    }
    private void ToughBodyReduce(int previousLevel, int currentLevel)
    {
        for (int i = previousLevel; i > currentLevel; i--)
        {
            healthComp.CmdChangeGearMaxHealth(-20);
        }
    }
    private void AnimalFeeder(int previousLevel, int currentLevel)
    {
        if (currentLevel >= 1)
            FindObjectOfType<GameManager>().recipeDatabase.GetRecipeByName("Animal Feeder").UnlockRecipe();
    }
    private void AnimalFeederReduce(int previousLevel, int currentLevel)
    {
        if (currentLevel <= 0)
            FindObjectOfType<GameManager>().recipeDatabase.GetRecipeByName("Animal Feeder").LockRecipe();
    }
    // Herbivore
    private void ChlorophyllSurge(int previousLevel, int currentLevel)
    {
        for (int i = previousLevel; i < currentLevel; i++)
        {
            manaComp.CmdChangeGearManaRegen(0.1f);
        }
    }
    private void ChlorophyllSurgeReduce(int previousLevel, int currentLevel)
    {
        for (int i = previousLevel; i > currentLevel; i--)
        {
            manaComp.CmdChangeGearManaRegen(-0.1f);
        }
    }
    private void EnergyReserves(int previousLevel, int currentLevel)
    {
        for (int i = previousLevel; i < currentLevel; i++)
        {
            manaComp.CmdChangeGearMaxMana(20);
        }
    }
    private void EnergyReservesReduce(int previousLevel, int currentLevel)
    {
        for (int i = previousLevel; i > currentLevel; i--)
        {
            manaComp.CmdChangeGearMaxMana(-20);
        }
    }
    private void PotatoSeed(int previousLevel, int currentLevel)
    {
        if (currentLevel >= 1)
            FindObjectOfType<GameManager>().recipeDatabase.GetRecipeByName("Potato Seed").UnlockRecipe();
    }
    private void PotatoSeedReduce(int previousLevel, int currentLevel)
    {
        if (currentLevel <= 0)
            FindObjectOfType<GameManager>().recipeDatabase.GetRecipeByName("Potato Seed").LockRecipe();
    }
    // Omnivore
    private void BalancedBite(int previousLevel, int currentLevel)
    {
        for (int i = previousLevel; i < currentLevel; i++)
        {
            healthComp.CmdChangeGearMaxHealth(15);
        }
    }
    private void BalancedBiteReduce(int previousLevel, int currentLevel)
    {
        for (int i = previousLevel; i > currentLevel; i--)
        {
            healthComp.CmdChangeGearMaxHealth(-15);
        }
    }
    private void EnergicBody(int previousLevel, int currentLevel)
    {
        for (int i = previousLevel; i < currentLevel; i++)
        {
            healthComp.CmdChangeGearMaxHealth(10);
            manaComp.CmdChangeGearMaxMana(10);
        }
    }
    private void EnergicBodyReduce(int previousLevel, int currentLevel)
    {
        for (int i = previousLevel; i > currentLevel; i--)
        {
            healthComp.CmdChangeGearMaxHealth(-10);
            manaComp.CmdChangeGearMaxMana(-10);
        }
    }
    private void Feast(int previousLevel, int currentLevel)
    {
        if (currentLevel >= 1)
            FindObjectOfType<GameManager>().recipeDatabase.GetRecipeByName("Feast").UnlockRecipe();
    }
    private void FeastReduce(int previousLevel, int currentLevel)
    {
        if (currentLevel <= 0)
            FindObjectOfType<GameManager>().recipeDatabase.GetRecipeByName("Feast").LockRecipe();
    }
    private void ExpandedStomach(int previousLevel, int currentLevel)
    {
        for (int i = previousLevel; i < currentLevel; i++)
        {
            CmdChangeMaxHunger(10);
        }
    }
    private void ExpandedStomachReduce(int previousLevel, int currentLevel)
    {
        for (int i = previousLevel; i > currentLevel; i--)
        {
            CmdChangeMaxHunger(-10);
        }
    }
    [Command(requiresAuthority = false)]
    private void CmdEfficientMetabolism(int previousLevel, int currentLevel)
    {
        RpcEfficientMetabolism(previousLevel, currentLevel);
    }
    [ClientRpc]
    private void RpcEfficientMetabolism(int previousLevel, int currentLevel)
    {
        if (currentLevel == 1)
            hungerBonus = 1.15f;
        if (currentLevel == 2)
            hungerBonus = 1.20f;
        if (currentLevel == 3)
            hungerBonus = 1.30f;
    }
    private void BloodExtractor(int previousLevel, int currentLevel)
    {
        if (currentLevel >= 1)
        {
            FindObjectOfType<GameManager>().recipeDatabase.GetRecipeByName("Extract Blood").UnlockRecipe();
            FindObjectOfType<GameManager>().recipeDatabase.GetRecipeByName("Extract Corrupted Blood").UnlockRecipe();
        }
    }
    private void BloodExtractorReduce(int previousLevel, int currentLevel)
    {
        if (currentLevel <= 0)
        {
            FindObjectOfType<GameManager>().recipeDatabase.GetRecipeByName("Extract Blood").LockRecipe();
            FindObjectOfType<GameManager>().recipeDatabase.GetRecipeByName("Extract Corrupted Blood").LockRecipe();
        }
    }
}
