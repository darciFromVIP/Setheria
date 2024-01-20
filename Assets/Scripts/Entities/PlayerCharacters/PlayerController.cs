using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;
using FoW;
using UnityEngine.EventSystems;
using System.Linq;
using HighlightPlus;

public enum PlayerState
{
    None, Busy, Casting, Working, OutOfGame
}
public enum CastingState
{
    None, AllyOnly, EnemyOnly, Both, BothExceptSelf, AllyExceptSelf
}
[RequireComponent(typeof(CanMove))]
public class PlayerController : NetworkBehaviour
{
    private CanMove moveComp;
    private CanAttack attackComp;
    private CanPickup pickupComp;
    private HasMana manaComp;
    private PlayerCharacter playerCharacter;
    public PlayerState state;
    private Skill currentSkill;
    public CastingState castingState;
    private List<Collider> collidingColliders = new();
    public LayerMask movementLayerMask;
    public LayerMask spellCastingLayerMask;
    private GameObject clickEffect;
    private SettingsManager settingsManager;
    public InputEnabledScriptable inputEnabled;
    private Character targetedCharacter;


    public float cooldown1;
    public float cooldown2;
    public float cooldown3;
    public float cooldown4;
    public float cooldown5;

    public UnityEvent<NetworkIdentity> Enemy_Clicked = new();
    [HideInInspector] public UnityEvent<Vector3> Ground_Left_Clicked = new();
    [HideInInspector] public UnityEvent<EnemyCharacter> Enemy_Left_Clicked = new();
    [HideInInspector] public UnityEvent<PlayerCharacter> Ally_Left_Clicked = new();
    [HideInInspector] public UnityEvent Cooldown_1_Started = new();
    [HideInInspector] public UnityEvent Cooldown_2_Started = new();
    [HideInInspector] public UnityEvent Cooldown_3_Started = new();
    [HideInInspector] public UnityEvent Cooldown_4_Started = new();
    [HideInInspector] public UnityEvent Cooldown_5_Started = new();
    [HideInInspector] public UnityEvent<float> Working_Event = new();
    [HideInInspector] public UnityEvent Work_Cancelled = new();
    [HideInInspector] public UnityEvent Work_Finished = new();
    [HideInInspector] public UnityEvent Resume_Acting = new();
    [HideInInspector] public UnityEvent<float> Repair_Tick = new();

    private void Start()
    {
        moveComp = GetComponent<CanMove>();
        attackComp = GetComponent<CanAttack>();
        pickupComp = GetComponent<CanPickup>();
        playerCharacter = GetComponent<PlayerCharacter>();
        playerCharacter.Stun_Begin.AddListener(ChangeStateToBusy);
        playerCharacter.Stun_End.AddListener(ChangeStateToNone);
        manaComp = GetComponent<HasMana>();
        clickEffect = FindObjectOfType<ClickEffect>().gameObject;
        settingsManager = FindObjectOfType<SettingsManager>();
    }
    private void Update()
    {
        if (!isOwned)
            return;

        if (cooldown1 > 0)
            cooldown1 -= Time.deltaTime;
        if (cooldown2 > 0)
            cooldown2 -= Time.deltaTime;
        if (cooldown3 > 0)
            cooldown3 -= Time.deltaTime;
        if (cooldown4 > 0)
            cooldown4 -= Time.deltaTime;
        if (cooldown5 > 0)
            cooldown5 -= Time.deltaTime;

        if (inputEnabled.inputEnabled)
            InputHandle();
    }
    private void LateUpdate()
    {
        if (!isOwned || !inputEnabled.inputEnabled)
            return;
        if (Input.GetKeyDown(KeyCode.Mouse0) && state == PlayerState.Casting)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, spellCastingLayerMask))
            {
                CmdGroundLeftClicked(hit.point);
                if (hit.collider.TryGetComponent(out EnemyCharacter enemy) && FogOfWarTeam.GetTeam(0).GetFogValue(transform.position) <= 150)
                {
                    CmdEnemyLeftClicked(enemy);
                }
                if (hit.collider.TryGetComponent(out PlayerCharacter player))
                {
                    CmdAllyLeftClicked(player);
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.Mouse1) && state == PlayerState.Casting)
        {
            ChangeState(PlayerState.None);
            ChangeCastingState(CastingState.None);
            Resume_Acting.Invoke();
            currentSkill.StopExecute();
        }
    }
    public void SetCurrentSkill(Skill skill)
    {
        if (currentSkill)
            currentSkill.StopExecute();
        currentSkill = skill;
    }
    [Command]
    private void CmdGroundLeftClicked(Vector3 point)
    {
        RpcGroundLeftClicked(point);
    }
    [ClientRpc]
    private void RpcGroundLeftClicked(Vector3 point)
    {
        Ground_Left_Clicked.Invoke(point);
    }
    [Command]
    private void CmdEnemyLeftClicked(EnemyCharacter enemy)
    {
        RpcEnemyLeftClicked(enemy);
    }
    [ClientRpc]
    private void RpcEnemyLeftClicked(EnemyCharacter enemy)
    {
        Enemy_Left_Clicked.Invoke(enemy);
    }
    [Command]
    private void CmdAllyLeftClicked(PlayerCharacter player)
    {
        RpcAllyLeftClicked(player);
    }
    [ClientRpc]
    private void RpcAllyLeftClicked(PlayerCharacter player)
    {
        Ally_Left_Clicked.Invoke(player);
    }
    private void OnTriggerEnter(Collider other)
    {
        collidingColliders.Add(other);
    }
    private void OnTriggerExit(Collider other)
    {
        collidingColliders.Remove(other);
    }
    public void RemoveCollider(Collider other)
    {
        if (collidingColliders.Contains(other))
            collidingColliders.Remove(other);
    }
    private void InputHandle()
    {
        if (state != PlayerState.None && state != PlayerState.Working)
            return;

        if (IsPointerOverGameObject())              // Is mouse over UI?
            return;

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, movementLayerMask))
        {
            
            if (Input.GetKey(settingsManager.settings.move))                     // This could potentially ruin some interactions by overriding Move order
            {
                if (hit.collider.CompareTag("Ground"))                         
                {
                    moveComp.MoveTo(hit.point);
                    attackComp.CmdTargetLost();
                }
            }
            if (Input.GetKeyDown(settingsManager.settings.move))
            {
                if (hit.collider.CompareTag("Ground"))                         
                {
                    clickEffect.transform.position = hit.point;
                    clickEffect.GetComponent<ParticleSystem>().Play();
                }
                if (state == PlayerState.Working)
                {
                    ChangeState(PlayerState.None);
                }
            }
            if (Input.GetKeyDown(settingsManager.settings.interact))
            {
                if (hit.collider.TryGetComponent(out EnemyCharacter enemy))
                {
                    Enemy_Clicked.Invoke(enemy.GetComponent<NetworkIdentity>());
                }
                if (hit.collider.TryGetComponent(out Item item))
                {
                    StartCoroutine(pickupComp.GoToPickup(item));
                }
                if (hit.collider.TryGetComponent(out IInteractable interactable))
                {
                    if (hit.collider.TryGetComponent(out SchoolOfFish fishing))
                        StartCoroutine(GoToInteract(fishing.interactionCollider, interactable));
                    else
                        StartCoroutine(GoToInteract(hit.collider, interactable));
                    if (hit.collider.TryGetComponent(out Ship ship))
                    {
                        if (Vector3.Distance(transform.position, ship.transform.position) < 10)
                            ship.GetComponent<CanMove>().MoveTo(transform.position);
                        else
                            FindObjectOfType<SystemMessages>().AddMessage("The ship is too far away!");
                    }
                }
            }
            if (Input.GetKeyDown(settingsManager.settings.target))
            {
                if (hit.collider.TryGetComponent(out Character character))
                {
                    if (character.TryGetComponent(out HasHealth hp))
                    {
                        FindObjectOfType<CharacterHoverDetail>().Show(character, true);
                        character.ToggleHUDCircle(true);
                        targetedCharacter = character;
                    }
                }
                else
                {
                    FindObjectOfType<CharacterHoverDetail>().Hide(true);
                    if (targetedCharacter)
                    {
                        targetedCharacter.ToggleHUDCircle(false);
                        targetedCharacter.GetComponent<HighlightEffect>().isSelected = false;
                        targetedCharacter.GetComponent<HighlightEffect>().highlighted = false;
                    }
                }
            }
        }
        
        if (state != PlayerState.None)
            return;
        if (Input.GetKeyDown(settingsManager.settings.passiveSkill))
            AttemptExecuteSkill1();
        if (Input.GetKeyDown(settingsManager.settings.skill1))
            AttemptExecuteSkill2();
        if (Input.GetKeyDown(settingsManager.settings.skill2))
            AttemptExecuteSkill3();
        if (Input.GetKeyDown(settingsManager.settings.skill3))
            AttemptExecuteSkill4();
        if (Input.GetKeyDown(settingsManager.settings.skill4))
            AttemptExecuteSkill5();
    }
    private void SendMessageNotEnoughMana()
    {
        FindObjectOfType<SystemMessages>().AddMessage("Not enough Mana!");
    }
    private void SendMessageSkillNotReady()
    {
        FindObjectOfType<SystemMessages>().AddMessage("This skill is not ready yet!");
    }
    private void SendMessageSkillNotUnlocked()
    {
        FindObjectOfType<SystemMessages>().AddMessage("This skill is not unlocked yet!");
    }
    public void AttemptExecuteSkill1()
    {
        if (state != PlayerState.None)
            return;
        if (playerCharacter.skills[1].unlocked)
            if (cooldown1 <= 0)
                if (manaComp.GetMana() >= playerCharacter.skills[1].manaCost)
                    CmdExecuteSkill1();
                else
                    SendMessageNotEnoughMana();
            else
                SendMessageSkillNotReady();
        else
            SendMessageSkillNotUnlocked();
    }
    public void AttemptExecuteSkill2()
    {
        if (state != PlayerState.None)
            return;
        if (playerCharacter.skills[2].unlocked)
            if (cooldown2 <= 0)
                if (manaComp.GetMana() >= playerCharacter.skills[2].manaCost)
                    CmdExecuteSkill2();
                else
                    SendMessageNotEnoughMana();
            else
                SendMessageSkillNotReady();
        else
            SendMessageSkillNotUnlocked();
    }
    public void AttemptExecuteSkill3()
    {
        if (state != PlayerState.None)
            return;
        if (playerCharacter.skills[3].unlocked)
            if (cooldown3 <= 0)
                if (manaComp.GetMana() >= playerCharacter.skills[3].manaCost)
                    CmdExecuteSkill3();
                else
                    SendMessageNotEnoughMana();
            else
                SendMessageSkillNotReady();
        else
            SendMessageSkillNotUnlocked();
    }
    public void AttemptExecuteSkill4()
    {
        if (state != PlayerState.None)
            return;
        if (playerCharacter.skills[4].unlocked)
            if (cooldown4 <= 0)
                if (manaComp.GetMana() >= playerCharacter.skills[4].manaCost)
                    CmdExecuteSkill4();
                else
                    SendMessageNotEnoughMana();
            else
                SendMessageSkillNotReady();
        else
            SendMessageSkillNotUnlocked();
    }
    public void AttemptExecuteSkill5()
    {
        if (state != PlayerState.None)
            return;
        if (playerCharacter.skills[5].unlocked)
            if (cooldown5 <= 0)
                if (manaComp.GetMana() >= playerCharacter.skills[5].manaCost)
                    CmdExecuteSkill5();
                else
                    SendMessageNotEnoughMana();
            else
                SendMessageSkillNotReady();
        else
            SendMessageSkillNotUnlocked();
    }

    [Command]
    public void CmdExecuteSkill1()
    {
        RpcExecuteSkill1();
    }
    [ClientRpc]
    private void RpcExecuteSkill1()
    {
        playerCharacter.skills[1].Execute(playerCharacter);
    }
    [Command]
    public void CmdExecuteSkill2()
    {
        RpcExecuteSkill2();
    }
    [ClientRpc]
    private void RpcExecuteSkill2()
    {
        playerCharacter.skills[2].Execute(playerCharacter);
    }
    [Command]
    public void CmdExecuteSkill3()
    {
        RpcExecuteSkill3();
    }
    [ClientRpc]
    private void RpcExecuteSkill3()
    {
        playerCharacter.skills[3].Execute(playerCharacter);
    }
    [Command]
    public void CmdExecuteSkill4()
    {
        RpcExecuteSkill4();
    }
    [ClientRpc]
    private void RpcExecuteSkill4()
    {
        playerCharacter.skills[4].Execute(playerCharacter);
    }
    [Command]
    public void CmdExecuteSkill5()
    {
        RpcExecuteSkill5();
    }
    [ClientRpc]
    private void RpcExecuteSkill5()
    {
        playerCharacter.skills[5].Execute(playerCharacter);
    }
    private IEnumerator GoToInteract(Collider collider, IInteractable interactable)
    {
        moveComp.MoveTo(collider.transform.position);
        var originDest = moveComp.agent.destination;
        yield return new WaitForSeconds(0.2f);
        while (true)
        {
            if (originDest != moveComp.agent.destination)
            {
                yield break;
            }
            if (ContainsCollider(collider))
                break;
            yield return null;
        }
        moveComp.Stop();
        interactable.Interact(playerCharacter);
    }
    public bool ContainsCollider(Collider colliderToCompare)
    {
        return collidingColliders.Contains(colliderToCompare);
    }
    public void ChangeStateToNone()
    {
        ChangeState(PlayerState.None);
    }
    public void ChangeStateToBusy()
    {
        ChangeState(PlayerState.Busy);
    }
    public void ChangeState(PlayerState state)
    {
        this.state = state;
        if (state == PlayerState.Working)
        {
            attackComp.CmdTargetLost();
        }
    }
    public void ChangeCastingState(CastingState state)
    {
        castingState = state;
    }
    public void StartCooldown1()
    {
        cooldown1 = playerCharacter.skills[1].cooldown * GetComponent<CanAttack>().GetCooldownReductionModifier();
        Cooldown_1_Started.Invoke();
        CmdResumeActing();
    }
    public void StartCooldown2()
    {
        cooldown2 = playerCharacter.skills[2].cooldown * GetComponent<CanAttack>().GetCooldownReductionModifier();
        Cooldown_2_Started.Invoke();
        CmdResumeActing();
    }
    public void StartCooldown3()
    {
        cooldown3 = playerCharacter.skills[3].cooldown * GetComponent<CanAttack>().GetCooldownReductionModifier();
        Cooldown_3_Started.Invoke();
        CmdResumeActing();
    }
    public void StartCooldown4()
    {
        cooldown4 = playerCharacter.skills[4].cooldown * GetComponent<CanAttack>().GetCooldownReductionModifier();
        Cooldown_4_Started.Invoke();
        CmdResumeActing();
    }
    public void StartCooldown5()
    {
        cooldown5 = playerCharacter.skills[5].cooldown * GetComponent<CanAttack>().GetCooldownReductionModifier();
        Cooldown_5_Started.Invoke();
        CmdResumeActing();
    }
    [Command(requiresAuthority = false)]
    private void CmdResumeActing()
    {
        RpcResumeActing();
    }
    [ClientRpc]
    private void RpcResumeActing()
    {
        Resume_Acting.Invoke();
    }
    [Command]
    public void CmdStartWorking(float duration)
    {
        RpcStartWorking(duration);
    }
    [ClientRpc]
    public void RpcStartWorking(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(Working(duration));
    }
    private IEnumerator Working(float duration)
    {
        moveComp.Stop();
        ChangeState(PlayerState.Working);
        playerCharacter.animator.animator.SetBool("Interact", true);

        //Preparation for repairing
        float repairInterval = 0;
        float repairAmount = 0;
        foreach (var item in FindObjectOfType<CharacterScreen>().GetEquippedGear())
        {
            if (item.item.itemType == ItemType.HandicraftTool)
                repairAmount = item.item.value * 10;
        }

        while (duration > 0)
        {
            if (state != PlayerState.Working)
            {
                Working_Event.Invoke(0);
                Work_Cancelled.Invoke();
                Work_Finished.RemoveAllListeners();
                Work_Cancelled.RemoveAllListeners();
                Repair_Tick.RemoveAllListeners();
                playerCharacter.animator.animator.SetBool("Interact", false);
                yield break;
            }
            repairInterval += Time.deltaTime;
            if (repairInterval >= 0.97f)
            {
                Repair_Tick.Invoke(repairAmount);
                repairInterval = 0;
            }
            duration -= Time.deltaTime;
            Working_Event.Invoke(duration);
            yield return null;
        }
        if (moveComp.agent.enabled)
            ChangeState(PlayerState.None);
        else
            ChangeState(PlayerState.OutOfGame);
        Work_Finished.Invoke();
        Work_Finished.RemoveAllListeners();
        Work_Cancelled.RemoveAllListeners();
        Repair_Tick.RemoveAllListeners();
        playerCharacter.animator.animator.SetBool("Interact", false);
    }
    public bool IsPointerOverGameObject()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            string[] tagsToIgnore = { "FxTemporaire" };

            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = Input.mousePosition;

            // Create a list to hold RaycastResults
            List<RaycastResult> results = new List<RaycastResult>();

            // Perform the raycast and store the results in the list
            EventSystem.current.RaycastAll(eventData, results);

            // Check if any of the results match the specified tags to ignore
            return !results.Any(result => tagsToIgnore.Any(tag => result.gameObject.CompareTag(tag)));
        }

        return false;
    }
    public List<Collider> GetColliders()
    {
        return collidingColliders;
    }
}
