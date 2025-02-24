using System;
using System.Collections;

#if UNITY_PS5

using Unity.SaveData.PS5;
using Unity.SaveData.PS5.Core;
using Unity.SaveData.PS5.Delete;
using Unity.SaveData.PS5.Dialog;
using Unity.SaveData.PS5.Info;
using Unity.SaveData.PS5.Mount;
using Unity.SaveData.PS5.Search;

public class SonySaveDataDialogs : IScreen
{
    MenuLayout m_MenuDialogs;

    SonySaveDataDialogTests m_DialogTests;
    public GetScreenShot screenShotHelper;

    public SonySaveDataDialogs()
    {
        Initialize();

        m_DialogTests = new SonySaveDataDialogTests();
    }

    public MenuLayout GetMenu()
    {
        return m_MenuDialogs;
    }

    public void OnEnter()
    {
    }

    public void OnExit()
    {
    }

    public void Process(MenuStack stack)
    {
        MenuUserProfiles(stack);
    }

    public void Initialize()
    {
        m_MenuDialogs = new MenuLayout(this, 530, 20);
    }

    public void MenuUserProfiles(MenuStack menuStack)
    {
        m_MenuDialogs.Update();


        if (m_MenuDialogs.AddItem("Create Test Save Data", "Create some test save data items used for testing the dialog save process."))
        {
            SonySaveDataMain.StartSaveDataCoroutine(SonySaveDataDialogs.CreateTestSaveData(this));
        }

        if (m_MenuDialogs.AddItem("Start Save Dialog Process", "Start the save dialog process."))
        {
            SaveProcess();
        }

        if (m_MenuDialogs.AddItem("Start Save Dialog (No new item)", "Start the save dialog process but don't provide a new item to create a new save"))
        {
            SaveProcess(true);
        }

        if (m_MenuDialogs.AddItem("Start Load Dialog Process", "Start the load dialog process."))
        {
            LoadProcess();
        }

        if (m_MenuDialogs.AddItem("Start Delete Dialog Process", "Start the delete dialog process."))
        {
            DeleteProcess();
        }

        if (m_MenuDialogs.AddItem("Test Dialog Methods", "Test each dialog type without doing any real file IO."))
        {
            menuStack.PushMenu(m_DialogTests.GetMenu());
        }

        if (m_MenuDialogs.AddBackIndex("Back"))
        {
            menuStack.PopMenu();
        }
    }

    public static IEnumerator CreateTestSaveData(SonySaveDataDialogs dialogs)
    {
        OnScreenLog.AddNewLine();
        OnScreenLog.AddWarning("Creating Test Save Data");
        OnScreenLog.AddNewLine();
        OnScreenLog.AddNewLine();
        OnScreenLog.Add("This will create a series of savedata directories for testing the various different states for the savdata dialog state machine.");
        OnScreenLog.Add("It will create a normal savedata, without backup, and one with a backup. Each of these can be overwritten.");
        OnScreenLog.Add("It will also create a couple of savedata which has details showing they should be flagged as corrupted. ");
        OnScreenLog.Add("One of these has a backup to test what happen when trying to overwrite a corrupted savedata with backup");

        OnScreenLog.AddNewLine();
        OnScreenLog.Add("This will block for a short time while all the test savedata is created.");
        OnScreenLog.AddNewLine();

        OnScreenLog.AddWarning("This will also delete anysave data beginning with \"Example\". Make sure no other savedata is mounted otherwise errors might occur.");
        OnScreenLog.AddWarning("Wait until the next warning message is displayed indicating the test data has been created.");

        OnScreenLog.AddNewLine();

        yield return null;

        OnScreenLog.Add("Delete any current 'Example' savedata...");

        Searching.DirNameSearchResponse searchResponse = new Searching.DirNameSearchResponse();

        dialogs.TestSaveDataSearch(searchResponse);

        if (searchResponse.SaveDataItems != null)
        {
            for (int i = 0; i < searchResponse.SaveDataItems.Length; i++)
            {
                DirName toDelete = searchResponse.SaveDataItems[i].DirName;
                OnScreenLog.Add("   Deleting " + toDelete);

                yield return null;

                dialogs.Delete(toDelete);
            }
        }

        // Test1
        OnScreenLog.Add("Creating Example1 savedata...");
        dialogs.CreateTestSaveData("Example1", false, false); // No backup

        yield return null;
        // Test2
        OnScreenLog.Add("Creating Example2 savedata...");
        dialogs.CreateTestSaveData("Example2", true, false); // Backup.

        yield return null;
        // Test3
        OnScreenLog.Add("Creating Example3 savedata...");
        dialogs.CreateTestSaveData("Example3", true, true); // Backup, Mark as corrupt test.

        yield return null;

        // Test4
        OnScreenLog.Add("Creating Example4 savedata...");
        dialogs.CreateTestSaveData("Example4", false, true); // No backup. Mark as corrupt test.

        yield return null;

        OnScreenLog.AddNewLine();
        OnScreenLog.Add("Finished creating test savedata...");

        OnScreenLog.AddNewLine();
        OnScreenLog.AddWarning("To create the corrupt saves follow these steps: ");
        OnScreenLog.AddWarning("   For an installed package build: ");
        OnScreenLog.AddWarning("      1) Press the 'PS' button. ");
        OnScreenLog.AddWarning("      2) Press the 'Options' button on the sample app.");
        OnScreenLog.AddWarning("      3) Select the '*Saved Data Management' option and press 'OK' if a warning message is shown.");
        OnScreenLog.AddWarning("      4) Select the 'Saved Data in System Storage' option");
        OnScreenLog.AddWarning("      5) Select the '+Upload to Online Storage' option");
        OnScreenLog.AddWarning("      6) Select the sample app");
        OnScreenLog.AddWarning("      7) Find the first save whose details show it should be flagged as corrupted and press the options button");
        OnScreenLog.AddWarning("      8) Select the '*Fake Save Data Broken Status' option");
        OnScreenLog.AddWarning("      9) Select the 'On' option");
        OnScreenLog.AddWarning("     10) Repeat for the other save data that is labeled as being corrupt.");
        OnScreenLog.AddWarning("     11) Both save data should now be shown as 'Corrupted Data'");
        OnScreenLog.AddWarning("     12) Important : One of the corrupted saves has a backup which changes the flow of the dialogs, the other doesn't.");
    }

    public void CreateTestSaveData(string dirName, bool backup, bool flagAsCorrupt)
    {
        Mounting.MountPoint mp = Mount(dirName);

        WriteFiles(mp);
        SetMountParams(mp, backup, flagAsCorrupt);
        SaveIconFromFile(mp);

        Unmount(mp);
    }

    public void TestSaveDataSearch(Searching.DirNameSearchResponse response)
    {
        try
        {
            Searching.DirNameSearchRequest request = new Searching.DirNameSearchRequest();

            DirName searchDirName = new DirName();
            searchDirName.Data = "Example%";

            request.UserId = User.GetActiveUserId;
            request.Key = Searching.SearchSortKey.DirName;
            request.Order = Searching.SearchSortOrder.Ascending;
            request.DirName = searchDirName;
            request.MaxDirNameCount = Searching.DirNameSearchRequest.DIR_NAME_MAXSIZE;
            request.Async = false;

            int errorCode = Searching.DirNameSearch(request, response);

            if (errorCode != (int)ReturnCodes.SUCCESS)
            {
                OnScreenLog.AddError("Search Error : " + errorCode.ToString("X8"));
            }
        }
        catch (SaveDataException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void Delete(DirName dirName)
    {
        try
        {
            Deleting.DeleteRequest request = new Deleting.DeleteRequest();

            request.UserId = User.GetActiveUserId;
            request.DirName = dirName;
            request.Async = false;

            EmptyResponse response = new EmptyResponse();

            int errorCode = Deleting.Delete(request, response);

            if (errorCode != (int)ReturnCodes.SUCCESS)
            {
                OnScreenLog.AddError("Delete Error : " + errorCode.ToString("X8"));
            }
        }
        catch (SaveDataException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public Mounting.MountPoint Mount(string directoryName)
    {
        try
        {
            Mounting.MountRequest request = new Mounting.MountRequest();

            DirName dirName = new DirName();
            dirName.Data = directoryName;

            request.UserId = User.GetActiveUserId;
            request.Async = false;
            request.DirName = dirName;
            request.MountMode = Mounting.MountModeFlags.Create2 | Mounting.MountModeFlags.ReadWrite;
            request.Blocks = SonySaveDataMain.TestBlockSize;
            // request.IgnoreCallback = true;

            Mounting.MountResponse response = new Mounting.MountResponse();

            int errorCode = Mounting.Mount(request, response);

            if (errorCode != (int)ReturnCodes.SUCCESS)
            {
                OnScreenLog.AddError("Mount Error : " + errorCode.ToString("X8"));
                return null;
            }

            return response.MountPoint;
        }
        catch (SaveDataException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }

        return null;
    }

    public void Unmount(Mounting.MountPoint mp)
    {
        try
        {
            Mounting.UnmountRequest request = new Mounting.UnmountRequest();

            if (mp == null) return;

            request.UserId = User.GetActiveUserId;
            request.MountPointName = mp.PathName;
            request.Async = false;
            //  request.IgnoreCallback = true;

            EmptyResponse response = new EmptyResponse();

            int errorCode = Mounting.Unmount(request, response);

            if (errorCode != (int)ReturnCodes.SUCCESS)
            {
                OnScreenLog.AddError("Unmount Error : " + errorCode.ToString("X8"));
            }
        }
        catch (SaveDataException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void WriteFiles(Mounting.MountPoint mp)
    {
        try
        {
            ExampleWriteFilesRequest request = new ExampleWriteFilesRequest();

            request.UserId = User.GetActiveUserId;
            request.MountPointName = mp.PathName;
            request.Async = false;

            request.myTestData = "This is some text test data which will be written to a file. " + OnScreenLog.FrameCount;
            request.myOtherTestData = "This is some more text which is written to another save file. " + OnScreenLog.FrameCount;

            string outputStr = "";
            for (int i = 0; i < 20; i++)
            {
                request.largeData[i] = (byte)(OnScreenLog.FrameCount + i);

                if (i > 0) outputStr += ", ";
                outputStr += request.largeData[i];
            }
         
            OnScreenLog.Add("Write Data = " + outputStr);

            ExampleWriteFilesResponse response = new ExampleWriteFilesResponse();

            int errorCode = FileOps.CustomFileOp(request, response);

            if (errorCode != (int)ReturnCodes.SUCCESS)
            {
                OnScreenLog.AddError("Write files Error : " + errorCode.ToString("X8"));
            }
        }
        catch (SaveDataException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void SetMountParams(Mounting.MountPoint mp, bool backup, bool flagAsCorrupt)
    {
        try
        {
            Mounting.SetMountParamsRequest request = new Mounting.SetMountParamsRequest();

            request.UserId = User.GetActiveUserId;
            request.MountPointName = mp.PathName;

            SaveDataParams sdParams = new SaveDataParams();

            if (flagAsCorrupt == false)
            {
                sdParams.Title = "My Save Data " + OnScreenLog.FrameCount;
                sdParams.SubTitle = "My Save Data Subtitle " + OnScreenLog.FrameCount;
                sdParams.Detail = "This is the long descrition of the save data.";
            }
            else
            {
                sdParams.Title = "Corrupt savedata test " + OnScreenLog.FrameCount;
                sdParams.SubTitle = "Corrupt savedata test ";
                sdParams.Detail = "This is the long descrition of the corrupt savedata test.";

                if (backup == true)
                {
                    sdParams.SubTitle = sdParams.SubTitle + ", with backup ";
                }

                sdParams.SubTitle = sdParams.SubTitle + OnScreenLog.FrameCount;
            }

            sdParams.UserParam = (UInt32)OnScreenLog.FrameCount;

            request.Params = sdParams;
            request.Async = false;
            //  request.IgnoreCallback = true;

            EmptyResponse response = new EmptyResponse();

            Mounting.SetMountParams(request, response);
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

            request.Async = false;
            //  request.IgnoreCallback = true;

            EmptyResponse response = new EmptyResponse();

            Mounting.SaveIcon(request, response);
        }
        catch (SaveDataException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public static bool AllowNewItemCB_Disable(Searching.DirNameSearchResponse response)
    {
        return false;
    }

    public void SaveProcess(bool hideNewItem = false)
    {
        // Get the user id for the saves
        int userId = User.GetActiveUserId;

        // Create the new item for the saves dialog list
        Dialogs.NewItem newItem = null;

        string title = "Testing new item title with save list " + OnScreenLog.FrameCount;

        newItem = new Dialogs.NewItem();
        //newItem.IconPath = "/app0/Media/StreamingAssets/SaveIcon.png";
        newItem.Title = title;

        // The directory name for a new savedata
        DirName newDirName = new DirName();
        newDirName.Data = "Example" + OnScreenLog.FrameCount;

        // Should the savedata be backed up automaitcally when the savedata is unmounted.
        bool backup = true;

        // What size should a new save data be created.
        UInt64 newSaveDataBlocks = SonySaveDataMain.TestBlockSize;

        // Parameters to use for the savedata
        SaveDataParams saveDataParams = new SaveDataParams();

        saveDataParams.Title = title;
        saveDataParams.SubTitle = "Subtitle for savedata " + OnScreenLog.FrameCount;
        saveDataParams.Detail = "Details for savedata " + OnScreenLog.FrameCount;
        saveDataParams.UserParam = (uint)OnScreenLog.FrameCount;

        // Actual custom file operation to perform on the savedata, once it is mounted.
        ExampleWriteFilesRequest fileRequest = new ExampleWriteFilesRequest();
        fileRequest.myTestData = "This is some text test data which will be written to a file. " + OnScreenLog.FrameCount;
        fileRequest.myOtherTestData = "This is some more text which is written to another save file. " + OnScreenLog.FrameCount;
        fileRequest.IgnoreCallback = false; // In this example get a async callback once the file operations are complete

        string outputStr = "";
        for (int i = 0; i < 20; i++)
        {
            fileRequest.largeData[i] = (byte)(OnScreenLog.FrameCount + i);

            if (i > 0) outputStr += ", ";
            outputStr += fileRequest.largeData[i];
        }

        OnScreenLog.Add("Write Data = " + outputStr);

        ExampleWriteFilesResponse fileResponse = new ExampleWriteFilesResponse();

        SaveDataDialogProcess.AllowNewItemTest allowNewItemCB = null;

        if(hideNewItem == true)
        {
            allowNewItemCB = AllowNewItemCB_Disable;
        }

        SonySaveDataMain.StartSaveDataCoroutine(StartSaveDialogProcessWithScreenshot(userId, newItem, newDirName, newSaveDataBlocks, saveDataParams, fileRequest, fileResponse, backup, allowNewItemCB, screenShotHelper));
    }

    public static IEnumerator StartSaveDialogProcessWithScreenshot(int userId, Dialogs.NewItem newItem, DirName newDirName, ulong newSaveDataBlocks, SaveDataParams saveDataParams,
        FileOps.FileOperationRequest fileRequest, FileOps.FileOperationResponse fileResponse, bool backup, SaveDataDialogProcess.AllowNewItemTest allowNewItemCB, GetScreenShot screenShotHelper)
    {
        screenShotHelper.DoScreenShot();

        while (screenShotHelper.IsWaiting == true)
        {
            yield return null;
        }

        // Can't do this yet as images aren't supported in the Dialog
        //newItem.RawPNG = screenShotHelper.screenShotBytes;

        SonySaveDataMain.StartSaveDataCoroutine(SaveDataDialogProcess.StartSaveDialogProcess(userId, newItem, newDirName, newSaveDataBlocks, saveDataParams, fileRequest, fileResponse, backup, allowNewItemCB));
    }

    public void LoadProcess()
    {
        // Get the user id for the saves
        int userId = User.GetActiveUserId;

        // Actual custom file operation to perform on the savedata, once it is mounted.
        ExampleReadFilesRequest fileRequest = new ExampleReadFilesRequest();
        fileRequest.IgnoreCallback = false; // In this example get a async callback once the file operations are complete
        ExampleReadFilesResponse fileResponse = new ExampleReadFilesResponse();

        SonySaveDataMain.StartSaveDataCoroutine(SaveDataDialogProcess.StartLoadDialogProcess(userId, fileRequest, fileResponse));
    }

    public void DeleteProcess()
    {
        // Get the user id for the saves
        int userId = User.GetActiveUserId;

        SonySaveDataMain.StartSaveDataCoroutine(SaveDataDialogProcess.StartDeleteDialogProcess(userId));
    }

    static DirName lastBackupDirname;

    public void OnAsyncEvent(SaveDataCallbackEvent callbackEvent)
    {
        m_DialogTests.OnAsyncEvent(callbackEvent);

        switch (callbackEvent.ApiCalled)
        {
            case FunctionTypes.NotificationUnmountWithBackup:
                {
                    UnmountWithBackupNotification response = callbackEvent.Response as UnmountWithBackupNotification;

                    if (response != null)
                    {
                        lastBackupDirname = response.DirName;
                    }
                }
                break;
        }
    }

}
#endif
