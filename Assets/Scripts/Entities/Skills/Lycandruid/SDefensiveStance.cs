using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Skills/Lycandruid/Defensive Stance")]
public class SDefensiveStance : Skill
{
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
        castingEntity = self;
        (self as PlayerCharacter).Level_Up.AddListener(LevelUp);
        TogglePassive(true);
    }
    private void LevelUp(int level)
    {
        if (castingEntity.GetComponent<Shapeshifter>().shapeshiftedModel.gameObject.activeSelf)
            castingEntity.GetComponent<HasHealth>().ChangeGearArmor(0.5f);
    }
    public void TogglePassive(bool value)
    {
        var hp = castingEntity.GetComponent<HasHealth>();
        if (value)
            hp.ChangeGearArmor(0.5f * hp.GetComponent<Character>().level);
        else
            hp.ChangeGearArmor(-0.5f * hp.GetComponent<Character>().level); 
    }
    public override void UpdateDescription()
    {
        description = GetTextIconByStat(PlayerStat.CooldownReduction) + (cooldown * castingEntity.GetComponent<CanAttack>().GetCooldownReductionModifier()).ToString("F1")
            + " " + GetTextIconByStat(PlayerStat.MaxMana) + manaCost + "\nPassive: Wolferius gains <color=orange>" + (0.5f * castingEntity.level)
            + GetTextIconByStat(PlayerStat.Armor) + "</color> (0.5 * " + GetTextIconByStat(PlayerStat.Level) + ")" + " permanently. Not active in wolf form!" 
            +  "\n\nActive: Shapeshifts into a wolf.";
        base.UpdateDescription();
    }
}
