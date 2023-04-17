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
        castingEntity = self;
        castingEntity.GetComponent<Shapeshifter>().CmdShapeshift(false);
        castingEntity.GetComponent<PlayerController>().StartCooldownD();
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
    public override void UpdateDescription()
    {
        description = GetTextIconByStat(PlayerStat.CooldownReduction) + (cooldown * castingEntity.GetComponent<CanAttack>().GetCooldownReductionModifier()).ToString("F1")
            + " " + GetTextIconByStat(PlayerStat.MaxMana) + manaCost + "\nPassive: Wolferius gains 1 " + GetTextIconByStat(PlayerStat.Armor) + " per enemy targeting him." +
            "\n\nActive: Shapeshifts into a wolf.";
        base.UpdateDescription();
    }
}
