using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class FloatingTextPrefab : MonoBehaviour
{
    private string message;
    private FloatingTextType textType;
    public void SetUp(string msg, FloatingTextType type)
    {
        message = msg;
        textType = type;
        Initialize();
    }
    private void Initialize()
    {
        var textMesh = GetComponentInChildren<TextMeshProUGUI>();
        textMesh.text = message;
        switch (textType)
        {
            case FloatingTextType.Healing:
                textMesh.color = Color.green;
                break;
            case FloatingTextType.Damage:
                textMesh.color = Color.red;
                break;
            case FloatingTextType.Resources:
                textMesh.color = new Color(1, 0.5537655f, 0.3160377f, 1);
                break;
            case FloatingTextType.Knowledge:
                textMesh.color = new Color(0.4f, 0.7410114f, 1, 1);
                break;
            case FloatingTextType.Experience:
                textMesh.color = new Color(0.7086645f, 0.4858491f, 1, 1);
                break;
            case FloatingTextType.Hunger:
                textMesh.color = new Color(1, 0.5f, 0, 1);
                break;
            default:
                break;
        }
    }
}
