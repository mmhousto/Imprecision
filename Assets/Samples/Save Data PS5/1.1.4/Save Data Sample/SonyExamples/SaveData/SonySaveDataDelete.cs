
#if UNITY_PS5

using Unity.SaveData.PS5;
using Unity.SaveData.PS5.Core;
using Unity.SaveData.PS5.Delete;

public class SonySaveDataDelete : IScreen
{
    MenuLayout m_MenuDelete;

    public SonySaveDataDelete()
    {
        Initialize();
    }

    public MenuLayout GetMenu()
    {
        return m_MenuDelete;
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
        m_MenuDelete = new MenuLayout(this, 530, 20);
    }

    public void MenuUserProfiles(MenuStack menuStack)
    {
        m_MenuDelete.Update();

        bool isEnabled = SaveDataDirNames.HasCurrentDirName();

        string dirName = SaveDataDirNames.GetCurrentDirName();

        string dirNameToolTip = "";

        if (isEnabled == true)
        {
            dirNameToolTip = " Use this on directory name \"" + dirName + "\".";
        }

        if (m_MenuDelete.AddItem("Delete", "Delete the save data."+ dirNameToolTip, isEnabled))
        {
            Delete();
        }

        if (m_MenuDelete.AddBackIndex("Back"))
        {
            menuStack.PopMenu();
        }
    }

    public void Delete()
    {
        try
        {
            Deleting.DeleteRequest request = new Deleting.DeleteRequest();

            DirName dirName = new DirName();
            dirName.Data = SaveDataDirNames.GetCurrentDirName();

            request.UserId = User.GetActiveUserId;
            request.DirName = dirName;

            EmptyResponse response = new EmptyResponse();

            int requestId = Deleting.Delete(request, response);

            OnScreenLog.Add("Delete Async : Request Id = " + requestId);
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
            case FunctionTypes.Delete:
                {
                    EmptyResponse response = callbackEvent.Response as EmptyResponse;

                    if (response != null && response.ReturnCode == ReturnCodes.SUCCESS)
                    {
                        Deleting.DeleteRequest request = callbackEvent.Request as Deleting.DeleteRequest;

                        if (request != null)
                        {
                            SaveDataDirNames.RemoveDirName(request.DirName.Data);
                        }
                    }
                }
                break;
        }
    }

}
#endif
