using System;
using System.Collections.Generic;

#if UNITY_PS5 || UNITY_PS4
using Unity.PSN.PS5;
using Unity.PSN.PS5.Aysnc;
using Unity.PSN.PS5.Leaderboard;

#if UNITY_PS5
using Unity.PSN.PS5.Entitlement;
using Unity.PSN.PS5.PremiumFeatures;
#endif

#endif

namespace PSNSample
{
#if UNITY_PS5 || UNITY_PS4
    public class SonyLeaderboards : IScreen
    {
        MenuLayout m_MenuLeaderboards;

        public SonyLeaderboards()
        {
            Initialize();
        }

        public MenuLayout GetMenu()
        {
            return m_MenuLeaderboards;
        }

        public void OnEnter()
        {
        }

        public void OnExit()
        {
        }

        public void Process(MenuStack stack)
        {
            MenuLeaderboards(stack);
        }

        public void Initialize()
        {
            m_MenuLeaderboards = new MenuLayout(this, 450, 20);
        }

        public byte[] MakeData(int size, int startNumber)
        {
            byte[] someData = new byte[size];

            for (int i = 0; i < someData.Length; i++)
            {
                someData[i] = (byte)(startNumber + i);
            }

            return someData;
        }

        string BestObjectId = null;

        public void MenuLeaderboards(MenuStack menuStack)
        {
            m_MenuLeaderboards.Update();

            bool enabled = true;

            if (m_MenuLeaderboards.AddItem("Get Board Definition", "Get board definition", enabled))
            {
                Leaderboards.GetBoardDefinitionRequest request = new Leaderboards.GetBoardDefinitionRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                    BoardId = 1,
                    ServiceLabel = 0,
                };

                var requestOp = new AsyncRequest<Leaderboards.GetBoardDefinitionRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OutputBoardDefinition(antecedent.Request.Board);
                    }
                });

                OnScreenLog.Add("Getting board definition...");

                Leaderboards.Schedule(requestOp);
            }

            if (m_MenuLeaderboards.AddItem("Record Score", "Record a score on the board", enabled))
            {
                Int64 score = (Int64)UnityEngine.Random.Range(0, 1000);

                Leaderboards.RecordScoreRequest request = new Leaderboards.RecordScoreRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                    BoardId = 1,
                    Score = score,
                    NeedsTmpRank = true,
                    Comment = "Sample app score",
                    SmallData = MakeData(30, (byte)score),
                };

                var requestOp = new AsyncRequest<Leaderboards.RecordScoreRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("TmpRank : " + antecedent.Request.TmpRank);
                        OnScreenLog.Add("TmpSerialRank : " + antecedent.Request.TmpSerialRank);
                    }
                });

                OnScreenLog.Add("Recording score... " + score);

                Leaderboards.Schedule(requestOp);
            }

            if (m_MenuLeaderboards.AddItem("Record Score (No temp rank)", "Record a score on the board, and don't return a temp rank", enabled))
            {
                Int64 score = (Int64)UnityEngine.Random.Range(0, 1000);

                Leaderboards.RecordScoreRequest request = new Leaderboards.RecordScoreRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                    BoardId = 1,
                    Score = score,
                    NeedsTmpRank = false,
                    Comment = "Sample app score",
                    SmallData = MakeData(30, (byte)score),
                };

                var requestOp = new AsyncRequest<Leaderboards.RecordScoreRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Score recorded");
                        //OnScreenLog.Add("TmpRank : " + antecedent.Request.TmpRank);
                        //OnScreenLog.Add("TmpSerialRank : " + antecedent.Request.TmpSerialRank);
                    }
                });

                OnScreenLog.Add("Recording score... " + score);

                Leaderboards.Schedule(requestOp);
            }

            if (m_MenuLeaderboards.AddItem("Record Score (Large Data)", "Record a score on the board plus large data", enabled))
            {
                Int64 score = (Int64)UnityEngine.Random.Range(0, 1000);

                Leaderboards.RecordScoreRequest request = new Leaderboards.RecordScoreRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                    BoardId = 1,
                    Score = score,
                    NeedsTmpRank = true,
                    Comment = "Sample app score",
                    SmallData = MakeData(30, (byte)score),
                    LargeData = MakeData(1024 * 20, (byte)(score+1))   // 20 k
                };

                var requestOp = new AsyncRequest<Leaderboards.RecordScoreRequest>(request).ContinueWith((antecedent) =>
                {
                    if(antecedent.Request.Result.apiResult == APIResultTypes.Error)
                    {
                        if((uint)antecedent.Request.Result.sceErrorCode == (uint)0x8222F408)
                        {
                            OnScreenLog.AddWarning("Error SCE_NP_WEBAPI_SERVER_ERROR_LEADERBOARDS_LARGE_DATA_EXCEEDS_NUMBER_LIMIT (0x8222F408) has occured when updloading the data");
                            OnScreenLog.AddWarning("Score bound to the specified Large data exceeds the ranking range limit defined by the board configuration.");
                        }
                    }

                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("TmpRank : " + antecedent.Request.TmpRank);
                        OnScreenLog.Add("TmpSerialRank : " + antecedent.Request.TmpSerialRank);
                    }
                });

                OnScreenLog.Add("Recording score plus large data... " + score + " : " + OutputBinaryData(request.LargeData, 15));

                Leaderboards.Schedule(requestOp);
            }

            if (m_MenuLeaderboards.AddItem("Get Ranking", "Get the rankings on the board", enabled))
            {
                Leaderboards.GetRankingRequest request = new Leaderboards.GetRankingRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                    BoardId = 1,
                };

                var requestOp = new AsyncRequest<Leaderboards.GetRankingRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OutputRankings(antecedent.Request.Rankings);
                    }
                });

                OnScreenLog.Add("Getting rankings... ");

                Leaderboards.Schedule(requestOp);
            }

            if (m_MenuLeaderboards.AddItem("Get Ranking (Centered Around)", "Get the rankings on the board", enabled))
            {
                Leaderboards.GetRankingRequest request = new Leaderboards.GetRankingRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                    BoardId = 1,
                    UserCenteredAround = new Leaderboards.GetRankingRequest.UserInfo() { AccountId = GamePad.activeGamePad.loggedInUser.accountId},
                    CenterToEdgeLimit = 1 // Get one User either side of the players rank.
                };

                var requestOp = new AsyncRequest<Leaderboards.GetRankingRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OutputRankings(antecedent.Request.Rankings);
                    }
                });

                OnScreenLog.Add("Getting rankings... ");

                Leaderboards.Schedule(requestOp);
            }

            if (m_MenuLeaderboards.AddItem("Get Large Object", "Get the large object id", enabled && BestObjectId != null))
            {
                Leaderboards.GetLargeDataRequest request = new Leaderboards.GetLargeDataRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                    ObjectId = BestObjectId
                };

                var requestOp = new AsyncRequest<Leaderboards.GetLargeDataRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Large Data : " + antecedent.Request.ObjectId);

                        if(antecedent.Request.LargeData != null)
                        {
                            string output = OutputBinaryData(antecedent.Request.LargeData, 30);
                            OnScreenLog.Add("             LargeData : " + output);
                            OnScreenLog.Add("             Large Data Size : " + antecedent.Request.LargeData.Length);
                        }
                        else
                        {
                            OnScreenLog.Add("             LargeData : null");
                        }
                    }
                });

                OnScreenLog.Add("Getting rankings... ");

                Leaderboards.Schedule(requestOp);
            }

            if (m_MenuLeaderboards.AddBackIndex("Back"))
            {
                menuStack.PopMenu();
            }
        }

        private void OutputBoardDefinition(Leaderboards.BoardDefinition board)
        {
            OnScreenLog.Add("Board Definition : ");

            if (board == null)
            {
                OnScreenLog.Add("    No board");
            }
            else
            {
                OnScreenLog.Add("      Fields : " + board.Fields);
                OnScreenLog.Add("      EntryLimit : " + board.EntryLimit);
                OnScreenLog.Add("      LargeDataNumLimit : " + board.LargeDataNumLimit);
                OnScreenLog.Add("      LargeDataSizeLimit : " + board.LargeDataSizeLimit);
                OnScreenLog.Add("      MaxScoreLimit : " + board.MaxScoreLimit);
                OnScreenLog.Add("      MinScoreLimit : " + board.MinScoreLimit);
                OnScreenLog.Add("      SortMode : " + board.SortMode);
                OnScreenLog.Add("      UpdateMode : " + board.UpdateMode);
            }

        }

        string OutputBinaryData(byte[] data, int maxLength)
        {
            string output = "";

            int max = Math.Min(data.Length, maxLength);

            for (int i = 0; i < max; i++)
            {
                output += data[i] + " ";
            }

            return output;
        }

        private void OutputRankings(Leaderboards.Rankings rankings)
        {
            BestObjectId = null;

            OnScreenLog.Add("Rankings : ");

            if (rankings == null)
            {
                OnScreenLog.Add("    No Rankings");
            }
            else
            {
                int count = rankings.Entries != null ? rankings.Entries.Count : 0;

                OnScreenLog.Add("      Entries : " + count);

                for(int i = 0; i < count; i++)
                {
                    var entry = rankings.Entries[i];

                    OnScreenLog.Add("          AccountId : " + entry.AccountId);
                    OnScreenLog.Add("             PlayerCharacterID : " + entry.PlayerCharacterID);
                    OnScreenLog.Add("             SerialRank : " + entry.SerialRank);
                    OnScreenLog.Add("             HighestSerialRank : " + entry.HighestSerialRank);
                    OnScreenLog.Add("             Rank : " + entry.Rank);
                    OnScreenLog.Add("             HighestRank : " + entry.HighestRank);
                    OnScreenLog.Add("             Score : " + entry.Score);

                    if (entry.SmallData != null)
                    {
                        string smallOutput = OutputBinaryData(entry.SmallData, 20);
                        OnScreenLog.Add("             SmallData : " + smallOutput);
                    }

                    if (entry.ObjectId != null)
                    {
                        if(BestObjectId == null)
                        {
                            BestObjectId = entry.ObjectId;
                        }
                        OnScreenLog.Add("             ObjectId : " + entry.ObjectId);
                    }

                    if (entry.Comment != null)
                    {
                        OnScreenLog.Add("             Comment : " + entry.Comment);
                    }

                    OnScreenLog.Add("             OnlineId : " + entry.OnlineId);
                }

                OnScreenLog.Add("      LastUpdateDateTime : " + rankings.LastUpdateDateTime);
                OnScreenLog.Add("      TotalEntryCount : " + rankings.TotalEntryCount);
            }
        }
    }
#endif
}
