using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "Skills/Lycandruid/Pounce")]
public class SPounce : Skill
{
    public float baseDamage;
    public PlayerStat damageScalingStat;
    public float damageScalingValue;
    [HideInInspector] public float finalDamage;
    public float baseDuration;
    public float range;
    public Projectile projectile;
    public BuffScriptable stunBuff;
    [HideInInspector] public EnemyCharacter enemy;
    public EventReference PounceImpactSound;

    [HideInInspector] public Vector3 actualPoint;
    public override void Execute(Character self)
    {
        base.Execute(self);
        self.GetComponent<PlayerController>().Enemy_Left_Clicked.AddListener(MoveWithinRange);
        self.GetComponent<PlayerController>().ChangeCastingState(CastingState.EnemyOnly);
        if (castingEntity.isOwned)
            castingEntity.skillIndicator.ShowRange(range, RPG_Indicator.RpgIndicator.IndicatorColor.Enemy, 0);
    }
    public override Skill GetInstance()
    {
        var instance = (SPounce)base.GetInstance();
        instance.baseDamage = baseDamage;
        instance.damageScalingStat = damageScalingStat;
        instance.damageScalingValue = damageScalingValue;
        instance.baseDuration = baseDuration;
        instance.stunBuff = stunBuff;
        instance.PounceImpactSound = PounceImpactSound;
        instance.range = range;
        instance.projectile = projectile;
        return instance;
    }
    public override void StopExecute()
    {
        base.StopExecute();
    }
    private void MoveWithinRange(EnemyCharacter enemy)
    {
        NavMeshPath path = new();
        NavMesh.CalculatePath(castingEntity.transform.position, enemy.transform.position, castingEntity.GetComponent<CanMove>().agent.areaMask, path);
        if (path.status != NavMeshPathStatus.PathComplete)
        {
            FindObjectOfType<SystemMessages>().AddMessage("The path to the target is not reachable.");
            return;
        }
        this.enemy = enemy;
        if (Vector3.Distance(castingEntity.transform.position, enemy.transform.position) > range)
        {
            castingEntity.GetComponent<CanMove>().Moved_Within_Range.AddListener(StartCasting);
            castingEntity.GetComponent<CanMove>().MoveWithinRange(enemy.transform, range);
        }
        else
        {
            castingEntity.GetComponent<PlayerController>().Enemy_Left_Clicked.RemoveAllListeners();
            StartCasting();
        }
    }
    protected override void StartCasting()
    {
        base.StartCasting();
        if (castingEntity.isOwned)
            castingEntity.GetComponent<Character>().CastSkill3();
        castingEntity.GetComponent<PlayerController>().CmdChangeState(PlayerState.Busy);
        castingEntity.GetComponent<CanMove>().Moved_Within_Range.RemoveListener(StartCasting);
        castingEntity.GetComponentInChildren<AnimatorEventReceiver>().Skill3_Casted.AddListener(Cast);
        castingEntity.GetComponent<Character>().RotateToPoint(enemy.transform.position);
        if (castingEntity.isOwned)
            castingEntity.skillIndicator.Casting(0);
    }
    protected override void Cast()
    {
        castingEntity.StartCoroutine(Jump());
    }
    private IEnumerator Jump()
    {
        FindObjectOfType<AudioManager>().PlayOneShot(sound, castingEntity.transform.position);
        var moveComp = castingEntity.GetComponent<CanMove>();
        moveComp.agent.enabled = false;
        float minDistance = moveComp.agent.stoppingDistance + castingEntity.GetComponent<CanAttack>().GetAttackRange();
        while (Vector3.Distance(castingEntity.transform.position, enemy.transform.position) > minDistance)
        {
            castingEntity.transform.position = Vector3.MoveTowards(castingEntity.transform.position, enemy.transform.position, Time.deltaTime * 25);
            yield return null;
        }
        moveComp.agent.enabled = true;
        stunBuff.duration = baseDuration;
        castingEntity.GetComponent<PlayerController>().CmdChangeState(PlayerState.None);
        CastEffect();
    }
    private void CastEffect()
    {
        base.Cast();
        if (castingEntity.isServer)
            castingEntity.GetComponent<Lycandruid>().CastPounce();
        PlayerController player = castingEntity.GetComponent<PlayerController>();
        FindObjectOfType<AudioManager>().PlayOneShot(PounceImpactSound, castingEntity.transform.position);
        if (castingEntity.isServer)
            player.GetComponent<HasMana>().RpcSpendMana(manaCost);
        player.StartCooldown3();
        player.GetComponentInChildren<AnimatorEventReceiver>().Skill3_Casted.RemoveListener(Cast);
    }
    public override void UpdateDescription()
    {
        finalDamage = baseDamage + GetScalingStatValue(damageScalingStat) * damageScalingValue;
        description = GetTextIconByStat(PlayerStat.CooldownReduction) + (cooldown * castingEntity.GetComponent<CanAttack>().GetCooldownReductionModifier()).ToString("F1")
            + " " + GetTextIconByStat(PlayerStat.MaxMana) + manaCost + "\nPounces to the enemy target, dealing <color=orange>" + finalDamage
            + "</color> damage " + "(" + baseDamage + " + " + (int)(damageScalingValue * 100) + "% " + GetTextIconByStat(damageScalingStat) + ")"
            + " and stunning it for " + baseDuration + " seconds.";
        base.UpdateDescription();
    }
}
