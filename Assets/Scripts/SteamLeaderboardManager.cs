using Com.MorganHouston.Imprecision;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteamLeaderboardManager : MonoBehaviour
{
    private static SteamLeaderboardManager instance;

    public static SteamLeaderboardManager Instance { get { return instance; } }

    private SteamLeaderboard_t s_currentLeaderboard;
    private bool s_initialized = false;
    private CallResult<LeaderboardFindResult_t> m_findResult = new CallResult<LeaderboardFindResult_t>();
    private CallResult<LeaderboardScoreUploaded_t> m_uploadResult = new CallResult<LeaderboardScoreUploaded_t>();
    private CallResult<LeaderboardScoresDownloaded_t> m_downloadResult = new CallResult<LeaderboardScoresDownloaded_t>();

    public enum LeaderboardName
    {
        AllTimeMostPoints,
        AllTimeMostArrowsFired,
        AllTimeMostTargetsHit,
        AllTimeMostBullseyesHit,
        AllTimeMostJewelsCollected,
        AllTimeBestAccuracy
    }

    public struct LeaderboardData
    {
        public string username;
        public int rank;
        public int score;
    }
    List<LeaderboardData> LeaderboardDataset;

    public List<string> leaderboardNames;
    List<SteamLeaderboard_t> steamLeaderboards = new List<SteamLeaderboard_t>();
    public List<bool> steamLeaderboardsInit = new List<bool>();
    int leaderboardToBeInit = 0;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            instance = this;
            //DontDestroyOnLoad(Instance.gameObject);
        }

        if (SteamManager.Initialized && CloudSaveLogin.Instance.currentSSO == CloudSaveLogin.ssoOption.Steam)
        {
            SteamAPICall_t hSteamAPICall = SteamUserStats.FindLeaderboard(leaderboardNames[leaderboardToBeInit]);
            m_findResult.Set(hSteamAPICall, OnLeaderboardFindResult);
            leaderboardToBeInit++;
            SteamAPI.RunCallbacks();

            SteamAPICall_t hSteamAPICall2 = SteamUserStats.FindLeaderboard(leaderboardNames[leaderboardToBeInit]);
            m_findResult.Set(hSteamAPICall2, OnLeaderboardFindResult);
            leaderboardToBeInit++;
            SteamAPI.RunCallbacks();

            SteamAPICall_t hSteamAPICall3 = SteamUserStats.FindLeaderboard(leaderboardNames[leaderboardToBeInit]);
            m_findResult.Set(hSteamAPICall3, OnLeaderboardFindResult);
            leaderboardToBeInit++;
            SteamAPI.RunCallbacks();

            SteamAPICall_t hSteamAPICall4 = SteamUserStats.FindLeaderboard(leaderboardNames[leaderboardToBeInit]);
            m_findResult.Set(hSteamAPICall4, OnLeaderboardFindResult);
            leaderboardToBeInit++;
            SteamAPI.RunCallbacks();

            SteamAPICall_t hSteamAPICall5 = SteamUserStats.FindLeaderboard(leaderboardNames[leaderboardToBeInit]);
            m_findResult.Set(hSteamAPICall5, OnLeaderboardFindResult);
            leaderboardToBeInit++;
            SteamAPI.RunCallbacks();

            SteamAPICall_t hSteamAPICall6 = SteamUserStats.FindLeaderboard(leaderboardNames[leaderboardToBeInit]);
            m_findResult.Set(hSteamAPICall6, OnLeaderboardFindResult);
            SteamAPI.RunCallbacks();
        }

    }

    public void UpdateScore(int score, LeaderboardName leaderboardName)
    {
        if (!steamLeaderboardsInit[(int)leaderboardName])
        {
            SteamAPICall_t hSteamAPICall = SteamUserStats.FindLeaderboard(name);
            m_findResult.Set(hSteamAPICall, OnLeaderboardFindResult);
            SteamAPI.RunCallbacks();
            UpdateScore(score, leaderboardName);
        }
        else
        {
            //Change upload method to 
            SteamAPICall_t hSteamAPICall = SteamUserStats.UploadLeaderboardScore(steamLeaderboards[(int)leaderboardName], ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest, score, null, 0);
            m_uploadResult.Set(hSteamAPICall, OnLeaderboardUploadResult);
            SteamAPI.RunCallbacks();
        }
    }

    private void OnLeaderboardFindResult(LeaderboardFindResult_t pCallback, bool failure)
    {
        Debug.Log($"Steam Leaderboard Find: Did it fail? {failure}, Found: {pCallback.m_bLeaderboardFound}, leaderboardID: {pCallback.m_hSteamLeaderboard.m_SteamLeaderboard}");
        //leaderboardToBeInit++;
        steamLeaderboards.Add(pCallback.m_hSteamLeaderboard);
        steamLeaderboardsInit.Add(true);

        //InitializeNextLeaderboard();
    }

    private void InitializeNextLeaderboard()
    {
        if (leaderboardToBeInit < leaderboardNames.Count)
        {
            SteamAPICall_t hSteamAPICall = SteamUserStats.FindLeaderboard(leaderboardNames[leaderboardToBeInit]);
            m_findResult.Set(hSteamAPICall, OnLeaderboardFindResult);
        }
    }

    private void OnLeaderboardUploadResult(LeaderboardScoreUploaded_t pCallback, bool failure)
    {
        Debug.Log($"Steam Leaderboard Upload: Did it fail? {failure}, Score: {pCallback.m_nScore}, HasChanged: {pCallback.m_bScoreChanged}");
    }

    //change ELeaderboardDataRequest to get a different set (focused around player or global)
    public void GetLeaderBoardData(ELeaderboardDataRequest _type = ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal, int entries = 14)
    {
        SteamAPICall_t hSteamAPICall;
        switch (_type)
        {
            case ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal:
                hSteamAPICall = SteamUserStats.DownloadLeaderboardEntries(s_currentLeaderboard, _type, 1, entries);
                m_downloadResult.Set(hSteamAPICall, OnLeaderboardDownloadResult);
                break;
            case ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobalAroundUser:
                hSteamAPICall = SteamUserStats.DownloadLeaderboardEntries(s_currentLeaderboard, _type, -(entries / 2), (entries / 2));
                m_downloadResult.Set(hSteamAPICall, OnLeaderboardDownloadResult);
                break;
            case ELeaderboardDataRequest.k_ELeaderboardDataRequestFriends:
                hSteamAPICall = SteamUserStats.DownloadLeaderboardEntries(s_currentLeaderboard, _type, 1, entries);
                m_downloadResult.Set(hSteamAPICall, OnLeaderboardDownloadResult);
                break;
        }
        //Note that the LeaderboardDataset will not be updated immediatly (see callback below)
    }

    private void OnLeaderboardDownloadResult(LeaderboardScoresDownloaded_t pCallback, bool failure)
    {
        Debug.Log($"Steam Leaderboard Download: Did it fail? {failure}, Result - {pCallback.m_hSteamLeaderboardEntries}");
        LeaderboardDataset = new List<LeaderboardData>();
        //Iterates through each entry gathered in leaderboard
        for (int i = 0; i < pCallback.m_cEntryCount; i++)
        {
            LeaderboardEntry_t leaderboardEntry;
            SteamUserStats.GetDownloadedLeaderboardEntry(pCallback.m_hSteamLeaderboardEntries, i, out leaderboardEntry, null, 0);
            //Example of how leaderboardEntry might be held/used
            LeaderboardData lD;
            lD.username = SteamFriends.GetFriendPersonaName(leaderboardEntry.m_steamIDUser);
            lD.rank = leaderboardEntry.m_nGlobalRank;
            lD.score = leaderboardEntry.m_nScore;
            LeaderboardDataset.Add(lD);
            Debug.Log($"User: {lD.username} - Score: {lD.score} - Rank: {lD.rank}");
        }
        //This is the callback for my own project - function is asynchronous so it must return from here rather than from GetLeaderBoardData
        //FindObjectOfType<HighscoreUIMan>().FillLeaderboard(LeaderboardDataset);
    }
}