using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PointType
{
    Attribute, Talent
}

public class AvailablePointsBTN : MonoBehaviour
{
    public Animator animator;
    public PointType pointType;
    public void Initialize(PlayerCharacter player)
    {
        if (pointType == PointType.Attribute)
            player.Attributes_Changed.AddListener(PointsChanged);
        else if (pointType == PointType.Talent)
            player.talentTrees.Talent_Points_Changed.AddListener(PointsChanged);
    }
    private void PointsChanged(int points)
    {
        if (points > 0)
            animator.SetBool("AvailablePoints", true);
        else
            animator.SetBool("AvailablePoints", false);
    }
}
