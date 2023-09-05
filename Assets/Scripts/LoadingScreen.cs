using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class LoadingScreen : MonoBehaviour
{
    public GameObject window;
    public TextMeshProUGUI loadingText, percentageProgress;
    public Slider loadingSlider;
    public Image loadingScreenImage;
    public List<Sprite> images;
    public void LoadAsyncOperation(string text, AsyncOperation operation)
    {
        window.SetActive(true);
        loadingText.text = text;
        loadingScreenImage.sprite = images[Random.Range(0, images.Count)];
        StartCoroutine(Loading(operation));
    }
    private IEnumerator Loading(AsyncOperation operation)
    {
        while (!operation.isDone)
        {
            percentageProgress.text = (operation.progress / 0.9f * 100 ).ToString("F0") + "%";
            loadingSlider.value = operation.progress / 0.9f;
            yield return null;
        }
    }
}
