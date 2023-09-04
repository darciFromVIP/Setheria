using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public enum MsgType
{
    Error, Notice, Positive
}
public class SystemMessages : MonoBehaviour
{
    public TextMeshProUGUI message;

    public void AddMessage(string msg, MsgType msgType = MsgType.Error)
    {
        StopAllCoroutines();
        switch (msgType)
        {
            case MsgType.Error:
                message.color = Color.red;
                FindObjectOfType<AudioManager>().UIError();
                break;
            case MsgType.Notice:
                message.color = Color.white; 
                FindObjectOfType<AudioManager>().QuestAccepted();
                break;
            case MsgType.Positive:
                message.color = Color.green;
                break;
            default:
                break;
        }
        StartCoroutine(AddMessageCoro(msg));
    }
    private IEnumerator AddMessageCoro(string msg)
    {
        float timer = 5;
        message.text = msg;
        message.transform.parent.gameObject.SetActive(true);
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }
        message.transform.parent.gameObject.SetActive(false);
    }
}
