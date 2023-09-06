using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Forest Protector/Flower Power")]
public class SFlowerPower : Skill
{
    public float timedLife;
    public float baseDamage;
    public PlayerStat damageScalingStat;
    public float damageScalingValue;
    public float baseHeal;
    public PlayerStat healScalingStat;
    public float healScalingValue;
    private float finalDamage;
    private float finalHeal;
    public float range;
    public Character flowerPrefab;
    public SFlowerPowerHeal healingSkillRef;
    public EventReference GrowSound;

    private Vector3 actualPoint;
    public override void Execute(Character self)
    {
        base.Execute(self);
        self.GetComponent<PlayerController>().Ground_Left_Clicked.AddListener(StartCast);
        self.GetComponent<PlayerController>().ChangeCastingState(CastingState.None);
    }
    public override void ExecuteOnStart(Character self)
    {

    }
    public override void StopExecute()
    {
        castingEntity.GetComponent<PlayerController>().Ground_Left_Clicked.RemoveListener(StartCast);
        castingEntity.GetComponentInChildren<AnimatorEventReceiver>().Skill5_Casted.RemoveListener(Cast);
    }
    private void StartCast(Vector3 point)
    {
        FindObjectOfType<AudioManager>().PlayOneShot(sound, castingEntity.transform.position);
        actualPoint = Vector3.MoveTowards(castingEntity.transform.position, point, range);
        castingEntity.GetComponent<PlayerController>().ChangeState(PlayerState.Busy);
        castingEntity.GetComponent<PlayerController>().Ground_Left_Clicked.RemoveListener(StartCast);
        castingEntity.GetComponent<Character>().CastSkill5();
        castingEntity.GetComponentInChildren<AnimatorEventReceiver>().Skill5_Casted.AddListener(Cast);
        castingEntity.GetComponent<Character>().RotateToPoint(point);
    }
    private void Cast()
    {
        castingEntity.GetComponent<CanHavePets>().CmdSpawnPet(flowerPrefab.name, actualPoint, timedLife, finalDamage);
        FindObjectOfType<AudioManager>().PlayOneShot(GrowSound, castingEntity.GetComponent<CanHavePets>().spawnedPets[0].transform.position);
        PlayerController player = castingEntity.GetComponent<PlayerController>();
        player.GetComponent<HasMana>().CmdSpendMana(manaCost);
        player.StartCooldownR();
        player.GetComponentInChildren<AnimatorEventReceiver>().Skill5_Casted.RemoveListener(Cast);
        player.ChangeState(PlayerState.None);
        player.ChangeCastingState(CastingState.None);
    }

    public override void UpdateDescription()
    {
        finalDamage = baseDamage + GetScalingStatValue(damageScalingStat) * damageScalingValue;
        finalHeal = baseHeal + GetScalingStatValue(healScalingStat) * healScalingValue;
        healingSkillRef.heal = finalHeal;
        description = GetTextIconByStat(PlayerStat.CooldownReduction) + (cooldown * castingEntity.GetComponent<CanAttack>().GetCooldownReductionModifier()).ToString("F1")
            + " " + GetTextIconByStat(PlayerStat.MaxMana) + manaCost + "\nMakes Alkestia grow on the target point. She attacks nearby enemies, dealing <color=orange>" + finalDamage
            + "</color> damage " + "(" + baseDamage + " + " + (int)(damageScalingValue * 100) + "% " + GetTextIconByStat(damageScalingStat) + ")"
            + " and occasionaly heals nearby allies for <color=orange>" + finalHeal + "</color> (" + baseHeal + " + " + (int)(healScalingValue * 100)
            + "% " + GetTextIconByStat(healScalingStat) + ")." + " Lasts " + timedLife + " seconds.";
        base.UpdateDescription();
    }
}
