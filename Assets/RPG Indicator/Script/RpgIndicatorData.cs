using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "Rpg Indicator Data", order = 1)]
public class RpgIndicatorData : ScriptableObject
{
    [ColorUsage(true, true)]
    public Color MainAllyColor;
    [ColorUsage(true, true)]
    public Color MainNeutralColor;
    [ColorUsage(true, true)]
    public Color MainEnemyColor;
    [HideInInspector]
    [ColorUsage(true, true)]
    public Color MainCustomColor;
    [Space (10)]
    [ColorUsage(true, true)]
    public Color RangeAllyColor;
    [ColorUsage(true, true)]
    public Color RangeNeutralColor;
    [ColorUsage(true, true)]
    public Color RangeEnemyColor;
    [HideInInspector]
    [ColorUsage(true, true)]
    public Color RangeCustomColor;

    [Space(10)]
    public Material RangeIndicator;
    public Material ConeIndicator;
    public Material AreaIndicator;
    public Material RadiusIndicator;
    public Material LineIndicator;

    [Space(10)]
    public LayerMask Layer;
}
