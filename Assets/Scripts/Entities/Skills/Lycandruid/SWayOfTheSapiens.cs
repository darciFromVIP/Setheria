using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Lycandruid/Way of the Sapiens")]
public class SWayOfTheSapiens : Skill
{
    public override void Execute(Character self)
    {
        castingEntity = self;
        self.GetComponent<HasHealth>().CmdChangeMaxHealthMultiplier(0.25f);
    }
    public override void StopExecute()
    {
        base.StopExecute();
        castingEntity.GetComponent<HasHealth>().CmdChangeMaxHealthMultiplier(-0.25f);
        TogglePassive(false);
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
        description = "Wolferius embraced the human, unlocking following features:" +
            "\n- Wolferius gains <color=orange>" + (0.5f * castingEntity.level)
            + GetTextIconByStat(PlayerStat.Armor) + "</color> (0.5 * " + GetTextIconByStat(PlayerStat.Level) + ")" + " permanently." +
            "\n- Wolferius gains +25% <sprite=5>" +
            "\n- Wolferius cannot shapeshift back to wolf anymore. ";
        base.UpdateDescription();
    }
}
