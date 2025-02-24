
#if UNITY_PS5

using Unity.SaveData.PS5;
using Unity.SaveData.PS5.Core;
using Unity.SaveData.PS5.Search;

public class SonySaveDataSearch : IScreen
{
    MenuLayout m_MenuSearch;

    public SonySaveDataSearch()
    {
        Initialize();
    }

    public MenuLayout GetMenu()
    {
        return m_MenuSearch;
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
        m_MenuSearch = new MenuLayout(this, 530, 20);
    }

    public void MenuUserProfiles(MenuStack menuStack)
    {
        m_MenuSearch.Update();

        if (m_MenuSearch.AddItem("Simple Search", "Search for last 10 save data directory names sorted by time."))
        {
            Search();
        }

        if (m_MenuSearch.AddItem("Full Search", "Search for all save data directory names including params and block sizes."))
        {
            FullSearch();
        }

        if (m_MenuSearch.AddItem("PS4 Search", "Search for all PS4 save data directory names including params and block sizes."))
        {
            PS4Search();
        }

        if (m_MenuSearch.AddBackIndex("Back"))
        {
            menuStack.PopMenu();
        }
    }

    public void Search()
    {
        try
        {
            Searching.DirNameSearchRequest request = new Searching.DirNameSearchRequest();

            request.UserId = User.GetActiveUserId;
            request.Key = Searching.SearchSortKey.Time;
            request.Order = Searching.SearchSortOrder.Ascending;
            request.IncludeBlockInfo = false;
            request.IncludeParams = false;
            request.MaxDirNameCount = 10;

            Searching.DirNameSearchResponse response = new Searching.DirNameSearchResponse();

            int requestId = Searching.DirNameSearch(request, response);

            OnScreenLog.Add("DirNameSearch Async : Request Id = " + requestId);
        }
        catch (SaveDataException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void FullSearch()
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

            Searching.DirNameSearchResponse response = new Searching.DirNameSearchResponse();

            int requestId = Searching.DirNameSearch(request, response);

            OnScreenLog.Add("DirNameSearch Async : Request Id = " + requestId);
        }
        catch (SaveDataException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }


    public void PS4Search()
    {

        try
        {
            Searching.DirNameSearchRequest request = new Searching.DirNameSearchRequest();

            TitleId titleId = new TitleId();
            titleId.Data = "NPXX51363";     // example title id ... Unity's PS4 sample title id ... Also needs to be set in the "titleIdForTransferringPs4" section of the param.json

  

            request.UserId = User.GetActiveUserId;
            request.Key = Searching.SearchSortKey.DirName;
            request.Order = Searching.SearchSortOrder.Ascending;
            request.IncludeBlockInfo = true;
            request.IncludeParams = true;
            request.MaxDirNameCount = Searching.DirNameSearchRequest.DIR_NAME_MAXSIZE;
            request.SearchPS4 = true;
            request.TitleId = titleId;

            Searching.DirNameSearchResponse response = new Searching.DirNameSearchResponse();

            int requestId = Searching.DirNameSearch(request, response);

            OnScreenLog.Add("DirNameSearch Async : Request Id = " + requestId);
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
            case FunctionTypes.DirNameSearch:
                {
                    DirNameSearchReponseOutput(callbackEvent.Response as Searching.DirNameSearchResponse);
                }
                break;
        }
    }

    public void DirNameSearchReponseOutput(Searching.DirNameSearchResponse response)
    {
        SaveDataDirNames.ClearAllNames();

        if (response != null)
        {
            bool hasParams = response.HasParams;
            bool hasInfo = response.HasInfo;

            var saveDataItems = response.SaveDataItems;

            OnScreenLog.Add("Search Found " + saveDataItems.Length + " saves");

            if ( saveDataItems.Length == 0 )
            {
                OnScreenLog.Add("Search didn't find any saves for this user.");
            }

            for (int i = 0; i < saveDataItems.Length; i++)
            {
                var dirName = saveDataItems[i].DirName;

                SaveDataDirNames.AddDirName(dirName);

                OnScreenLog.Add("DirName : " + dirName.Data);

                if (hasParams == true)
                {
                    var sdParams = saveDataItems[i].Params;

                    OnScreenLog.Add("   Title : " + sdParams.Title);
                    OnScreenLog.Add("   SubTitle : " + sdParams.SubTitle);
                    OnScreenLog.Add("   Detail : " + sdParams.Detail);
                    OnScreenLog.Add("   UserParam : " + sdParams.UserParam);
                    OnScreenLog.Add("   Time : " + sdParams.Time.ToString("d/M/yyyy HH:mm:ss"));
                }

                if (hasInfo == true)
                {
                    var sdInfo = saveDataItems[i].Info;

                    OnScreenLog.Add("   Blocks : " + sdInfo.Blocks);
                    OnScreenLog.Add("   FreeBlocks : " + sdInfo.FreeBlocks);
                }
            }
        }
    }

}
#endif
