using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG_Indicator;

public class CodeExample : MonoBehaviour
{
    public RpgIndicator PlayerIndicator;

    // Note
    // ShowRangeIndicator will activate the range indicator before casting
    // RpgIndicator.IndicatorAlignement.Ally will determine the color to use when showing the indicator
    // Style refer to the array od RPGIndicatorData. It will affect the colors, materials and layer to use
    private void Start()
    {
        Radius();
    }
    public void Cone()
    {
        // Cone ability with a 40 degree angle and range of 10
        PlayerIndicator.ShowCone(40, 10, true, RpgIndicator.IndicatorColor.Ally, 0);
    }
    public void Line()
    {
        // Line ability with a length og 6 and range of 10
        PlayerIndicator.ShowLine(6, 10, true, RpgIndicator.IndicatorColor.Ally, 0);
    }
    public void Area()
    {
        // Area ability with a radius of 5 and range of 10 and with 2 custom colors
        PlayerIndicator.CustomColor("#80989700", "#80989700");
        PlayerIndicator.ShowArea(5, 10, true, RpgIndicator.IndicatorColor.Custom, 0);
    }
    public void Radius()
    {
        // Radius ability with a radius of 10
        PlayerIndicator.ShowRadius(10, false, RpgIndicator.IndicatorColor.Enemy, 0);
    }    
    public void Cast()
    {
        // Start casting with a casting time of 5 seconds
        PlayerIndicator.Casting(5);
    }
    public void Interrupt()
    {
        // Interrupt casting
        PlayerIndicator.InterruptCasting();
    }
}
