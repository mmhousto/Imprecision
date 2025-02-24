
using UnityEngine;
using Unity.SaveData.PS5;
using Unity.SaveData.PS5.Mount;
using Unity.SaveData.PS5.Core;

public class SaveIconWithScreenShot : MonoBehaviour
{
    public Renderer myRenderer;

    private bool doScreenshot = false;
    private Mounting.MountPoint currentMP;

    public void DoScreenShot(Mounting.MountPoint mp)
    {
        currentMP = mp;

        if (mp != null)
        {
            doScreenshot = true;
        }
    }

    void LateUpdate()
    {
        if (doScreenshot)
        {
#if UNITY_PS5
            doScreenshot = false;

            Camera camera = GetComponent<Camera>();

            int iconWidth = Mounting.SaveIconRequest.DATA_ICON_WIDTH_FULL;
            int iconHeight = Mounting.SaveIconRequest.DATA_ICON_HEIGHT_FULL;

            RenderTexture rt = new RenderTexture(iconWidth, iconHeight, 24);

            camera.targetTexture = rt;

            Texture2D screenShot = new Texture2D(iconWidth, iconHeight, TextureFormat.RGB24, false);

            camera.Render();

            RenderTexture.active = rt;

            screenShot.ReadPixels(new Rect(0, 0, iconWidth, iconHeight), 0, 0, false);
            screenShot.Apply();

            myRenderer.material.mainTexture = screenShot;

            camera.targetTexture = null;
            RenderTexture.active = null;

            Destroy(rt);

            byte[] bytes = screenShot.EncodeToPNG();

            OnScreenLog.Add("Screenshot Icon size : W = " + iconWidth + " H = " + iconHeight);

            SaveIcon(currentMP, bytes);
#endif
        }      
    }

    public void SaveIcon(Mounting.MountPoint mp, byte[] pngBytes)
    {
#if UNITY_PS5
        try
        {
            Mounting.SaveIconRequest request = new Mounting.SaveIconRequest();

            if (mp == null) return;

            request.UserId = User.GetActiveUserId;
            request.MountPointName = mp.PathName;
            request.RawPNG = pngBytes;

            EmptyResponse response = new EmptyResponse();

            int requestId = Mounting.SaveIcon(request, response);

            OnScreenLog.Add("SaveIcon Async : Request Id = " + requestId);
        }
        catch (SaveDataException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
#endif
    }
}