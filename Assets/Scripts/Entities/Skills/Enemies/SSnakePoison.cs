using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Enemies/Snake Poison")]
public class SSnakePoison : Skill
{
    public float range;
    public BuffScriptable poisonBuff;
    public BuffScriptable slowBuff;
    public Projectile projectile;
    public override void Execute(Character self)
    {
        base.Execute(self);
        if (Vector3.Distance(castingEntity.transform.position, self.GetComponent<CanAttack>().enemyTarget.transform.position) > range)
        {
            castingEntity.GetComponent<CanMove>().Moved_Within_Range.AddListener(StartCasting);
            castingEntity.GetComponent<CanMove>().MoveWithinRangeEnemy(self.GetComponent<CanAttack>().enemyTarget.transform, range);
        }
        else
        {
            StartCasting();
        }
    }
    public override Skill GetInstance()
    {
        var instance = (SSnakePoison)base.GetInstance();
        instance.poisonBuff = poisonBuff;
        instance.slowBuff = slowBuff;
        instance.range = range;
        instance.projectile = projectile;
        return instance;
    }
    public override void ExecuteOnStart(Character self)
    {
    }

    public override void StopExecute()
    {
        base.StopExecute();
    }
    protected override void StartCasting()
    {
        base.StartCasting();
        if (castingEntity.GetComponent<CanAttack>().enemyTarget == null)
        {
            castingEntity.GetComponent<CanMove>().Moved_Within_Range.RemoveListener(StartCasting);
            return;
        }
        if (castingEntity.isServer)
            castingEntity.GetComponent<Character>().CastSkill1();
        castingEntity.GetComponentInChildren<AnimatorEventReceiver>().Skill1_Casted.AddListener(Cast);
        castingEntity.GetComponent<Character>().RotateToPoint(castingEntity.GetComponent<CanAttack>().enemyTarget.transform.position);
    }
    protected override void Cast()
    {
        base.Cast(); 
        if (castingEntity.isServer)
            castingEntity.GetComponent<EnemySkills>().CastSnakePoison();
        castingEntity.GetComponent<CanMove>().Moved_Within_Range.RemoveListener(StartCasting);
        Character enemy = castingEntity.GetComponent<Character>();
        if (!sound.IsNull)
            FindObjectOfType<AudioManager>().PlayOneShot(sound, castingEntity.GetComponent<CanAttack>().enemyTarget.transform.position);
        enemy.StartCooldown1();
        enemy.GetComponentInChildren<AnimatorEventReceiver>().Skill1_Casted.RemoveAllListeners();
    }
}
