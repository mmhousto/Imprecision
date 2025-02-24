
using System.Collections.Generic;
using UnityEngine;

#if UNITY_PS5 || UNITY_PS4
using Unity.PSN.PS5.Aysnc;
#if UNITY_PS5
using Unity.PSN.PS5.Trophies;
using Unity.PSN.PS5.UDS;
#endif
#endif


namespace PSNSample
{
#if UNITY_PS5
    public class SonyNpTrophies : IScreen
    {

        MenuLayout m_MenuTrophies;

        public SonyNpTrophies()
        {
            Initialize();

            TrophySystem.OnUnlockNotification += OnUnlockNotification;
        }

        public MenuLayout GetMenu()
        {
            return m_MenuTrophies;
        }

        public void OnEnter()
        {
        }

        public void OnExit()
        {
        }

        public void Process(MenuStack stack)
        {
            MenuTrophies(stack);
        }

        public void Initialize()
        {
            m_MenuTrophies = new MenuLayout(this, 450, 20);
        }

        public void Update(MeshRenderer iconRender)
        {
            for (int i = 0; i < pendingIcons.Count; i++)
            {
                if (pendingIcons[i] != null)
                {
                    OnScreenLog.Add("Game Icon : Size = " + pendingIcons[i].Image.width + " x " + pendingIcons[i].Image.height);
                    iconRender.material.SetTexture("_MainTex", pendingIcons[i].Image);
                }
            }

            pendingIcons.Clear();
        }


        public void MenuTrophies(MenuStack menuStack)
        {
            m_MenuTrophies.Update();

            bool enabled = TrophySystem.IsInitialized;

            if (m_MenuTrophies.AddItem("Start Trophy System", "Start the trophy system", !enabled))
            {
                TrophySystem.StartSystemRequest request = new TrophySystem.StartSystemRequest();

                var requestOp = new AsyncRequest<TrophySystem.StartSystemRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("System Started");
                    }
                });

                TrophySystem.Schedule(requestOp);
            }

            if (m_MenuTrophies.AddItem("Stop Trophy System", "Stop the trophy System", enabled))
            {
                TrophySystem.StopSystemRequest request = new TrophySystem.StopSystemRequest();

                var requestOp = new AsyncRequest<TrophySystem.StopSystemRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("System Stopped");
                    }
                });

                TrophySystem.Schedule(requestOp);
            }

            if (m_MenuTrophies.AddItem("Get Game Info", "Get game info for current user", enabled))
            {
                TrophySystem.TrophyGameDetails gameDetails = new TrophySystem.TrophyGameDetails();
                TrophySystem.TrophyGameData gameData = new TrophySystem.TrophyGameData();

                TrophySystem.GetGameInfoRequest request = new TrophySystem.GetGameInfoRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                    GameDetails = gameDetails,
                    GameData = gameData
                };

                var requestOp = new AsyncRequest<TrophySystem.GetGameInfoRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("GetGameInfoRequest completed");

                        OutputTrophyGameDetails(antecedent.Request.GameDetails);
                        OutputTrophyGameData(antecedent.Request.GameData);
                    }
                });

                TrophySystem.Schedule(requestOp);
            }

            if (m_MenuTrophies.AddItem("Get Group Info", "Get group info for current user", enabled))
            {
                GetGroupInfo(-1);
            }

            if (m_MenuTrophies.AddItem("Get Group Info (1)", "Get group info for current user", enabled))
            {
                GetGroupInfo(1);
            }

            if (m_MenuTrophies.AddItem("Get Trophy Info", "Get trophy info for current user", enabled))
            {
                GetAllTrophyState();
            }

            bool udsEnabled = UniversalDataSystem.IsInitialized;

            if (m_MenuTrophies.AddItem("Start UDS", "Start the Universal Data System to allow trophy unlocking", !udsEnabled))
            {
                UniversalDataSystem.StartSystemRequest request = new UniversalDataSystem.StartSystemRequest();

                request.PoolSize = 256 * 1024;

                var requestOp = new AsyncRequest<UniversalDataSystem.StartSystemRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("System Started");
                    }
                });

                UniversalDataSystem.Schedule(requestOp);
            }

            bool allowUnlocking = (numTrophiesReturned == (int)SampleTrophies.TrophyCount) && udsEnabled && enabled;

            if (m_MenuTrophies.AddItem("Unlock Next Trophy", "Unlock the next locked non-progress trophy", allowUnlocking))
            {
                if ( UniversalDataSystem.IsInitialized == true)
                {
                    UnlockNextLockedTrophy();
                }
                else
                {
                    OnScreenLog.AddError("The Universal Data System needs to be running. Use the UDS menu option to start it.");
                }
            }

            if (m_MenuTrophies.AddItem("Increase Progress", "Increase the progress on the basic progress trophy.", allowUnlocking))
            {
                if (UniversalDataSystem.IsInitialized == true)
                {
                    IncreaseBasicProgress();
                }
                else
                {
                    OnScreenLog.AddError("The Universal Data System needs to be running. Use the UDS menu option to start it.");
                }
            }

            if (m_MenuTrophies.AddItem("Increase Kill Count Stat", "Increase a progress stat that will unlock 2 different trophies", allowUnlocking))
            {
                if (UniversalDataSystem.IsInitialized == true)
                {
                    IncreaseProgressStat();
                }
                else
                {
                    OnScreenLog.AddError("The Universal Data System needs to be running. Use the UDS menu option to start it.");
                }
            }

            if (m_MenuTrophies.AddItem("Get Game Icon", "Get the Game Icon", enabled))
            {
                GetTrophyGameIcon();
            }

            if (m_MenuTrophies.AddItem("Get Group Icon", "Get the Group Icon", enabled))
            {
                GetTrophyGroupIcon(-1);
            }

            if (m_MenuTrophies.AddItem("Get Group Icon (1)", "Get the Group Icon", enabled))
            {
                GetTrophyGroupIcon(1);
            }

            if (m_MenuTrophies.AddItem("Get Trophy Icon (0)", "Get the Trophy Icon", enabled))
            {
                GetTrophyIcon(0);
            }

            if (m_MenuTrophies.AddItem("Get Reward Icon (8)", "Get the Reward Icon", enabled))
            {
                GetTrophyRewardIcon(8);
            }

            if (m_MenuTrophies.AddItem("Show Trophy List", "Show the list of trophies", enabled))
            {
                ShowTrophyList();
            }

            if (m_MenuTrophies.AddBackIndex("Back"))
            {
                menuStack.PopMenu();
            }
        }

        private void IncreaseProgressStat()
        {
            int id = (int)SampleTrophies.ProgressStatTwenty;

            if (currentData[id] != null && currentData[id].IsProgress == true)
            {
                long currentProgress = currentData[id].ProgressValue;

                currentProgress += 1;

                UniversalDataSystem.UDSEvent myEvent = new UniversalDataSystem.UDSEvent();

                myEvent.Create("UpdateKillCount");
                myEvent.Properties.Set("newKillCount", (int)currentProgress);

                UniversalDataSystem.PostEventRequest request = new UniversalDataSystem.PostEventRequest();

                request.UserId = GamePad.activeGamePad.loggedInUser.userId;
                request.CalculateEstimatedSize = false;
                request.EventData = myEvent;

                var requestOp = new AsyncRequest<UniversalDataSystem.PostEventRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("UpdateKillCount Event sent");
                        GetTrophyInfo(id);
                    }
                    else
                    {
                        OnScreenLog.AddError("Event send error");
                    }
                });

                UniversalDataSystem.Schedule(requestOp);
            }
        }

        private void IncreaseBasicProgress()
        {
            int id = (int)SampleTrophies.BasicProgress;

            if(currentData[id] != null && currentData[id].IsProgress == true)
            {
                long currentProgress = currentData[id].ProgressValue;

                currentProgress += 10;

                UnlockProgressTrophy(id, currentProgress);
            }
        }

        private void OnUnlockNotification(int trophyId)
        {
            OnScreenLog.AddWarning("OnUnlockNotification: Trophy Unlocked " + trophyId);

            GetTrophyInfo(trophyId);
        }

        public void UnlockNextLockedTrophy()
        {
            for (int i = 0; i < (int)SampleTrophies.TrophyCount; i++)
            {
                if ( currentData[i].Unlocked == false && currentData[i].IsProgress == false && currentDetails[i].TrophyGrade != 1)
                {
                    UnlockTrophy(currentData[i].TrophyId);
                    return;
                }
            }
        }

        public void UnlockTrophy(int id)
        {
            UniversalDataSystem.UnlockTrophyRequest request = new UniversalDataSystem.UnlockTrophyRequest();

            request.TrophyId = id;
            request.UserId = GamePad.activeGamePad.loggedInUser.userId;

            var getTrophyOp = new AsyncRequest<UniversalDataSystem.UnlockTrophyRequest>(request).ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                    OnScreenLog.Add("Trophy Unlock Request finished = " + antecedent.Request.TrophyId);
                }
            });

            UniversalDataSystem.Schedule(getTrophyOp);

            OnScreenLog.Add("Trophy Unlocking");
        }

        public void UnlockProgressTrophy(int id, long value)
        {
            UniversalDataSystem.UpdateTrophyProgressRequest request = new UniversalDataSystem.UpdateTrophyProgressRequest();

            request.TrophyId = id;
            request.UserId = GamePad.activeGamePad.loggedInUser.userId;
            request.Progress = value;

            var getTrophyOp = new AsyncRequest<UniversalDataSystem.UpdateTrophyProgressRequest>(request).ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                    OnScreenLog.Add("Progress Trophy Update Request finished = " + antecedent.Request.TrophyId + " : Progress = " + antecedent.Request.Progress);
                    GetTrophyInfo(id);
                }
            });

            UniversalDataSystem.Schedule(getTrophyOp);

            OnScreenLog.Add("Progress Trophy Updating");
        }

        public void GetGroupInfo(int groupId)
        {
            TrophySystem.GetGroupInfoRequest request = new TrophySystem.GetGroupInfoRequest();

            request.UserId = GamePad.activeGamePad.loggedInUser.userId;
            request.GroupId = groupId;
            request.GroupDetails = new TrophySystem.TrophyGroupDetails();
            request.GroupData = new TrophySystem.TrophyGroupData();

            var getTrophyOp = new AsyncRequest<TrophySystem.GetGroupInfoRequest>(request).ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                    OutputTrophyGroupDetails(antecedent.Request.GroupDetails);
                    OutputTrophyGroupData(antecedent.Request.GroupData);
                }
            });

            UniversalDataSystem.Schedule(getTrophyOp);

            OnScreenLog.Add("Getting Group Info");
        }

        enum SampleTrophies
        {
            Platinum = 0,
            BasicGold = 1,
            BasicSilver = 2,
            BasicBronze = 3,
            Hidden = 4,
            ProggreeStatThree = 5,
            ProgressStatTwenty = 6,
            BasicProgress = 7,
            Reward = 8,

            LastIndex = Reward,
            TrophyCount,
        }


        int numTrophiesReturned = 0;
        TrophySystem.TrophyDetails[] currentDetails;
        TrophySystem.TrophyData[] currentData;

        public void GetAllTrophyState()
        {
            currentDetails = new TrophySystem.TrophyDetails[(int)SampleTrophies.TrophyCount];
            currentData = new TrophySystem.TrophyData[(int)SampleTrophies.TrophyCount];

            numTrophiesReturned = 0;

            for (int i = 0; i < (int)SampleTrophies.TrophyCount; i++)
            {
                GetTrophyInfo(i);
            }
        }


        public void GetTrophyInfo(int trophyId)
        {
            OnScreenLog.Add("Getting info for trophy " + trophyId);

            TrophySystem.GetTrophyInfoRequest request = new TrophySystem.GetTrophyInfoRequest();

            request.UserId = GamePad.activeGamePad.loggedInUser.userId;
            request.TrophyId = trophyId;
            request.TrophyDetails = new TrophySystem.TrophyDetails();
            request.TrophyData = new TrophySystem.TrophyData();

            var getTrophyOp = new AsyncRequest<TrophySystem.GetTrophyInfoRequest>(request).ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                    OutputTrophyDetails(antecedent.Request.TrophyDetails);
                    OutputTrophyData(antecedent.Request.TrophyData);

                    int id = antecedent.Request.TrophyId;

                    if(currentDetails[id] == null)
                    {
                        numTrophiesReturned++;
                    }

                    currentDetails[id] = antecedent.Request.TrophyDetails;
                    currentData[id] = antecedent.Request.TrophyData;
                }
            });

            UniversalDataSystem.Schedule(getTrophyOp);
        }

        public List<TrophySystem.Icon> pendingIcons = new List<TrophySystem.Icon>();

        public void GetTrophyGameIcon()
        {
            TrophySystem.GetTrophyGameIconRequest request = new TrophySystem.GetTrophyGameIconRequest();

            request.UserId = GamePad.activeGamePad.loggedInUser.userId;

            var getTrophyOp = new AsyncRequest<TrophySystem.GetTrophyGameIconRequest>(request).ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                    if (request.Icon != null)
                    {
                        pendingIcons.Add(request.Icon);
                    }
                }
            });

            UniversalDataSystem.Schedule(getTrophyOp);

            OnScreenLog.Add("Getting Game Icon");
        }

        public void GetTrophyGroupIcon(int groupId)
        {
            TrophySystem.GetTrophyGroupIconRequest request = new TrophySystem.GetTrophyGroupIconRequest();

            request.UserId = GamePad.activeGamePad.loggedInUser.userId;
            request.GroupId = groupId;

            var getTrophyOp = new AsyncRequest<TrophySystem.GetTrophyGroupIconRequest>(request).ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                    if (request.Icon != null)
                    {
                        pendingIcons.Add(request.Icon);
                    }
                }
            });

            UniversalDataSystem.Schedule(getTrophyOp);

            OnScreenLog.Add("Getting Group Icon");
        }

        public void GetTrophyIcon(int trophyId)
        {
            TrophySystem.GetTrophyIconRequest request = new TrophySystem.GetTrophyIconRequest();

            request.UserId = GamePad.activeGamePad.loggedInUser.userId;
            request.TrophyId = trophyId;

            var getTrophyOp = new AsyncRequest<TrophySystem.GetTrophyIconRequest>(request).ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                    if (request.Icon != null)
                    {
                        pendingIcons.Add(request.Icon);
                    }
                }
            });

            UniversalDataSystem.Schedule(getTrophyOp);

            OnScreenLog.Add("Getting Trophy Icon");
        }

        public void GetTrophyRewardIcon(int trophyId)
        {
            TrophySystem.GetTrophyRewardIconRequest request = new TrophySystem.GetTrophyRewardIconRequest();

            request.UserId = GamePad.activeGamePad.loggedInUser.userId;
            request.TrophyId = trophyId;

            var getTrophyOp = new AsyncRequest<TrophySystem.GetTrophyRewardIconRequest>(request).ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                    if (request.Icon != null)
                    {
                        pendingIcons.Add(request.Icon);
                    }
                }
            });

            UniversalDataSystem.Schedule(getTrophyOp);

            OnScreenLog.Add("Getting Trophy Reward Icon");
        }

        public void ShowTrophyList()
        {
            TrophySystem.ShowTrophyListRequest request = new TrophySystem.ShowTrophyListRequest();

            request.UserId = GamePad.activeGamePad.loggedInUser.userId;

            var getTrophyOp = new AsyncRequest<TrophySystem.ShowTrophyListRequest>(request).ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                }
            });

            UniversalDataSystem.Schedule(getTrophyOp);

            OnScreenLog.Add("Show Trophy List");
        }

        private void OutputTrophyGameDetails(TrophySystem.TrophyGameDetails gameDetails)
        {
            OnScreenLog.Add("TrophyGameDetails");

            OnScreenLog.Add("   # Groups : " + gameDetails.NumGroups);
            OnScreenLog.Add("   # Trophies : " + gameDetails.NumTrophies);
            OnScreenLog.Add("   # Platinum : " + gameDetails.NumPlatinum);
            OnScreenLog.Add("   # Gold : " + gameDetails.NumGold);
            OnScreenLog.Add("   # Silver : " + gameDetails.NumSilver);
            OnScreenLog.Add("   # Bronze : " + gameDetails.NumBronze);
            OnScreenLog.Add("   Title : " + gameDetails.Title);

            OnScreenLog.AddNewLine();
        }

        private void OutputTrophyGameData(TrophySystem.TrophyGameData gameData)
        {
            OnScreenLog.Add("TrophyGameData");

            OnScreenLog.Add("   # UnlockedTrophies : " + gameData.UnlockedTrophies);
            OnScreenLog.Add("   # UnlockedPlatinum : " + gameData.UnlockedPlatinum);
            OnScreenLog.Add("   # UnlockedGold : " + gameData.UnlockedGold);
            OnScreenLog.Add("   # UnlockedSilver : " + gameData.UnlockedSilver);
            OnScreenLog.Add("   # UnlockedBronze : " + gameData.UnlockedBronze);
            OnScreenLog.Add("   # ProgressPercentage : " + gameData.ProgressPercentage);

            OnScreenLog.AddNewLine();
        }

        private void OutputTrophyGroupDetails(TrophySystem.TrophyGroupDetails groupDetails)
        {
            OnScreenLog.Add("TrophyGroupDetails");

            OnScreenLog.Add("   GroupId : " + groupDetails.GroupId);
            OnScreenLog.Add("   # Trophies : " + groupDetails.NumTrophies);
            OnScreenLog.Add("   # Platinum : " + groupDetails.NumPlatinum);
            OnScreenLog.Add("   # Gold : " + groupDetails.NumGold);
            OnScreenLog.Add("   # Silver : " + groupDetails.NumSilver);
            OnScreenLog.Add("   # Bronze : " + groupDetails.NumBronze);
            OnScreenLog.Add("   Title : " + groupDetails.Title);

            OnScreenLog.AddNewLine();
        }

        private void OutputTrophyGroupData(TrophySystem.TrophyGroupData groupData)
        {
            OnScreenLog.Add("TrophyGroupData");

            OnScreenLog.Add("   GroupId : " + groupData.GroupId);
            OnScreenLog.Add("   # UnlockedTrophies : " + groupData.UnlockedTrophies);
            OnScreenLog.Add("   # UnlockedPlatinum : " + groupData.UnlockedPlatinum);
            OnScreenLog.Add("   # UnlockedGold : " + groupData.UnlockedGold);
            OnScreenLog.Add("   # UnlockedSilver : " + groupData.UnlockedSilver);
            OnScreenLog.Add("   # UnlockedBronze : " + groupData.UnlockedBronze);
            OnScreenLog.Add("   # ProgressPercentage : " + groupData.ProgressPercentage);

            OnScreenLog.AddNewLine();
        }

        private void OutputTrophyDetails(TrophySystem.TrophyDetails trophyDetails)
        {
            OnScreenLog.Add("TrophyDetails");

            OnScreenLog.Add("   TrophyId : " + trophyDetails.TrophyId);
            OnScreenLog.Add("   TrophyGrade : " + trophyDetails.TrophyGrade);
            OnScreenLog.Add("   GroupId : " + trophyDetails.GroupId);
            OnScreenLog.Add("   Hidden : " + trophyDetails.Hidden);
            OnScreenLog.Add("   HasReward : " + trophyDetails.HasReward);
            OnScreenLog.Add("   Title : " + trophyDetails.Title);
            OnScreenLog.Add("   Description : " + trophyDetails.Description);
            OnScreenLog.Add("   Reward : " + trophyDetails.Reward);
            OnScreenLog.Add("   IsProgress : " + trophyDetails.IsProgress);

            if (trophyDetails.IsProgress)
            {
                OnScreenLog.Add("   TargetValue : " + trophyDetails.TargetValue);
            }

            OnScreenLog.AddNewLine();
        }

        private void OutputTrophyData(TrophySystem.TrophyData trophyData)
        {
            OnScreenLog.Add("TrophyData");

            OnScreenLog.Add("   TrophyId : " + trophyData.TrophyId);
            OnScreenLog.Add("   Unlocked : " + trophyData.Unlocked);
            OnScreenLog.Add("   TimeStamp : " + trophyData.TimeStamp);
            OnScreenLog.Add("   IsProgress : " + trophyData.IsProgress);

            if (trophyData.IsProgress)
            {
                OnScreenLog.Add("   ProgressValue : " + trophyData.ProgressValue);
            }

            OnScreenLog.AddNewLine();
        }

    }
#endif
}
