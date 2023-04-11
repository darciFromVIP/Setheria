using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;
using FoW;
public enum PlayerState
{
    None, Busy, Casting, Working
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

    public float cooldownD;
    public float cooldownQ;
    public float cooldownW;
    public float cooldownE;
    public float cooldownR;

    private List<UnityAction<NetworkIdentity>> Enemy_ClickedListeners = new();
    private UnityEvent<NetworkIdentity> Enemy_Clicked = new();
    [HideInInspector] public UnityEvent Enemy_Lost = new();
    [HideInInspector] public UnityEvent<Vector3> Ground_Left_Clicked = new();
    [HideInInspector] public UnityEvent<EnemyCharacter> Enemy_Left_Clicked = new();
    [HideInInspector] public UnityEvent<PlayerCharacter> Ally_Left_Clicked = new();
    [HideInInspector] public UnityEvent Cooldown_D_Started = new();
    [HideInInspector] public UnityEvent Cooldown_Q_Started = new();
    [HideInInspector] public UnityEvent Cooldown_W_Started = new();
    [HideInInspector] public UnityEvent Cooldown_E_Started = new();
    [HideInInspector] public UnityEvent Cooldown_R_Started = new();
    [HideInInspector] public UnityEvent<float> Working_Event = new();
    [HideInInspector] public UnityEvent Work_Cancelled = new();
    [HideInInspector] public UnityEvent Work_Finished = new();
    [HideInInspector] public UnityEvent Resume_Acting = new();

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
    }
    private void Update()
    {
        if (!isOwned)
            return;

        if (cooldownD > 0)
            cooldownD -= Time.deltaTime;
        if (cooldownQ > 0)
            cooldownQ -= Time.deltaTime;
        if (cooldownW > 0)
            cooldownW -= Time.deltaTime;
        if (cooldownE > 0)
            cooldownE -= Time.deltaTime;
        if (cooldownR > 0)
            cooldownR -= Time.deltaTime;

        InputHandle();
    }
    private void LateUpdate()
    {
        if (!isOwned)
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
    private void InputHandle()
    {
        if (state != PlayerState.None && state != PlayerState.Working)
            return;
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, movementLayerMask))
        {
            
            if (Input.GetKey(KeyCode.Mouse1))                                       // This could potentially ruin some interactions by overriding Move order
            {
                if (hit.collider.gameObject.layer == 0)                         // Default Layer
                {
                    moveComp.MoveTo(hit.point);
                    attackComp.TargetLost();
                }
            }
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                if (hit.collider.gameObject.layer == 0)
                {
                    clickEffect.transform.position = hit.point;
                    clickEffect.GetComponent<ParticleSystem>().Play();
                }
                if (hit.collider.TryGetComponent(out EnemyCharacter enemy))
                {
                    Enemy_Clicked.Invoke(enemy.GetComponent<NetworkIdentity>());
                }
                if (state == PlayerState.Working)
                {
                    ChangeState(PlayerState.None);
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
                        ship.GetComponent<CanMove>().MoveTo(transform.position);
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (hit.collider.TryGetComponent(out Character character))
                {
                    if (character.TryGetComponent(out HasHealth hp))
                    {
                        FindObjectOfType<CharacterHoverDetail>().Show(character, true);
                    }
                }
                else
                    FindObjectOfType<CharacterHoverDetail>().Hide(true);
            }
        }
        
        if (state != PlayerState.None)
            return;
        if (Input.GetKeyDown(KeyCode.D) && cooldownD <= 0 && manaComp.GetMana() >= playerCharacter.skills[1].manaCost)
            CmdExecuteSkill1();
        if (Input.GetKeyDown(KeyCode.Q) && cooldownQ <= 0 && manaComp.GetMana() >= playerCharacter.skills[2].manaCost)
            CmdExecuteSkill2();
        if (Input.GetKeyDown(KeyCode.W) && cooldownW <= 0 && manaComp.GetMana() >= playerCharacter.skills[3].manaCost)
            CmdExecuteSkill3();
        if (Input.GetKeyDown(KeyCode.E) && cooldownE <= 0 && manaComp.GetMana() >= playerCharacter.skills[4].manaCost)
            CmdExecuteSkill4();
        if (Input.GetKeyDown(KeyCode.R) && cooldownR <= 0 && manaComp.GetMana() >= playerCharacter.skills[5].manaCost)
            CmdExecuteSkill5();
    }
    [Command]
    private void CmdExecuteSkill1()
    {
        RpcExecuteSkill1();
    }
    [ClientRpc]
    private void RpcExecuteSkill1()
    {
        playerCharacter.skills[1].Execute(playerCharacter);
    }
    [Command]
    private void CmdExecuteSkill2()
    {
        RpcExecuteSkill2();
    }
    [ClientRpc]
    private void RpcExecuteSkill2()
    {
        playerCharacter.skills[2].Execute(playerCharacter);
    }
    [Command]
    private void CmdExecuteSkill3()
    {
        RpcExecuteSkill3();
    }
    [ClientRpc]
    private void RpcExecuteSkill3()
    {
        playerCharacter.skills[3].Execute(playerCharacter);
    }
    [Command]
    private void CmdExecuteSkill4()
    {
        RpcExecuteSkill4();
    }
    [ClientRpc]
    private void RpcExecuteSkill4()
    {
        playerCharacter.skills[4].Execute(playerCharacter);
    }
    [Command]
    private void CmdExecuteSkill5()
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
        while (true)
        {
            if (originDest != moveComp.agent.destination)
            {
                yield break;
            }
            if (ContainsCollider(collider) || moveComp.HasReachedDestination())
                break;
            yield return null;
        }
        moveComp.Stop();
        interactable.Interact(playerCharacter);
    }
    private bool ContainsCollider(Collider colliderToCompare)
    {
        return collidingColliders.Contains(colliderToCompare);
    }
    public void EnemyClickedAddListener(UnityAction<NetworkIdentity> method)
    {
        Enemy_Clicked.AddListener(method);
        Enemy_ClickedListeners.Add(method);
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
        Debug.Log(state);
    }
    public void ChangeCastingState(CastingState state)
    {
        castingState = state;
    }
    public void StartCooldownD()
    {
        cooldownD = playerCharacter.skills[1].cooldown;
        Cooldown_D_Started.Invoke();
        Resume_Acting.Invoke();
    }
    public void StartCooldownQ()
    {
        cooldownQ = playerCharacter.skills[2].cooldown;
        Cooldown_Q_Started.Invoke();
        Resume_Acting.Invoke();
    }
    public void StartCooldownW()
    {
        cooldownW = playerCharacter.skills[3].cooldown;
        Cooldown_W_Started.Invoke();
        Resume_Acting.Invoke();
    }
    public void StartCooldownE()
    {
        cooldownE = playerCharacter.skills[4].cooldown;
        Cooldown_E_Started.Invoke();
        Resume_Acting.Invoke();
    }
    public void StartCooldownR()
    {
        cooldownR = playerCharacter.skills[5].cooldown;
        Cooldown_R_Started.Invoke();
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
        StartCoroutine(Working(duration));
    }
    private IEnumerator Working(float duration)
    {
        ChangeState(PlayerState.Working);
        while (duration > 0)
        {
            if (state != PlayerState.Working)
            {
                Working_Event.Invoke(0);
                Work_Cancelled.Invoke();
                Work_Finished.RemoveAllListeners();
                Work_Cancelled.RemoveAllListeners();
                yield break;
            }
            duration -= Time.deltaTime;
            Working_Event.Invoke(duration);
            yield return null;
        }
        ChangeState(PlayerState.None);
        Work_Finished.Invoke();
        Work_Finished.RemoveAllListeners();
    }
}
