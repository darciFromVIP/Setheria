using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Databases/Hero Model Database")]
public class HeroModelDatabase : ScriptableObject
{
    [Tooltip("Order must be: Lycandruid, ForestProtector")]
    public List<GameObject> models;

    public GameObject GetModelByHeroType(Hero hero)
    {
        switch (hero)
        {
            case Hero.Lycandruid:
                return models[0];
            case Hero.ForestProtector:
                return models[1];
            default:
                return null;
        }
    }
}
