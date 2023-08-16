using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class SystemMessages : MonoBehaviour
{
    public TextMeshProUGUI message;

    public void AddMessage(string msg)
    {
        StartCoroutine(AddMessageCoro(msg));
        FindObjectOfType<AudioManager>().UIError();
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
