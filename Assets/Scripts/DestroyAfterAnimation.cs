using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class DestroyAfterAnimation : MonoBehaviour
{
    public void Destroy()
    {
        Destroy(gameObject);
    }
    public void DestroyParent()
    {
        Destroy(transform.parent.gameObject);
    }
    public void DestroyLootableParentOnServer()
    {
        GetComponentInParent<LootableObject>().CmdDestroyOnServer();
    }
}
