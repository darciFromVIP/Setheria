using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Items/Blowpipe")]
public class ISBlowpipe : Skill
{
    public float range;
    public Projectile projectile;
    public BuffScriptable sleepBuff;
    [HideInInspector] public EnemyCharacter enemy;
    public override void ExecuteOnStart(ItemScriptable self)
    {
        base.ExecuteOnStart(self);

    }
    public override void Execute(Character self)
    {
        base.Execute(self);
        self.GetComponent<PlayerController>().Enemy_Left_Clicked.AddListener(MoveWithinRange);
        self.GetComponent<PlayerController>().ChangeCastingState(CastingState.EnemyOnly);
        if (castingEntity.isOwned)
            castingEntity.skillIndicator.ShowRange(range, RPG_Indicator.RpgIndicator.IndicatorColor.Enemy, 0);
    }
    public override void StopExecute()
    {
        base.StopExecute();
        castingEntity.GetComponent<PlayerController>().Enemy_Left_Clicked.RemoveListener(MoveWithinRange);
        castingEntity.GetComponent<CanMove>().Moved_Within_Range.RemoveListener(Cast);
        if (castingEntity.isOwned)
            castingEntity.skillIndicator.InterruptCasting();
    }
    private void MoveWithinRange(EnemyCharacter enemy)
    {
        this.enemy = enemy;
        if (Vector3.Distance(castingEntity.transform.position, enemy.transform.position) > range)
        {
            castingEntity.GetComponent<CanMove>().Moved_Within_Range.AddListener(Cast);
            castingEntity.GetComponent<CanMove>().MoveWithinRange(enemy.transform, range);
        }
        else
        {
            castingEntity.GetComponent<PlayerController>().Enemy_Left_Clicked.RemoveAllListeners();
            Cast();
        }
    }
    protected override void Cast()
    {
        base.Cast();
        castingEntity.GetComponent<CanMove>().Moved_Within_Range.RemoveListener(Cast);
        castingEntity.GetComponent<Character>().RotateToPoint(enemy.transform.position);
        castingEntity.GetComponent<ActiveItemSkills>().CmdCastBlowpipe(enemy.GetComponent<NetworkIdentity>());
        PlayerController player = castingEntity.GetComponent<PlayerController>();
        player.ChangeCastingState(CastingState.None);
        player.ChangeState(PlayerState.None);
        Skill_Casted.Invoke();
        if (castingEntity.isOwned)
            castingEntity.skillIndicator.Casting(0);
    }
}
