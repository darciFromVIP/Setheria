using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Skills/Lycandruid/Defensive Stance")]
public class SDefensiveStance : Skill
{
    private HasHealth selfCharacter;
    private List<HasHealth> aggroedEnemies = new();
    private bool enabled = true;
    private void OnValidate()
    {
        aggroedEnemies.Clear();
        enabled = true;
    }
    public override void Execute(Character self)
    {
        castingEntity = self;
        FindObjectOfType<AudioManager>().PlayOneShot(sound, castingEntity.transform.position);
        TogglePassive(false);
        if (castingEntity.isServer)
            castingEntity.GetComponent<Shapeshifter>().RpcShapeshift(false);
        castingEntity.GetComponent<PlayerController>().StartCooldown1();
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
            if (!enabled)
                return;
            selfCharacter.ChangeGearArmor(1);
        }
    }
    private void RemoveEnemy(HasHealth enemy)
    {
        if (aggroedEnemies.Contains(enemy))
        {
            aggroedEnemies.Remove(enemy);
            if (!enabled)
                return;
            selfCharacter.ChangeGearArmor(-1);
        }
    }
    public void TogglePassive(bool value)
    {
        enabled = value;
        if (value)
        {
            foreach (var item in aggroedEnemies)
            {
                selfCharacter.ChangeGearArmor(1);
            }
        }
        else
        {
            foreach (var item in aggroedEnemies)
            {
                selfCharacter.ChangeGearArmor(-1);
            }
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
