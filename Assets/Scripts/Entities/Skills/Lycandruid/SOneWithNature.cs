using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Lycandruid/One With Nature")]
public class SOneWithNature : Skill
{
    public override void Execute(Character self)
    {
        castingEntity = self;
        FindObjectOfType<AudioManager>().PlayOneShot(sound, castingEntity.transform.position);
        (castingEntity.GetComponent<Shapeshifter>().defaultSkills[1] as SDefensiveStance).TogglePassive(true);
        castingEntity.GetComponent<Shapeshifter>().CmdShapeshift(true);
        castingEntity.GetComponent<PlayerController>().StartCooldownD();
    }
    public override void UpdateDescription()
    {
        description = GetTextIconByStat(PlayerStat.CooldownReduction) + (cooldown * castingEntity.GetComponent<CanAttack>().GetCooldownReductionModifier()).ToString("F1")
            + " " + GetTextIconByStat(PlayerStat.MaxMana) + manaCost + "\nPassive: Enemy Beasts will ignore Wolferius unless he attacks first." +
            "\n\nActive: Shapeshifts back into the human form.";
        base.UpdateDescription();
    }
}
