using System;

#if UNITY_PS5

using Unity.SaveData.PS5;
using Unity.SaveData.PS5.Core;
using Unity.SaveData.PS5.Dialog;
using Unity.SaveData.PS5.Search;

public class SonySaveDataDialogTests : IScreen
{
    MenuLayout m_MenuDialogTests;

    float progress = 0.0f;
    bool showingProgressBar = false;

    public SonySaveDataDialogTests()
    {
        Initialize();
    }

    public MenuLayout GetMenu()
    {
        return m_MenuDialogTests;
    }

    bool isActive = false;

    public void OnEnter()
    {
        isActive = true;
    }

    public void OnExit()
    {
        isActive = false;
    }

    public void Process(MenuStack stack)
    {
        if(showingProgressBar == true)
        {
            if (Dialogs.DialogIsReadyToDisplay() == true)
            {
                progress += 0.10f;
                Dialogs.ProgressBarSetValue((UInt32)progress);

                if (progress >= 100.0f)
                {
                    showingProgressBar = false;
                    progress = 0;

                    Dialogs.CloseParam closeParam = new Dialogs.CloseParam();
                    closeParam.Anim = Dialogs.Animation.On;

                    Dialogs.Close(closeParam);
                }
            }
        }

        MenuUserProfiles(stack);
    }

    public void Initialize()
    {
        m_MenuDialogTests = new MenuLayout(this, 530, 20);
    }

    public void MenuUserProfiles(MenuStack menuStack)
    {
        m_MenuDialogTests.Update();

        if (m_MenuDialogTests.AddItem("User Message Dialog", "Open an example user message dialog"))
        {
            UserMessageDialog();
        }

        if (m_MenuDialogTests.AddItem("System Message Dialog", "Open an example system message dialog"))
        {
            SystemMessageDialog();
        }

        if (m_MenuDialogTests.AddItem("Error Dialog", "Open an example error code dialog"))
        {
            ErrorDialog();
        }

        if (m_MenuDialogTests.AddItem("Progress Bar Dialog", "Open an example progress bar dialog"))
        {
            ProgressBarDialog();
        }

        if (m_MenuDialogTests.AddItem("List Dialog", "Open an example savedata list dialog"))
        {
            ListDialog();
        }

        if (m_MenuDialogTests.AddItem("PS4 Load List Dialog", "Open an example PS4 Save Load Dialog"))
        {
            PS4LoadListDialog();
        }

        if (m_MenuDialogTests.AddBackIndex("Back"))
        {
            menuStack.PopMenu();
        }
    }

    public void UserMessageDialog()
    {
        try
        {
            Dialogs.OpenDialogRequest request = new Dialogs.OpenDialogRequest();

            request.UserId = User.GetActiveUserId;
            request.Mode = Dialogs.DialogMode.UserMsg;
            request.DispType = Dialogs.DialogType.Save;

            request.Animations = new Dialogs.AnimationParam(Dialogs.Animation.On, Dialogs.Animation.On);

            Dialogs.UserMessageParam msg = new Dialogs.UserMessageParam();
            msg.MsgType = Dialogs.UserMessageType.Normal;
            msg.ButtonType = Dialogs.DialogButtonTypes.YesNo;
            msg.Message = "This is a test of the user message savedata dialog";

            request.UserMessage = msg;

            Dialogs.OpenDialogResponse response = new Dialogs.OpenDialogResponse();

            int requestId = Dialogs.OpenDialog(request, response);

            OnScreenLog.Add("OpenDialog Async : Request Id = " + requestId);
        }
        catch (SaveDataException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void SystemMessageDialog()
    {
        try
        {
            Dialogs.OpenDialogRequest request = new Dialogs.OpenDialogRequest();

            request.UserId = User.GetActiveUserId;
            request.Mode = Dialogs.DialogMode.SystemMsg;
            request.DispType = Dialogs.DialogType.Load;

            request.Animations = new Dialogs.AnimationParam(Dialogs.Animation.On, Dialogs.Animation.On);

            Dialogs.SystemMessageParam msg = new Dialogs.SystemMessageParam();
            msg.SysMsgType = Dialogs.SystemMessageType.Corrupted;

            Searching.DirNameSearchResponse searchItems = FullSearch();

            if (searchItems.SaveDataItems.Length > 0)
            {
                DirName[] dirNames = new DirName[1];
                dirNames[0] = searchItems.SaveDataItems[0].DirName;

                Dialogs.Items items = new Dialogs.Items();
                items.DirNames = dirNames;

                request.Items = items;

                request.SystemMessage = msg;

                Dialogs.OpenDialogResponse response = new Dialogs.OpenDialogResponse();

                int requestId = Dialogs.OpenDialog(request, response);

                OnScreenLog.Add("OpenDialog Async : Request Id = " + requestId);
            }

        }
        catch (SaveDataException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void ErrorDialog()
    {
        try
        {
            Dialogs.OpenDialogRequest request = new Dialogs.OpenDialogRequest();

            request.UserId = User.GetActiveUserId;
            request.Mode = Dialogs.DialogMode.ErrorCode;
            request.DispType = Dialogs.DialogType.Save;

            request.Animations = new Dialogs.AnimationParam(Dialogs.Animation.On, Dialogs.Animation.On);

            Dialogs.ErrorCodeParam errorParam = new Dialogs.ErrorCodeParam();
            errorParam.ErrorCode = unchecked((int)0x80B80006);

            request.ErrorCode = errorParam;

            Dialogs.OpenDialogResponse response = new Dialogs.OpenDialogResponse();

            int requestId = Dialogs.OpenDialog(request, response);

            OnScreenLog.Add("OpenDialog Async : Request Id = " + requestId);
        }
        catch (SaveDataException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void ProgressBarDialog()
    {
        try
        {
            Dialogs.OpenDialogRequest request = new Dialogs.OpenDialogRequest();

            request.UserId = User.GetActiveUserId;
            request.Mode = Dialogs.DialogMode.ProgressBar;
            request.DispType = Dialogs.DialogType.Save;

            request.Animations = new Dialogs.AnimationParam(Dialogs.Animation.On, Dialogs.Animation.On);

            Dialogs.ProgressBarParam progressBar = new Dialogs.ProgressBarParam();
            
            progressBar.BarType = Dialogs.ProgressBarType.Percentage;
            progressBar.SysMsgType = Dialogs.ProgressSystemMessageType.Progress;

            request.ProgressBar = progressBar;

            Dialogs.NewItem newItem = new Dialogs.NewItem();

            //Todo : Doesn't work in SDK 0.90 
            //newItem.IconPath = "/app0/Media/StreamingAssets/SaveIcon.png";
            newItem.Title = "Testing new item title with progress bar";

            request.NewItem = newItem;

            Dialogs.OpenDialogResponse response = new Dialogs.OpenDialogResponse();

            int requestId = Dialogs.OpenDialog(request, response);

            OnScreenLog.Add("OpenDialog Async : Request Id = " + requestId);
        }
        catch (SaveDataException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void ListDialog()
    {
        try
        {
            Searching.DirNameSearchResponse searchItems = FullSearch();

            DirName[] dirNames = null;

            if (searchItems.SaveDataItems.Length > 0)
            {
                dirNames = new DirName[searchItems.SaveDataItems.Length];

                for (int i = 0; i < searchItems.SaveDataItems.Length; i++)
                {
                    dirNames[i] = searchItems.SaveDataItems[i].DirName;
                }
            }

            Dialogs.OpenDialogRequest request = new Dialogs.OpenDialogRequest();

            request.UserId = User.GetActiveUserId;
            request.Mode = Dialogs.DialogMode.List;
            request.DispType = Dialogs.DialogType.Save;

            request.Animations = new Dialogs.AnimationParam(Dialogs.Animation.On, Dialogs.Animation.On);

            Dialogs.Items items = new Dialogs.Items();
          
            if(dirNames != null)
            {
                items.DirNames = dirNames;
            }

            items.FocusPos = Dialogs.FocusPos.DataLatest;
            items.ItemStyle = Dialogs.ItemStyle.SubtitleDataSize;

            request.Items = items;

            Dialogs.NewItem newItem = new Dialogs.NewItem();

            //Todo : Doesn't work in SDK 0.90 
            //newItem.IconPath = "/app0/Media/StreamingAssets/SaveIcon.png";
            newItem.Title = "Testing new item title with save list";

            request.NewItem = newItem;

            Dialogs.OpenDialogResponse response = new Dialogs.OpenDialogResponse();

            int requestId = Dialogs.OpenDialog(request, response);

            OnScreenLog.Add("OpenDialog Async : Request Id = " + requestId);
        }
        catch (SaveDataException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void PS4LoadListDialog()
    {
        try
        {
            Searching.DirNameSearchResponse searchItems = PS4Search();

            DirName[] dirNames = null;

            if (searchItems.SaveDataItems.Length > 0)
            {
                dirNames = new DirName[searchItems.SaveDataItems.Length];

                for (int i = 0; i < searchItems.SaveDataItems.Length; i++)
                {
                    dirNames[i] = searchItems.SaveDataItems[i].DirName;
                }
            }
            else
            {
                OnScreenLog.AddWarning("No PS4 Save Data found, create saves from the PS4 Save Game sample for some to show here");
            }

            Dialogs.OpenDialogRequest request = new Dialogs.OpenDialogRequest();

            request.UserId = User.GetActiveUserId;
            request.Mode = Dialogs.DialogMode.List;
            request.DispType = Dialogs.DialogType.Load;

            request.Animations = new Dialogs.AnimationParam(Dialogs.Animation.On, Dialogs.Animation.On);

            Dialogs.Items items = new Dialogs.Items();

            if (dirNames != null)
            {
                items.DirNames = dirNames;
            }

            items.FocusPos = Dialogs.FocusPos.DataLatest;
            items.ItemStyle = Dialogs.ItemStyle.SubtitleDataSize;
            items.TitleId = new TitleId()
            {
                Data = "NPXX51363" //PS4 Save Data Sample Title ID
            };

            request.Items = items;

            request.Option = new Dialogs.OptionParam()
            {
                Flag = Dialogs.OptionFlag.TargetPS4SaveData
            };

            Dialogs.OpenDialogResponse response = new Dialogs.OpenDialogResponse();

            int requestId = Dialogs.OpenDialog(request, response);

            OnScreenLog.Add("OpenDialog Async : Request Id = " + requestId);
        }
        catch (SaveDataException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public Searching.DirNameSearchResponse FullSearch()
    {
        try
        {
            Searching.DirNameSearchRequest request = new Searching.DirNameSearchRequest();

            request.UserId = User.GetActiveUserId;
            request.Key = Searching.SearchSortKey.DirName;
            request.Order = Searching.SearchSortOrder.Ascending;
            request.IncludeBlockInfo = true;
            request.IncludeParams = true;
            request.MaxDirNameCount = Searching.DirNameSearchRequest.DIR_NAME_MAXSIZE;
            request.Async = false;

            Searching.DirNameSearchResponse response = new Searching.DirNameSearchResponse();

            Searching.DirNameSearch(request, response);

            return response;
        }
        catch (SaveDataException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }

        return null;
    }


    public Searching.DirNameSearchResponse PS4Search()
    {
        try
        {
            Searching.DirNameSearchRequest request = new Searching.DirNameSearchRequest();

            request.UserId = User.GetActiveUserId;
            request.Key = Searching.SearchSortKey.DirName;
            request.Order = Searching.SearchSortOrder.Ascending;
            request.IncludeBlockInfo = true;
            request.IncludeParams = true;
            request.MaxDirNameCount = Searching.DirNameSearchRequest.DIR_NAME_MAXSIZE;
            request.Async = false;
            request.SearchPS4 = true;
            request.TitleId = new TitleId()
            {
                Data = "NPXX51363" //PS4 Save Data Sample Title ID
            };


            Searching.DirNameSearchResponse response = new Searching.DirNameSearchResponse();

            Searching.DirNameSearch(request, response);

            return response;
        }
        catch (SaveDataException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }

        return null;
    }
    public void OnAsyncEvent(SaveDataCallbackEvent callbackEvent)
    {
        if (isActive == false) return;

        switch (callbackEvent.ApiCalled)
        {
            case FunctionTypes.NotificationDialogOpened:
                {
                    Dialogs.OpenDialogRequest request = callbackEvent.Request as Dialogs.OpenDialogRequest;

                    if (request.Mode == Dialogs.DialogMode.ProgressBar)
                    {
                        showingProgressBar = true;
                        progress = 0;
                    }
                }
                break;
            case FunctionTypes.OpenDialog:
                {
                    OpenDialogResponseOutput(callbackEvent.Response as Dialogs.OpenDialogResponse);
                }
                break;
        }
    }

    public void OpenDialogResponseOutput(Dialogs.OpenDialogResponse response)
    {
        if (response != null)
        {
            Dialogs.DialogResult result = response.Result;

            if(result == null)
            {
                OnScreenLog.Add("Error occured when opening dialog");
                return;
            }

            OnScreenLog.Add("Mode : " + result.Mode);
            OnScreenLog.Add("Result : " + result.CallResult);
            OnScreenLog.Add("ButtonId : " + result.ButtonId);
            OnScreenLog.Add("DirName : " + result.DirName.Data);
            OnScreenLog.Add("Params :");
            OnScreenLog.Add("   Title : " + result.Param.Title);
            OnScreenLog.Add("   SubTitle : " + result.Param.SubTitle);
            OnScreenLog.Add("   Detail : " + result.Param.Detail);
            OnScreenLog.Add("   UserParam : " + result.Param.UserParam);
            OnScreenLog.Add("   Time : " + result.Param.Time.ToString("d/M/yyyy HH:mm:ss"));
        }
    }

}
#endif
