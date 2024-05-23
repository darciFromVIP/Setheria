using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class BuffUI : MonoBehaviour
{
    private Image image;
    private TooltipTrigger tooltipTrigger;
    public TextMeshProUGUI durationText, stacksText;
    private Slider slider;
    public Buff currentBuffInstance;
    public BuffDatabase buffDatabase;

    private void Awake()
    {
        image = GetComponent<Image>();
        tooltipTrigger = GetComponent<TooltipTrigger>();
        slider = GetComponentInChildren<Slider>();
    }
    private void Update()
    {
        if (currentBuffInstance != null)
        {
            TimeSpan time = TimeSpan.FromSeconds(currentBuffInstance.durationTimer);

            //here backslash is must to tell that colon is
            //not the part of format, it just a character that we want in output
            string str;
            if (currentBuffInstance.durationTimer >= 3600)
                str = time.ToString(@"hh\h");
            else if (currentBuffInstance.durationTimer >= 60)
                str = time.ToString(@"mm\m");
            else
                str = currentBuffInstance.durationTimer.ToString("F0");

            durationText.text = str;
            slider.value = currentBuffInstance.durationTimer;
            if (currentBuffInstance.stacks > 1)
            {
                stacksText.gameObject.SetActive(true);
                stacksText.text = currentBuffInstance.stacks.ToString();
            }
        }
    }
    private void BuffExpired()
    {
        Destroy(gameObject);
    }
    public void Initialize(string buffName, Buff buffInstance)
    {
        var buff = buffDatabase.GetBuffByName(buffName);
        if (buff.sprite != null)
            image.sprite = buff.sprite;
        else
            Destroy(gameObject);
        tooltipTrigger.SetText(buff.name, buff.description, buff.sprite);
        currentBuffInstance = buffInstance;
        slider.maxValue = buff.duration;
        buffInstance.Buff_Expired.AddListener(BuffExpired);
        if (buff.duration == 0)
            durationText.gameObject.SetActive(false);
    }
}
