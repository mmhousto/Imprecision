using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.SaveData.PS5.Info;
using Unity.SaveData.PS5.Mount;
using Unity.SaveData.PS5.Core;

#if UNITY_PS5

using Unity.SaveData.PS5;

public class SonySaveDataMount : IScreen
{
    MenuLayout m_MenuMount;

    Icon currentIcon = null;
    bool updateIcon = false;

    public Material iconMaterial;
    public SaveIconWithScreenShot screenShotHelper;


    public void SetIconTexture(Icon icon)
    {
        OnScreenLog.Add("New Icon received");
        currentIcon = icon;
        updateIcon = true;
    }

    public SonySaveDataMount()
    {
        Initialize();
    }

    public MenuLayout GetMenu()
    {
        return m_MenuMount;
    }

    public void OnEnter()
    {
    }

    public void OnExit()
    {
    }

    public void Process(MenuStack stack)
    {
        if (updateIcon == true)
        {
            updateIcon = false;

            if (currentIcon != null)
            {
                // This will create the texture if it is not already cached in the currentIcon.           
                UnityEngine.Texture2D iconTexture = new UnityEngine.Texture2D(currentIcon.Width, currentIcon.Height);

                iconTexture.LoadImage(currentIcon.RawBytes);

                iconMaterial.mainTexture = iconTexture;

                OnScreenLog.Add("Updating icon material : W = " + iconTexture.width + " H = " + iconTexture.height);
            }
        }

        MenuUserProfiles(stack);
    }

    public void Initialize()
    {
        m_MenuMount = new MenuLayout(this, 530, 20);
    }

    public void MenuUserProfiles(MenuStack menuStack)
    {
        List<Mounting.MountPoint> mountPoints = Mounting.ActiveMountPoints;

        m_MenuMount.Update();

        bool isEnabled = SaveDataDirNames.HasCurrentDirName();

        string dirName = SaveDataDirNames.GetCurrentDirName();

        string dirNameToolTip = "";
        
        if ( isEnabled == true )
        {
            dirNameToolTip = " Use this on directory name \"" + dirName + "\".";
        }

        if (m_MenuMount.AddItem("Create Random Directory Name", "This will generate a new directory name and mount it for read/write access."))
        {
            SaveDataDirNames.GenerateNewDirName(OnScreenLog.FrameCount);

            if (SaveDataDirNames.HasCurrentDirName() == true)
            {
                Mount(true, true);
            }
        }

        if (m_MenuMount.AddItem("Mount Read/Write", "Mount a save data for read/write access." + dirNameToolTip, isEnabled))
        {
            Mount(true, true);
        }

        if (m_MenuMount.AddItem("Mount R/W (synchronous)", "Mount a save data for read/write access using synchronous request." + dirNameToolTip, isEnabled))
        {
            Mount(false, true);
        }

        if (m_MenuMount.AddItem("Mount Read Only", "Mount a read only save data." + dirNameToolTip, isEnabled))
        {
            Mount(true, false);
        }

        if (m_MenuMount.AddItem("Mount PS4 Save", "Mount a PS4 save data as read only." + dirNameToolTip, isEnabled))
        {
            MountPS4(true); // requires sdk 3.00 and greater
        }        

        Mounting.MountPoint mp = SonySaveDataMain.GetMountPoint();

        isEnabled = (mp != null);

        string moutPointToolTip = "";

        if (mp != null)
        {
            moutPointToolTip = " Use this on mount point \"" + mp.PathName.Data + "\".";
        }

        if (m_MenuMount.AddItem("Get Mount Info", "Get mount info." + moutPointToolTip, isEnabled))
        {
            GetMountInfo(mp);
        }

        if (m_MenuMount.AddItem("Get Mount Params", "Get mount params." + moutPointToolTip, isEnabled))
        {
            GetMountParams(mp);
        }

        if (m_MenuMount.AddItem("Set Mount Params", "Set mount params." + moutPointToolTip, isEnabled))
        {
            SetMountParams(mp);
        }

        if (m_MenuMount.AddItem("Save Icon (From File)", "Save the save data icon. This will read PNG data from a file and use that as the save data icon." + moutPointToolTip, isEnabled))
        {
            SaveIconFromFile(mp);
        }

        if (m_MenuMount.AddItem("Save Icon (Screenshot)", "Save the save data icon. Take a screenshot and use it for the save data icon. This will not include the OnGUI text in the sample." + moutPointToolTip, isEnabled))
        {
            screenShotHelper.DoScreenShot(mp);
        }
               
        if (m_MenuMount.AddItem("Load Icon", "Load the save data icon." + moutPointToolTip, isEnabled))
        {
            LoadIcon(mp);
        }

        //if(m_MenuMount.AddItem("Test", "" + moutPointToolTip, isEnabled))
        //{
        //    byte[] fileBytes = File.ReadAllBytes("/app0/Media/StreamingAssets/SaveIcon.png");
        //    OnScreenLog.Add("File bytes = " + fileBytes.Length);

        //    string outpath = mp.PathName.Data + "/SaveIcon.dat";
        //    OnScreenLog.Add("Output Path = " + outpath);
        //    File.WriteAllBytes(outpath, fileBytes);
        //}

        if (m_MenuMount.AddBackIndex("Back"))
        {
            menuStack.PopMenu();
        }
    }

    public void Mount(bool async, bool readWrite)
    {
        try
        {
            Mounting.MountRequest request = new Mounting.MountRequest();

            DirName dirName = new DirName();
            dirName.Data = SaveDataDirNames.GetCurrentDirName();

            OnScreenLog.Add("Mounting Direcotry : " + dirName.Data);

            request.UserId = User.GetActiveUserId;
            request.Async = async;
            request.DirName = dirName;

            if (readWrite == true)
            {
                request.MountMode = Mounting.MountModeFlags.Create2 | Mounting.MountModeFlags.ReadWrite;
            }
            else
            {
                request.MountMode = Mounting.MountModeFlags.ReadOnly;
            }

            request.Blocks = SonySaveDataMain.TestBlockSize;

            Mounting.MountResponse response = new Mounting.MountResponse();

            int requestId = Mounting.Mount(request, response);

            if (async == true)
            {
                OnScreenLog.Add("Mount Async : Request Id = " + requestId);
            }
            else
            {
                if (response.ReturnCodeValue < 0)
                {
                    OnScreenLog.AddError("Mount Sync : " + response.ConvertReturnCodeToString(request.FunctionType));
                }
                else
                {
                    OnScreenLog.Add("Mount Sync : " + response.ConvertReturnCodeToString(request.FunctionType));
                }
                MountReponseOutput(response);
                OnScreenLog.AddNewLine();
            }
        }
        catch (SaveDataException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void MountPS4(bool async)
    {
        try
        {
            Mounting.MountPS4Request request = new Mounting.MountPS4Request();

            DirName dirName = new DirName();
            dirName.Data = "Autosave";      // for this example to work you need to run the ps4 savedata sample and trigger an "autosave"

            TitleId titleId = new TitleId();
            titleId.Data = "NPXX51363";     // example title id ... Unity's PS4 sample title id ... Also needs to be set in the "titleIdForTransferringPs4" section of the param.json

            Fingerprint fingerprint = new Fingerprint();
            fingerprint.Data = "47117e63c68456258dfa10d29d1e050a4d6439572601babf18efbf3f8cc1ec76";  // from ps4 package generator tool. project settings->package->passcode fingerprint

            OnScreenLog.Add("Mounting Direcotry : " + dirName.Data);

            request.UserId = User.GetActiveUserId;
            request.Async = async;
            request.DirName = dirName;
            request.TitleId = titleId;
            request.Fingerprint = fingerprint;

 
            Mounting.MountResponse response = new Mounting.MountResponse();

            int requestId = Mounting.MountPS4(request, response);

            if (async == true)
            {
                OnScreenLog.Add("Mount Async : Request Id = " + requestId);
            }
            else
            {
                if (response.ReturnCodeValue < 0)
                {
                    OnScreenLog.AddError("Mount Sync : " + response.ConvertReturnCodeToString(request.FunctionType));
                }
                else
                {
                    OnScreenLog.Add("Mount Sync : " + response.ConvertReturnCodeToString(request.FunctionType));
                }
                MountReponseOutput(response);
                OnScreenLog.AddNewLine();
            }
        }
        catch (SaveDataException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }


    public void GetMountInfo(Mounting.MountPoint mp)
    {
        try
        {
            Mounting.GetMountInfoRequest request = new Mounting.GetMountInfoRequest();

            if (mp == null) return;

            request.UserId = User.GetActiveUserId;
            request.MountPointName = mp.PathName;

            Mounting.MountInfoResponse response = new Mounting.MountInfoResponse();

            int requestId = Mounting.GetMountInfo(request, response);

            OnScreenLog.Add("GetMountInfo Async : Request Id = " + requestId);
        }
        catch (SaveDataException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void GetMountParams(Mounting.MountPoint mp)
    {
        try
        {
            Mounting.GetMountParamsRequest request = new Mounting.GetMountParamsRequest();

            if (mp == null) return;

            request.UserId = User.GetActiveUserId;
            request.MountPointName = mp.PathName;

            Mounting.MountParamsResponse response = new Mounting.MountParamsResponse();

            int requestId = Mounting.GetMountParams(request, response);

            OnScreenLog.Add("GetMountParams Async : Request Id = " + requestId);
        }
        catch (SaveDataException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void SetMountParams(Mounting.MountPoint mp)
    {
        try
        {
            Mounting.SetMountParamsRequest request = new Mounting.SetMountParamsRequest();

            if (mp == null) return;

            request.UserId = User.GetActiveUserId;
            request.MountPointName = mp.PathName;

            SaveDataParams sdParams = new SaveDataParams();

            sdParams.Title = "My Save Data " + OnScreenLog.FrameCount;
            sdParams.SubTitle = "My Save Data Subtitle " + OnScreenLog.FrameCount;
            sdParams.Detail = "This is the long descrition of the save data.";
            sdParams.UserParam = (UInt32)OnScreenLog.FrameCount;

            request.Params = sdParams;

            EmptyResponse response = new EmptyResponse();

            int requestId = Mounting.SetMountParams(request, response);

            OnScreenLog.Add("GetMountParams Async : Request Id = " + requestId);
        }
        catch (SaveDataException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void SaveIconFromFile(Mounting.MountPoint mp)
    {
        try
        {
            Mounting.SaveIconRequest request = new Mounting.SaveIconRequest();

            if (mp == null) return;

            request.UserId = User.GetActiveUserId;
            request.MountPointName = mp.PathName;

            request.IconPath = "/app0/Media/StreamingAssets/SaveIcon.png";

            EmptyResponse response = new EmptyResponse();

            int requestId = Mounting.SaveIcon(request, response);

            OnScreenLog.Add("SaveIcon Async : Request Id = " + requestId);
        }
        catch (SaveDataException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void LoadIcon(Mounting.MountPoint mp)
    {
        try
        {
            Mounting.LoadIconRequest request = new Mounting.LoadIconRequest();

            if (mp == null) return;

            request.UserId = User.GetActiveUserId;
            request.MountPointName = mp.PathName;

            Mounting.LoadIconResponse response = new Mounting.LoadIconResponse();

            int requestId = Mounting.LoadIcon(request, response);

            OnScreenLog.Add("LoadIcon Async : Request Id = " + requestId);
        }
        catch (SaveDataException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void OnAsyncEvent(SaveDataCallbackEvent callbackEvent)
    {
        switch (callbackEvent.ApiCalled)
        {
            case FunctionTypes.Mount:
                {
                    MountReponseOutput(callbackEvent.Response as Mounting.MountResponse);
                }
                break;
            case FunctionTypes.GetMountInfo:
                {
                    MountInfoReponseOutput(callbackEvent.Response as Mounting.MountInfoResponse);
                }
                break;
            case FunctionTypes.GetMountParams:
                {
                    MountParamsReponseOutput(callbackEvent.Response as Mounting.MountParamsResponse);
                }
                break;
            case FunctionTypes.LoadIcon:
                {
                    LoadIconReponseOutput(callbackEvent.Response as Mounting.LoadIconResponse);
                }
                break;
        }
    }

    public void MountReponseOutput(Mounting.MountResponse response)
    {
        if (response != null)
        {
            OnScreenLog.Add("MountPoint : " + response.MountPoint.PathName.Data);
            OnScreenLog.Add("RequiredBlocks : " + response.RequiredBlocks);
            OnScreenLog.Add("WasCreated : " + response.WasCreated);
        }
    }

    public void MountInfoReponseOutput(Mounting.MountInfoResponse response)
    {
        if (response != null)
        {
            SaveDataInfo sdInfo = response.Info; 
            OnScreenLog.Add("Blocks : " + sdInfo.Blocks);
            OnScreenLog.Add("FreeBlocks : " + sdInfo.FreeBlocks);
        }
    }

    public void MountParamsReponseOutput(Mounting.MountParamsResponse response)
    {
        if (response != null)
        {
            SaveDataParams sdParams = response.Params;

            OnScreenLog.Add("Title : " + sdParams.Title);
            OnScreenLog.Add("SubTitle : " + sdParams.SubTitle);
            OnScreenLog.Add("Detail : " + sdParams.Detail);
            OnScreenLog.Add("UserParam : " + sdParams.UserParam);
            OnScreenLog.Add("Time : " + sdParams.Time.ToString("d/M/yyyy HH:mm:ss"));
        }
    }

    public void LoadIconReponseOutput(Mounting.LoadIconResponse response)
    {
        if (response != null)
        {
            if ( response.Icon != null)
            {
                OnScreenLog.Add("Icon loaded");
                SetIconTexture(response.Icon);
            }
            else
            {
                OnScreenLog.Add("No Icon saved");
            }       
        }
    }
}
#endif
