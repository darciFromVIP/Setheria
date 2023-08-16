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
        castingEntity = self;
        castingEntity.GetComponent<Character>().CastSkill1();
        if (self.isServer)
        {
            castingEntity.GetComponentInChildren<AnimatorEventReceiver>().Skill1_Casted.RemoveAllListeners();
            castingEntity.GetComponentInChildren<AnimatorEventReceiver>().Skill1_Casted.AddListener(Cast);
        }
    }
    private void Cast()
    {
        FindObjectOfType<AudioManager>().PlayOneShot(sound, castingEntity.transform.position);
        castingEntity.GetComponent<CharacterVFXReference>().skill1.SetActive(true);
        var proj = Instantiate(projectile, castingEntity.transform.position, Quaternion.identity);
        proj.InitializeProjectile(new ProjectileData()
        {
            projectileTravel = ProjectileTravelType.Instant,
            projectileImpact = ProjectileImpactType.AoE,
            impactEffect = ProjectileImpactEffect.Healing,
            affectsEntities = true,
            targetsMask = LayerMask.GetMask("Player"),
            aoeRadius = range,
            affectsOwner = false,
            effectValue = heal,
        });
        castingEntity.GetComponent<CanAttack>().ResumeActing();
    }
}
