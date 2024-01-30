using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Skills/Forest Protector/Flower Power Heal")]
public class SFlowerPowerHeal : Skill
{
    public float range;
    public float heal;
    public Projectile projectile;
    public override void Execute(Character self)
    {
        base.Execute(self);
        if (self.isServer)
            castingEntity.GetComponent<Character>().CastSkill1();

        castingEntity.GetComponentInChildren<AnimatorEventReceiver>().Skill1_Casted.RemoveAllListeners();
        castingEntity.GetComponentInChildren<AnimatorEventReceiver>().Skill1_Casted.AddListener(Cast);
        castingEntity.skillIndicator.ShowRadius(range, false, RPG_Indicator.RpgIndicator.IndicatorColor.Ally, 0);
        castingEntity.skillIndicator.Casting(1.23f);
        
    }
    protected override void Cast()
    {
        base.Cast();
        if (castingEntity.isServer)
            castingEntity.GetComponent<Alkestia>().CastFlowerPowerHeal();
        FindObjectOfType<AudioManager>().PlayOneShot(sound, castingEntity.transform.position);
        castingEntity.GetComponent<CharacterVFXReference>().skill1.SetActive(true);
        castingEntity.GetComponent<CanAttack>().CmdResumeActing();
    }
}
