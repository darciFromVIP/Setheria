using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType
{
    None, Beast
}
public class IgnoredByEnemies : MonoBehaviour
{
    public List<EnemyType> ignoredEnemies;
}
