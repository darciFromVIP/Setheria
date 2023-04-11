using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuffUI : MonoBehaviour
{
    private Image image;
    private TooltipTrigger tooltipTrigger;
    private TextMeshProUGUI durationText;
    private Slider slider;
    private Buff currentBuffInstance;

    private void Awake()
    {
        image = GetComponent<Image>();
        tooltipTrigger = GetComponent<TooltipTrigger>();
        slider = GetComponentInChildren<Slider>();
        durationText = GetComponentInChildren<TextMeshProUGUI>();
    }
    private void Update()
    {
        if (currentBuffInstance != null)
        {
            durationText.text = currentBuffInstance.durationTimer.ToString("F0");
            slider.value = currentBuffInstance.durationTimer;
        }
    }
    private void BuffExpired()
    {
        Destroy(gameObject);
    }
    public void Initialize(BuffScriptable buff, Buff buffInstance)
    {
        image.sprite = buff.sprite;
        tooltipTrigger.SetText(buff.name, buff.description, buff.sprite);
        currentBuffInstance = buffInstance;
        slider.maxValue = buff.duration;
        buffInstance.Buff_Expired.AddListener(BuffExpired);
    }
}
