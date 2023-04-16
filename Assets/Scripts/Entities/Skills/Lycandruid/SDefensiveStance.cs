using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Skills/Lycandruid/Defensive Stance")]
public class SDefensiveStance : Skill
{
    private HasHealth selfCharacter;
    private List<HasHealth> aggroedEnemies = new();
    public override void Execute(Character self)
    {
        base.Execute(self);
        castingEntity = self;
        castingEntity.GetComponent<Character>().CastSkill1();
        castingEntity.GetComponent<PlayerController>().ChangeState(PlayerState.Busy);
        if (castingEntity.isOwned)
            self.GetComponentInChildren<AnimatorEventReceiver>().Skill1_Casted.AddListener(Cast);
    }
    public override void ExecuteOnStart(Character self)
    {
        base.ExecuteOnStart(self);
        selfCharacter = self.GetComponent<HasHealth>();
        selfCharacter.Target_Received.AddListener(AddEnemy);
        selfCharacter.Received_Target_Lost.AddListener(RemoveEnemy);
    }
    private void AddEnemy(HasHealth enemy)
    {
        if (!aggroedEnemies.Contains(enemy))
        {
            aggroedEnemies.Add(enemy);
            selfCharacter.ChangeArmor(1);
        }
    }
    private void RemoveEnemy(HasHealth enemy)
    {
        if (aggroedEnemies.Contains(enemy))
        {
            aggroedEnemies.Remove(enemy);
            selfCharacter.ChangeArmor(-1);
        }
    }
    private void Cast()
    {
        var targets = new List<HasHealth>();
        Collider[] colliders = new Collider[20];
        Physics.OverlapSphereNonAlloc(selfCharacter.transform.position, 10, colliders, selfCharacter.GetComponent<Character>().enemyLayers);
        foreach (var item in colliders)
        {
            if (item != null)
                if (item.TryGetComponent(out HasHealth entity))
                    targets.Add(entity);
        }
        HasHealth[] copy = new HasHealth[20];
        targets.CopyTo(copy);
        foreach (var item in copy)
        {
            if (item)
            {
                if (item.TryGetComponent(out CanAttack character))
                {
                    character.TargetAcquired(selfCharacter.GetComponent<Mirror.NetworkIdentity>());
                }
            }
        }
        PlayerController player = castingEntity.GetComponent<PlayerController>();
        player.StartCooldownD();
        player.GetComponentInChildren<AnimatorEventReceiver>().Skill1_Casted.RemoveAllListeners();
        player.ChangeState(PlayerState.None);
    }
    public override void UpdateDescription()
    {
        description = GetTextIconByStat(PlayerStat.CooldownReduction) + (cooldown * castingEntity.GetComponent<CanAttack>().GetCooldownReductionModifier()).ToString("F1")
            + " " + GetTextIconByStat(PlayerStat.MaxMana) + manaCost + "\nPassive: Wolferius gains 1 " + GetTextIconByStat(PlayerStat.Armor) + " per enemy targeting him." +
            "\n\nActive: Wolferius taunts nearby enemies, making them target him.";
        base.UpdateDescription();
    }
}
