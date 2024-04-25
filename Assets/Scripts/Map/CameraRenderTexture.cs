using System.IO;
using UnityEngine;

public class CameraRenderTexture : MonoBehaviour
{
    public int fileCounter;
    public KeyCode screenshotKey;
   
    private void LateUpdate()
    {
        if (Input.GetKeyDown(screenshotKey))
        {
            Capture();
        }
    }
    [ContextMenu("Capture")]
    public void Capture()
    {
        RenderTexture activeRenderTexture = RenderTexture.active;
        RenderTexture.active = GetComponent<Camera>().targetTexture;

        GetComponent<Camera>().Render();

        Texture2D image = new Texture2D(GetComponent<Camera>().targetTexture.width, GetComponent<Camera>().targetTexture.height);
        image.ReadPixels(new Rect(0, 0, GetComponent<Camera>().targetTexture.width, GetComponent<Camera>().targetTexture.height), 0, 0);
        image.Apply();
        RenderTexture.active = activeRenderTexture;

        byte[] bytes = image.EncodeToJPG();
        DestroyImmediate(image);

        string d = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "/Maps";
        Directory.CreateDirectory(d);
        File.WriteAllBytes(d + "/" + fileCounter + ".jpg", bytes);
        fileCounter++;
    }
}
