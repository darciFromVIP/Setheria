using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
[CreateAssetMenu(menuName = "Post Processing Data")]
public class PostProcessingDataScriptable : ScriptableObject
{
    public float timeStamp;
    [Header("Light Values")]
    public Color color;
    public Vector3 rotation;
    public float intensity;
    [Header("Volume Values")]
    [Header("Color Adjustments")]
    public float postExposure;
    public float contrast;
    public Color colorFilter;
    public float hueShift;
    public float saturation;
    [Header("Lift Gamma Gain")]
    public Vector4 lift;
    public Vector4 gamma;
    public Vector4 gain;
    [Header("Tonemapping")]
    public TonemappingMode mode;
}
