


#if UNITY_PS5

using Unity.SaveData.PS5;
using Unity.SaveData.PS5.Core;
using Unity.SaveData.PS5.Info;
using Unity.SaveData.PS5.Mount;

public class SonySaveDataFileOps : IScreen
{
    MenuLayout m_MenuFileOps;

    public SonySaveDataFileOps()
    {
        Initialize();
    }

    public MenuLayout GetMenu()
    {
        return m_MenuFileOps;
    }

    public void OnEnter()
    {
    }

    public void OnExit()
    {
    }

    public void Process(MenuStack stack)
    {
        MenuFileOps(stack);
    }

    public void Initialize()
    {
        m_MenuFileOps = new MenuLayout(this, 530, 20);
    }

    public void MenuFileOps(MenuStack menuStack)
    {
        m_MenuFileOps.Update();

        Mounting.MountPoint mp = SonySaveDataMain.GetMountPoint();

        bool isEnabled = (mp != null);

        string moutPointToolTip = "";

        if (mp != null)
        {
            moutPointToolTip = " Use this on mount point \"" + mp.PathName.Data + "\".";
        }

        if (m_MenuFileOps.AddItem("Write Files", "Write a couple of text file to the current mount point." + moutPointToolTip, isEnabled))
        {
            WriteFiles(mp);
        }

        if (m_MenuFileOps.AddItem("Enumerate Files", "List the files inside the save data." + moutPointToolTip, isEnabled))
        {
            EnumerateFiles(mp);
        }

        if (m_MenuFileOps.AddItem("Read Files", "Read the files in the save data." + moutPointToolTip, isEnabled))
        {
            ReadFiles(mp);
        }

        if (m_MenuFileOps.AddBackIndex("Back"))
        {
            menuStack.PopMenu();
        }
    }

  
    public void WriteFiles(Mounting.MountPoint mp)
    {
        try
        {
            ExampleWriteFilesRequest request = new ExampleWriteFilesRequest();

            request.UserId = User.GetActiveUserId;
            request.MountPointName = mp.PathName;

            for (int i = 0; i < 20; i++)
            {
                request.largeData[i] = (byte)(OnScreenLog.FrameCount+i); 
            }

            ExampleWriteFilesResponse response = new ExampleWriteFilesResponse();

            int requestId = FileOps.CustomFileOp(request, response);

            OnScreenLog.Add("WriteFiles Async : Request Id = " + requestId);

            string dataStr = "";
            for (int i = 0; i < 20; i++)
            {
                if (i > 0) dataStr += ", ";
                dataStr += request.largeData[i];
            }
            OnScreenLog.Add("Write Data = " + dataStr);
        }
        catch (SaveDataException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void EnumerateFiles(Mounting.MountPoint mp)
    {
        try
        {
            ExampleEnumerateFilesRequest request = new ExampleEnumerateFilesRequest();

            request.UserId = User.GetActiveUserId;
            request.MountPointName = mp.PathName;

            ExampleEnumerateFilesResponse response = new ExampleEnumerateFilesResponse();

            int requestId = FileOps.CustomFileOp(request, response);

            OnScreenLog.Add("WriteFiles Async : Request Id = " + requestId);
        }
        catch (SaveDataException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void ReadFiles(Mounting.MountPoint mp)
    {
        try
        {
            ExampleReadFilesRequest request = new ExampleReadFilesRequest();

            request.UserId = User.GetActiveUserId;
            request.MountPointName = mp.PathName;

            ExampleReadFilesResponse response = new ExampleReadFilesResponse();

            int requestId = FileOps.CustomFileOp(request, response);

            OnScreenLog.Add("ReadFiles Async : Request Id = " + requestId);
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
            case FunctionTypes.FileOps:
                {
                    if (callbackEvent.Request is ExampleWriteFilesRequest)
                    {
                        ExampleWriteReponseOutput(callbackEvent.Response as ExampleWriteFilesResponse);
                    }
                    else if (callbackEvent.Request is ExampleEnumerateFilesRequest)
                    {
                        ExampleEnumerateReponseOutput(callbackEvent.Response as ExampleEnumerateFilesResponse);
                    }
                    else if (callbackEvent.Request is ExampleReadFilesRequest)
                    {
                        ExampleReadReponseOutput(callbackEvent.Response as ExampleReadFilesResponse);
                    }
                }
                break;
        }
    }

    public void ExampleWriteReponseOutput(ExampleWriteFilesResponse response)
    {
        if (response != null)
        {
            OnScreenLog.Add("Last Write Time : " + response.lastWriteTime);
            OnScreenLog.Add("Total File Size Written : " + response.totalFileSizeWritten);
        }
    }

    public void ExampleEnumerateReponseOutput(ExampleEnumerateFilesResponse response)
    {
        if (response != null)
        {
            if (response.files == null || response.files.Length == 0)
            {
                OnScreenLog.Add("No files were found");
            }
            else
            {
                for (int i = 0; i < response.files.Length; i++)
                {
                    OnScreenLog.Add("   " + response.files[i]);
                }
            }
        }
    }

    public void ExampleReadReponseOutput(ExampleReadFilesResponse response)
    {
        if (response != null)
        {
            OnScreenLog.Add("My Test Data : " + response.myTestData);
            OnScreenLog.Add("My Other Test Data : " + response.myOtherTestData);

            string dataStr = "";
            for (int i = 0; i < 20; i++)
            {
                if (i > 0) dataStr += ", ";
                dataStr += response.largeData[i];
            }
            OnScreenLog.Add("Read Data = " + dataStr);
        }
    }

}
#endif
