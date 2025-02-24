
using UnityEngine;
using Unity.SaveData.PS5.Mount;

#if UNITY_PS5
using Unity.SaveData.PS5;
#endif

public class GetScreenShot : MonoBehaviour
{
    public Renderer myRenderer;

    private bool doScreenshot = false;
    public byte[] screenShotBytes = null;

    public bool IsWaiting
    {
        get { return doScreenshot == true; }
    }

    public void DoScreenShot()
    {
        screenShotBytes = null;
        doScreenshot = true;
    }

    void LateUpdate()
    {
#if UNITY_PS5
        if (doScreenshot)
        {
            var currentRT = RenderTexture.active;

            Camera camera = GetComponent<Camera>();

            int iconWidth = Mounting.SaveIconRequest.DATA_ICON_WIDTH_FULL;
            int iconHeight = Mounting.SaveIconRequest.DATA_ICON_HEIGHT_FULL;

            RenderTexture rt = new RenderTexture(iconWidth, iconHeight, 24);

            camera.targetTexture = rt;
            RenderTexture.active = rt;

            camera.Render();

            Texture2D screenShot = new Texture2D(iconWidth, iconHeight, TextureFormat.RGB24, false);
            screenShot.ReadPixels(new Rect(0, 0, iconWidth, iconHeight), 0, 0, false);
            screenShot.Apply();

            myRenderer.material.mainTexture = screenShot;

            byte[] rawData = screenShot.GetRawTextureData();
            OnScreenLog.Add("Screenshot Raw size : = " + rawData.Length);

            screenShotBytes = screenShot.EncodeToPNG();

            camera.targetTexture = null;
            RenderTexture.active = currentRT;

            Destroy(rt);

            doScreenshot = false;

            OnScreenLog.Add("Screenshot Icon size : W = " + iconWidth + " H = " + iconHeight);
            OnScreenLog.Add("Screenshot Byte Size : " + screenShotBytes.Length);
        }
#endif
    }
}
