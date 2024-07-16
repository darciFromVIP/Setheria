using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using FMOD;
public enum MsgType
{
    Error, Notice, Positive
}
public class SystemMessages : MonoBehaviour
{
    public TextMeshProUGUI message;
    public Button cameraTeleportBTN;
    public UnityEvent<Vector3> cameraTeleportEvent = new();

    private Vector3 pos;
    public void AddMessage(string msg, MsgType msgType = MsgType.Error)
    {
        if (!cameraTeleportBTN.gameObject.activeSelf)
            StopAllCoroutines();
        else
            return;
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
        StartCoroutine(AddMessageCoro(msg, 5));
    }
    public void AddMessageWithTeleportBTN(string msg, Vector3 position, MsgType msgType = MsgType.Error)
    {
        StopAllCoroutines();
        switch (msgType)
        {
            case MsgType.Error:
                message.color = Color.red;
                break;
            case MsgType.Notice:
                message.color = Color.white;
                break;
            case MsgType.Positive:
                message.color = Color.green;
                break;
            default:
                break;
        }
        pos = position;
        cameraTeleportBTN.gameObject.SetActive(true);
        StartCoroutine(AddMessageCoro(msg, 10));
    }
    private IEnumerator AddMessageCoro(string msg, float time)
    {
        float timer = time;
        message.text = msg;
        message.transform.parent.gameObject.SetActive(true);
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }
        message.transform.parent.gameObject.SetActive(false);
        cameraTeleportBTN.gameObject.SetActive(false);
    }
    public void HideMessage()
    {
        StopAllCoroutines();
        message.transform.parent.gameObject.SetActive(false);
        cameraTeleportBTN.gameObject.SetActive(false);
    }
    public void TeleportCamera()
    {
        cameraTeleportEvent.Invoke(pos);
    }
}
