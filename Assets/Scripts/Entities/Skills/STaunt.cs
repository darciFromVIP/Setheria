using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Skills/Tanks/Taunt")]
public class STaunt : Skill
{
    private HasHealth selfCharacter;
    public override void Execute(Character self)
    {
        base.Execute(self);
        castingEntity = self;
        castingEntity.GetComponent<Character>().CastSkill1();
        castingEntity.GetComponent<PlayerController>().ChangeState(PlayerState.Busy);
        FindObjectOfType<AudioManager>().PlayOneShot(sound, castingEntity.transform.position);
        if (castingEntity.isOwned)
            self.GetComponentInChildren<AnimatorEventReceiver>().Skill1_Casted.AddListener(Cast);
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
            + " " + GetTextIconByStat(PlayerStat.MaxMana) + manaCost +"\nActive: Taunts nearby enemies, making them target you.";
        base.UpdateDescription();
    }
}
