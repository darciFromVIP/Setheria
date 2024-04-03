using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
            durationText.text = currentBuffInstance.durationTimer.ToString("F0");
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
