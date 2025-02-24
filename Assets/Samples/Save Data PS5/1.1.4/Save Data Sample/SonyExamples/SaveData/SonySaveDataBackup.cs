


#if UNITY_PS5

using Unity.SaveData.PS5;
using Unity.SaveData.PS5.Backup;
using Unity.SaveData.PS5.Core;

public class SonySaveDataBackup : IScreen
{
    MenuLayout m_MenuBackup;

    public SonySaveDataBackup()
    {
        Initialize();
    }

    public MenuLayout GetMenu()
    {
        return m_MenuBackup;
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
        m_MenuBackup = new MenuLayout(this, 530, 20);
    }

    public void MenuUserProfiles(MenuStack menuStack)
    {
        m_MenuBackup.Update();

        bool isEnabled = SaveDataDirNames.HasCurrentDirName();

        string dirName = SaveDataDirNames.GetCurrentDirName();

        string dirNameToolTip = "";

        if (isEnabled == true)
        {
            dirNameToolTip = " Use this on directory name \"" + dirName + "\".";
        }

        if (m_MenuBackup.AddItem("Backup", "Backup the current save data directory." + dirNameToolTip + " Backup for current save will be skipped if save was created with rollback enabled (see Mounting.MountRequest() for more details).", isEnabled))
        {
            Backup();
        }

        if (m_MenuBackup.AddBackIndex("Back"))
        {
            menuStack.PopMenu();
        }
    }

    /// <summary>
    /// Creates a backup for the current save directory. Backup for current save will be skipped if save was created with rollback enabled (see Mounting.MountRequest() for more details).
    /// </summary>
    public void Backup()
    {
        try
        {
            Backups.BackupRequest request = new Backups.BackupRequest();

            DirName dirName = new DirName();
            dirName.Data = SaveDataDirNames.GetCurrentDirName();

            request.UserId = User.GetActiveUserId;
            request.DirName = dirName;

            EmptyResponse response = new EmptyResponse();

            int requestId = Backups.Backup(request, response);

            OnScreenLog.Add("Backup Async : Request Id = " + requestId);
        }
        catch (SaveDataException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void OnAsyncEvent(SaveDataCallbackEvent callbackEvent)
    {
        //switch (callbackEvent.ApiCalled)
        //{
        //}
    }

}
#endif
