using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Skills/Lycandruid/Defensive Stance")]
public class SDefensiveStance : Skill
{
    private HasHealth selfCharacter;
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
        TogglePassive(true);
    }
    public void TogglePassive(bool value)
    {
        if (value)
            selfCharacter.ChangeGearArmor(0.5f * selfCharacter.GetComponent<Character>().level);
        else
            selfCharacter.ChangeGearArmor(-0.5f * selfCharacter.GetComponent<Character>().level); 
    }
    public override void UpdateDescription()
    {
        description = GetTextIconByStat(PlayerStat.CooldownReduction) + (cooldown * castingEntity.GetComponent<CanAttack>().GetCooldownReductionModifier()).ToString("F1")
            + " " + GetTextIconByStat(PlayerStat.MaxMana) + manaCost + "\nPassive: Wolferius gains <color=orange>" + (0.5f * selfCharacter.GetComponent<Character>().level)
            + GetTextIconByStat(PlayerStat.Armor) + "</color> (0.5 * " + GetTextIconByStat(PlayerStat.Level) + ")" + " permanently. Not active in wolf form!" 
            +  "\n\nActive: Shapeshifts into a wolf.";
        base.UpdateDescription();
    }
}
