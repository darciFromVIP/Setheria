using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Aura")]
public class SAura : Skill
{
    public List<BuffScriptable> buffsApplied;
    public LayerMask layerMask;
    public float radius;
    public override void ExecuteOnStart(Transform self)
    {
        base.ExecuteOnStart(self);
        var collider = self.Find("Aura").GetComponent<Collider>();
        if (collider is SphereCollider)
            (collider as SphereCollider).radius = radius;
        collider.GetComponent<AuraEventReceiver>().On_Trigger_Stay.AddListener(OnTriggerStay);
        collider.GetComponent<AuraEventReceiver>().On_Trigger_Exit.AddListener(OnTriggerExit);
    }
    private void OnTriggerStay(Collider other)
    {
        if (layerMask == (layerMask | (1 << other.gameObject.layer)))
        {
            if (other.TryGetComponent(out Character character))
            {
                foreach (var item in buffsApplied)
                {
                    character.RpcAddBuff(item.name);
                }
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (layerMask == (layerMask | (1 << other.gameObject.layer)))
        {
            if (other.TryGetComponent(out Character character))
            {
                foreach (var item in buffsApplied)
                {
                    character.RpcRemoveBuff(item.name);
                }
            }
        }
    }
}
