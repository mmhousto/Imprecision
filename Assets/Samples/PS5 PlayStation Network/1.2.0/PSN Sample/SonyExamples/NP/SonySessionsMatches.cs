
#if UNITY_PS5 || UNITY_PS4
using System;
using System.Collections.Generic;
using Unity.PSN.PS5.Aysnc;
using Unity.PSN.PS5.Dialogs;
using Unity.PSN.PS5.GameIntent;
using Unity.PSN.PS5.Matches;
using Unity.PSN.PS5.Sessions;
using Unity.PSN.PS5.Users;
using Unity.PSN.PS5.WebApi;
using UnityEngine;
#endif


#if UNITY_PS4
using PlatformInput = UnityEngine.PS4.PS4Input;
#elif UNITY_PS5
using PlatformInput = UnityEngine.PS5.PS5Input;
#endif


namespace PSNSample
{
#if UNITY_PS5 || UNITY_PS4
    public partial class SonySessions : IScreen
    {
        // ***************************************************************************
        // Notifications
        // ***************************************************************************


        // ***************************************************************************
        // Menus
        // ***************************************************************************

        void DoMatchesButtons()
        {
            // Test the current user and calculate if they have a session and what state is it in.
            if (GamePad.activeGamePad != null && GamePad.activeGamePad.loggedInUser.onlineStatus == PlatformInput.OnlineStatus.SignedIn)
            {
                int currentUserId = GamePad.activeGamePad.loggedInUser.userId;
                bool isUserRegistered = SessionsManager.IsUserRegistered(currentUserId);

                if (isUserRegistered == true)
                {
                    GameSession currentGS = SessionsManager.FindGameSessionFromUserId(currentUserId);

                    DoMatchButton(currentUserId, currentGS);

                    Match currentMatch = Match.FindMatchFromAccountId(GamePad.activeGamePad.loggedInUser.accountId);

                    DoRefreshButtons(currentUserId, currentMatch);

                    List<Match> allMatches = Match.MatchesList;
                    if (allMatches != null && allMatches.Count > 0)
                    {
                        DoJoinButtons(currentUserId, allMatches[0]);
                    }

                    DoScoreButton(currentUserId, currentMatch);

                    DoListMatchesButton();

                    DoRemoveButton(currentUserId, currentMatch);
                }
            }

            if (m_MenuSessions.AddBackIndex("Back"))
            {
                currentMenu = MenuTypes.SessionSelection;
            }
        }

        void DoMatchButton(Int32 currentUserId, GameSession gs)
        {
            if (m_MenuSessions.AddItem("Create Match", "Create a match from the current game session.", gs != null && gs.Players.Count > 0))
            {
                List<MatchPlayerCreateParams> players = new List<MatchPlayerCreateParams>();

                for (int i = 0; i < gs.Players.Count; i++)
                {
                    var gsPlayer = gs.Players[i];

                    MatchPlayerCreateParams newPlayer = new MatchPlayerCreateParams(i.ToString());

                    newPlayer.PlayerName = gsPlayer.OnlineId;
                    newPlayer.PlayerType = PlayerType.PSNPlayer;
                    newPlayer.AccountId = gsPlayer.AccountId;

                    players.Add(newPlayer);
                }

                MatchCreationParams matchParams = new MatchCreationParams()
                {
                    ActivityId = "MyPVPMatch",
                    Players = players
                };

                AsyncOp requestOp = CreateMatchRequest(currentUserId, matchParams);

                OnScreenLog.AddNewLine();
                OnScreenLog.Add("Creating match...");

                SessionsManager.Schedule(requestOp);
            }

            if (m_MenuSessions.AddItem("Create Team Match", "Create a team match from the current game session.", gs != null && gs.Players.Count > 0))
            {
                List<MatchPlayerCreateParams> players = new List<MatchPlayerCreateParams>();

                for (int i = 0; i < gs.Players.Count; i++)
                {
                    var gsPlayer = gs.Players[i];

                    MatchPlayerCreateParams newPlayer = new MatchPlayerCreateParams(i.ToString());

                    newPlayer.PlayerName = gsPlayer.OnlineId;
                    newPlayer.PlayerType = PlayerType.PSNPlayer;
                    newPlayer.AccountId = gsPlayer.AccountId;

                    players.Add(newPlayer);
                }

                List<MatchTeamCreateParams> teams = new List<MatchTeamCreateParams>();
                // Create two teams. Add half the players to each team.

                MatchTeamCreateParams teamA = new MatchTeamCreateParams("TeamA");
                MatchTeamCreateParams teamB = new MatchTeamCreateParams("TeamB");

                List<MatchPlayerCreateParams> teamAPlayers = new List<MatchPlayerCreateParams>();
                List<MatchPlayerCreateParams> teamBPlayers = new List<MatchPlayerCreateParams>();

                teamA.TeamMembers = teamAPlayers;
                teamB.TeamMembers = teamBPlayers;

                int firstCount = players.Count > 1 ? players.Count / 2 : 1;

                for (int i = 0; i < firstCount; i++)
                {
                    MatchPlayerCreateParams player = players[i];
                    teamAPlayers.Add(player);
                }

                for (int i = firstCount; i < players.Count; i++)
                {
                    MatchPlayerCreateParams player = players[i];
                    teamBPlayers.Add(player);
                }

                if (teamAPlayers.Count > 0) teams.Add(teamA);
                if (teamBPlayers.Count > 0) teams.Add(teamB);

                MatchCreationParams matchParams = new MatchCreationParams()
                {
                    ActivityId = "MyCompetitiveMatch",
                    Players = players,
                    Teams = teams
                };

                AsyncOp requestOp = CreateMatchRequest(currentUserId, matchParams);

                OnScreenLog.AddNewLine();
                OnScreenLog.Add("Creating match...");

                SessionsManager.Schedule(requestOp);
            }
        }

        void DoRefreshButtons(Int32 currentUserId, Match match)
        {
            string matchId = match != null ? match.MatchId : "null";

            if (m_MenuSessions.AddItem("Get Match Detail", $"Get the details for the current match. {matchId}", match != null))
            {
                AsyncOp requestOp = GetMatchDetailRequest(currentUserId, match);

                OnScreenLog.AddNewLine();
                OnScreenLog.Add("Get match details...");

                SessionsManager.Schedule(requestOp);
            }

            if (m_MenuSessions.AddItem("Update Match Status (Playing)", $"Update the match status to playing. {matchId}", match != null))
            {
                AsyncOp requestOp = UpdateMatchStatusRequest(currentUserId, match, MatchRequests.UpdateMatchStatus.Playing);

                OnScreenLog.AddNewLine();
                OnScreenLog.Add("Updating match status...");

                SessionsManager.Schedule(requestOp);
            }

            if (m_MenuSessions.AddItem("Update Match Status (OnHold)", $"Update the match status to OnHold. {matchId}. Tournament Matches cannot be On Hold", match != null && match.MatchType != MatchType.PSNTournament))
            {
                AsyncOp requestOp = UpdateMatchStatusRequest(currentUserId, match, MatchRequests.UpdateMatchStatus.OnHold);

                OnScreenLog.AddNewLine();
                OnScreenLog.Add("Updating match status...");

                SessionsManager.Schedule(requestOp);
            }

            if (m_MenuSessions.AddItem("Update Match Status (Cancelled)", $"Cancel the match. {matchId}", match != null))
            {
                AsyncOp requestOp = UpdateMatchStatusRequest(currentUserId, match, MatchRequests.UpdateMatchStatus.Cancelled);

                OnScreenLog.AddNewLine();
                OnScreenLog.Add("Updating match status...");

                SessionsManager.Schedule(requestOp);
            }

            if (m_MenuSessions.AddItem("Update Match Details (ExpirationTime)", $"Increase match experation time by 30 seconds. {matchId}", match != null && match.MatchType != MatchType.PSNTournament))
            {
                MatchRequests.UpdateMatchDetailRequest request = new MatchRequests.UpdateMatchDetailRequest()
                {
                    UserId = currentUserId,
                    MatchID = match.MatchId,
                    SetFlags = MatchRequests.UpdateMatchDetailRequest.ParamTypes.ExpirationTime,
                    ExpirationTime = match.ExpirationTime + 30
                };

                var requestOp = new AsyncRequest<MatchRequests.UpdateMatchDetailRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Updated Match");
                    }
                });

                OnScreenLog.AddNewLine();
                OnScreenLog.Add("Updating match ExpirationTime time...");

                SessionsManager.Schedule(requestOp);
            }

            if (m_MenuSessions.AddItem("Update Match Details (Team Names)", $"Change the team names. {matchId}", match != null && match.Teams != null && match.Teams.Count >= 2))
            {
                var Teams = match.Teams;

                var TeamA = Teams[0];
                var TeamB = Teams[1];

                TeamA.TeamName = "Team A (" + OnScreenLog.FrameCount + ")";
                TeamB.TeamName = "Team B (" + OnScreenLog.FrameCount + ")";

                MatchRequests.UpdateMatchDetailRequest request = new MatchRequests.UpdateMatchDetailRequest()
                {
                    UserId = currentUserId,
                    MatchID = match.MatchId,
                    SetFlags = MatchRequests.UpdateMatchDetailRequest.ParamTypes.Players | MatchRequests.UpdateMatchDetailRequest.ParamTypes.Teams,
                    Players = match.Players,
                    Teams = Teams
                };

                var requestOp = new AsyncRequest<MatchRequests.UpdateMatchDetailRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Updated Match");
                    }
                });

                OnScreenLog.AddNewLine();
                OnScreenLog.Add("Updating match team names...");

                SessionsManager.Schedule(requestOp);
            }

            if (m_MenuSessions.AddItem("Update Match Details (Interim Scores)", $"Update the match with interim results and stats {matchId}", match != null && match.CompetitionType == MatchCompetitionType.Competitive))
            {
                var results = GenerateRandomResults(match, match.CompetitionType, match.GroupType, match.ResultsType);

                var stats = GenerateRandomStats(match, match.CompetitionType, match.GroupType, match.ResultsType);

                MatchRequests.UpdateMatchDetailRequest request = new MatchRequests.UpdateMatchDetailRequest()
                {
                    UserId = currentUserId,
                    MatchID = match.MatchId,
                    SetFlags = MatchRequests.UpdateMatchDetailRequest.ParamTypes.Results | MatchRequests.UpdateMatchDetailRequest.ParamTypes.Stats,
                    GroupType = match.GroupType,
                    ResultType = match.ResultsType,
                    Results = results,
                    Stats = stats
                };

                var requestOp = new AsyncRequest<MatchRequests.UpdateMatchDetailRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Updated Match");
                    }
                });

                OnScreenLog.AddNewLine();
                OnScreenLog.Add("Updating match interim results and stats...");

                SessionsManager.Schedule(requestOp);
            }
        }

        void DoScoreButton(Int32 currentUserId, Match match)
        {
            string matchId = match != null ? match.MatchId : "null";

            if (m_MenuSessions.AddItem("Set Match Results", $"Set the match scores. {matchId}", match != null && match.Status > MatchStatus.Waiting))
            {
                AsyncOp requestOp = ReportResultsRequest(currentUserId, match);

                OnScreenLog.AddNewLine();
                OnScreenLog.Add("Set match results...");

                SessionsManager.Schedule(requestOp);
            }

#if UNITY_PS5
            if (m_MenuSessions.AddItem("Open Player Review Dialog", $"Open the player review dialog. {matchId}", match != null))
            {
                MatchesDialogSystem.OpenPlayerReviewDialogRequest request = new MatchesDialogSystem.OpenPlayerReviewDialogRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                    MatchId = match.MatchId,
                    Mode = MatchesDialogSystem.OpenPlayerReviewDialogRequest.ReviewModes.ReviewAllPlayers
                };

                var requestOp = new AsyncRequest<MatchesDialogSystem.OpenPlayerReviewDialogRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Dialog Status : " + antecedent.Request.Status);

                        OnScreenLog.Add("Dialog Closed...");
                    }
                });

                OnScreenLog.Add("Opening Dialog ...");

                DialogSystem.Schedule(requestOp);
            }
#endif

        }

        void DoJoinButtons(Int32 currentUserId, Match match)
        {
            string matchId = match != null ? match.MatchId : "null";

            int currentCount = match != null && match.Players != null ? match.Players.Count : 0;

            MatchPlayer player = null;
            UInt64 accountId = 0;

            if (GamePad.activeGamePad != null && match != null)
            {
                accountId = GamePad.activeGamePad.loggedInUser.accountId;
                player = match.FindPlayer(accountId);
            }

            if (m_MenuSessions.AddItem("Join Match", $"Join the match. {matchId}", match != null && currentCount > 0))
            {
                int index = currentCount;

                var requestOp = GetMatchJoinRequest(currentUserId, index.ToString(), accountId, match);

                OnScreenLog.AddNewLine();
                OnScreenLog.Add("Joining match...");

                SessionsManager.Schedule(requestOp);
            }

            if (m_MenuSessions.AddItem("Leave Match", $"Leave the match. {matchId}", match != null && player != null))
            {
                AsyncOp requestOp = CreateLeaveMatchRequest(currentUserId, player, match);

                OnScreenLog.AddNewLine();
                OnScreenLog.Add("Leaving match...");

                SessionsManager.Schedule(requestOp);
            }
        }

        static AsyncAction<AsyncRequest<MatchRequests.JoinMatchRequest>> GetMatchJoinRequest(int currentUserId, string playerID, ulong accountId, Match match)
        {
            MatchRequests.JoinPlayer join = new MatchRequests.JoinPlayer()
            {
                PlayerId = playerID,
                PlayerType = PlayerType.PSNPlayer,
                AccountId = accountId,
                ValidParams = MatchRequests.JoinPlayer.ParamTypes.AccountId
            };

            List<MatchRequests.JoinPlayer> players = new List<MatchRequests.JoinPlayer>();
            players.Add(join);

            MatchRequests.JoinMatchRequest request = new MatchRequests.JoinMatchRequest()
            {
                UserId = currentUserId,
                MatchID = match.MatchId,
                Players = players
            };

            var requestOp = new AsyncRequest<MatchRequests.JoinMatchRequest>(request).ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                    OnScreenLog.Add("Joined Match");
                }
            });
            return requestOp;
        }

        bool HasMatches()
        {
            return Match.MatchesList != null && Match.MatchesList.Count > 0;
        }

        void DoListMatchesButton()
        {
            if (m_MenuSessions.AddItem("List Matches", "List all the matches tracked internally", HasMatches()))
            {
                OnScreenLog.Add("Matches:");
                for (int i = 0; i < Match.MatchesList.Count; i++)
                {
                    OnScreenLog.Add("   Id : " + Match.MatchesList[i].MatchId);
                }
            }
        }

        void DoRemoveButton(Int32 currentUserId, Match match)
        {
            string matchId = match != null ? match.MatchId : "null";

            string hint = $"Remove all players from the match and remove the match from the active list '{matchId}'.";

            if (m_MenuSessions.AddItem("Remove Match", hint, match != null))
            {
                // Remove all players from the match.

                for(int i = 0; i < match.Players.Count; i++)
                {
                    AsyncOp requestOp = CreateLeaveMatchRequest(currentUserId, match.Players[i], match);

                    SessionsManager.Schedule(requestOp);
                }

                Match.RemoveMatch(match);
            }
        }


        // ***************************************************************************
        // Make Requests
        // ***************************************************************************

        AsyncOp CreateMatchRequest(Int32 currentUserId, MatchCreationParams matchParams)
        {
            MatchRequests.CreateMatchRequest request = new MatchRequests.CreateMatchRequest()
            {
                UserId = currentUserId,
                CreationParams = matchParams
            };

            var requestOp = new AsyncRequest<MatchRequests.CreateMatchRequest>(request).ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                    OnScreenLog.Add("CreateMatchRequest success:");

                    OutputMatch(antecedent.Request.Match);
                }
            });

            return requestOp;
        }

        AsyncOp CreateLeaveMatchRequest(Int32 currentUserId, MatchPlayer player, Match match)
        {
            MatchRequests.LeavePlayer join = new MatchRequests.LeavePlayer()
            {
                PlayerId = player.PlayerId,
                Reason = MatchRequests.LeavePlayer.Reasons.Quit
            };

            List<MatchRequests.LeavePlayer> players = new List<MatchRequests.LeavePlayer>();
            players.Add(join);

            MatchRequests.LeaveMatchRequest request = new MatchRequests.LeaveMatchRequest()
            {
                UserId = currentUserId,
                MatchID = match.MatchId,
                Players = players
            };

            var requestOp = new AsyncRequest<MatchRequests.LeaveMatchRequest>(request).ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                    OnScreenLog.Add("Left Match");
                }
            });

            return requestOp;
        }

        List<MatchPlayerResult> GenerateRandomPlayerResults(Match match, MatchResultsType resultsType, bool alwaysSetScore = true)
        {
            List<MatchPlayerResult> playerResults = new List<MatchPlayerResult>();

            int count = match.Players != null ? match.Players.Count : 0;

            for (int i = 0; i < count; i++)
            {
                var player = match.Players[i];

                MatchPlayerResult playerResult = new MatchPlayerResult();
                playerResult.PlayerId = player.PlayerId;
                playerResult.Rank = i + 1;

                if (alwaysSetScore || resultsType == MatchResultsType.Score)
                {
                    playerResult.Score = 100 - i;
                    playerResult.IsScoreSet = true;
                }

                playerResults.Add(playerResult);
            }

            return playerResults;
        }

        List<MatchTeamMemberResult> GenerateRandomMatchTeamMemberResults(MatchTeam team, MatchResultsType resultsType, out double teamTotalScore)
        {
            teamTotalScore = 0.0;

            List<MatchTeamMemberResult> teamMemberResults = new List<MatchTeamMemberResult>();

            int count = team.Members != null ? team.Members.Count : 0;

            for (int i = 0; i < count; i++)
            {
                var teamMember = team.Members[i];

                MatchTeamMemberResult memberResult = new MatchTeamMemberResult();

                memberResult.PlayerId = teamMember.PlayerId;
                memberResult.Score = 100 - i;

                teamMemberResults.Add(memberResult);

                teamTotalScore += memberResult.Score;
            }

            return teamMemberResults;
        }

        List<MatchTeamResult> GenerateRandomMatchTeamResults(Match match, MatchResultsType resultsType)
        {
            List<MatchTeamResult> teamResults = new List<MatchTeamResult>();

            int count = match.Teams != null ? match.Teams.Count : 0;

            for (int i = 0; i < count; i++)
            {
                var team = match.Teams[i];

                MatchTeamResult teamResult = new MatchTeamResult();

                teamResult.TeamId = team.TeamId;
                teamResult.Rank = i + 1;

                double teamTotalScore = 0.0;
                teamResult.TeamMemberResults = GenerateRandomMatchTeamMemberResults(team, resultsType, out teamTotalScore);

                if (resultsType == MatchResultsType.Score)
                {
                    teamResult.Score = teamTotalScore;
                    teamResult.IsScoreSet = true;
                }

                teamResults.Add(teamResult);
            }

            return teamResults;
        }

        MatchResults GenerateRandomResults(Match match, MatchCompetitionType competitionType, MatchGroupType groupType, MatchResultsType resultsType)
        {
            MatchResults newResults = new MatchResults();

            if (competitionType == MatchCompetitionType.Cooperative)
            {
                newResults.CooperativeResult = CooperativeResults.Success;
            }
            else if (competitionType == MatchCompetitionType.Competitive)
            {
                if (groupType == MatchGroupType.NonTeamMatch)
                {
                    newResults.PlayerResults = GenerateRandomPlayerResults(match, resultsType);
                }
                else if (groupType == MatchGroupType.TeamMatch)
                {
                    newResults.TeamResults = GenerateRandomMatchTeamResults(match, resultsType);
                }
            }

            return newResults;
        }

        List<MatchPlayerStats> GeneratePlayerStats(Match match, MatchResultsType resultsType)
        {
            List<MatchPlayerStats> playerStats = new List<MatchPlayerStats>();

            int count = match.Players != null ? match.Players.Count : 0;

            for (int i = 0; i < count; i++)
            {
                var player = match.Players[i];

                MatchPlayerStats playerStat = new MatchPlayerStats();
                playerStat.PlayerId = player.PlayerId;

                int kills = i * 2;
                int deaths = i;

                playerStat.Stats.StatsData.Add("Kills", kills.ToString());
                playerStat.Stats.StatsData.Add("Deaths", deaths.ToString());

                playerStats.Add(playerStat);
            }

            return playerStats;
        }

        List<MatchTeamMemberStats> GenerateRandomMatchTeamMemberStats(MatchTeam team)
        {
            List<MatchTeamMemberStats> teamMemberStats = new List<MatchTeamMemberStats>();

            int count = team.Members != null ? team.Members.Count : 0;

            for (int i = 0; i < count; i++)
            {
                var teamMember = team.Members[i];

                MatchTeamMemberStats memberstats = new MatchTeamMemberStats();

                memberstats.PlayerId = teamMember.PlayerId;

                int kills = i * 2;
                int deaths = i;

                memberstats.Stats.StatsData.Add("Kills", kills.ToString());
                memberstats.Stats.StatsData.Add("Deaths", deaths.ToString());

                teamMemberStats.Add(memberstats);
            }

            return teamMemberStats;
        }

        List<MatchTeamStats> GenerateTeamStats(Match match, MatchResultsType resultsType)
        {
            List<MatchTeamStats> teamStats = new List<MatchTeamStats>();

            int count = match.Teams != null ? match.Teams.Count : 0;

            for (int i = 0; i < count; i++)
            {
                var team = match.Teams[i];

                MatchTeamStats teamStat = new MatchTeamStats();

                teamStat.TeamId = team.TeamId;

                teamStat.Stats.StatsData.Add("TeamFlags", i.ToString());

                teamStat.TeamMemberStats = GenerateRandomMatchTeamMemberStats(team);

                teamStats.Add(teamStat);
            }

            return teamStats;
        }

        MatchStats GenerateRandomStats(Match match, MatchCompetitionType competitionType, MatchGroupType groupType, MatchResultsType resultsType)
        {
            MatchStats newStats = new MatchStats();

            if (competitionType == MatchCompetitionType.Cooperative || (competitionType == MatchCompetitionType.Competitive && groupType == MatchGroupType.NonTeamMatch))
            {
                newStats.PlayerStats = GeneratePlayerStats(match, resultsType);
            }
            else if (competitionType == MatchCompetitionType.Competitive && groupType == MatchGroupType.TeamMatch)
            {
                newStats.TeamStats = GenerateTeamStats(match, resultsType);
            }

            return newStats;
        }

        AsyncOp ReportResultsRequest(Int32 currentUserId, Match match)
        {
            MatchRequests.ReportResultsRequest request = null;

            request = new MatchRequests.ReportResultsRequest()
            {
                UserId = currentUserId,
                MatchID = match.MatchId,
                ReviewEligibility = MatchRequests.PlayerReviewEligibility.Enabled, // Not used for SDK 7.0 and above
                CompetitionType = match.CompetitionType,
                GroupType = match.GroupType,
                ResultType = match.ResultsType,
                Results = null,
                Stats = null
            };

            request.Results = GenerateRandomResults(match, request.CompetitionType, request.GroupType, request.ResultType);

            request.Stats = GenerateRandomStats(match, request.CompetitionType, request.GroupType, request.ResultType);

            var requestOp = new AsyncRequest<MatchRequests.ReportResultsRequest>(request).ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                    OnScreenLog.Add("Match results reported");
                }
            });

            return requestOp;
        }

        static AsyncOp GetMatchDetailRequest(Int32 currentUserId, Match match)
        {
            MatchRequests.GetMatchDetailRequest request = new MatchRequests.GetMatchDetailRequest()
            {
                UserId = currentUserId,
                MatchID = match.MatchId,
            };

            var requestOp = new AsyncRequest<MatchRequests.GetMatchDetailRequest>(request).ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                    OnScreenLog.Add("Got match detail");

                    OutputMatchDetail(antecedent.Request.MatchDetail);

                    match.UpdateDetails(antecedent.Request.MatchDetail);

                    OutputMatch(match);
                }
            });

            return requestOp;
        }

        AsyncOp UpdateMatchStatusRequest(Int32 currentUserId, Match match, MatchRequests.UpdateMatchStatus status)
        {
            MatchRequests.UpdateMatchStatusRequest request = new MatchRequests.UpdateMatchStatusRequest()
            {
                UserId = currentUserId,
                MatchID = match.MatchId,
                UpdateStatus = status
            };

            var requestOp = new AsyncRequest<MatchRequests.UpdateMatchStatusRequest>(request).ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                    OnScreenLog.Add("Updated Match to " + status);
                }
            });

            return requestOp;
        }

        // ***************************************************************************
        // Output
        // ***************************************************************************

        static void OutputMatchDetail(MatchRequests.RetrievedMatchDetail matchDetail)
        {
            if (matchDetail == null)
            {
                OnScreenLog.AddError("   OutputMatchDetail failed : matchDetail is null");
                return;
            }

            OnScreenLog.Add("Retrieved Match Detail:");

            OnScreenLog.Add("   MatchId : " + matchDetail.MatchId);
            OnScreenLog.Add("   MatchType : " + matchDetail.MatchType);
            OnScreenLog.Add("   Status : " + matchDetail.Status);
            OnScreenLog.Add("   ActivityId : " + matchDetail.ActivityId);
            OnScreenLog.Add("   GroupType : " + matchDetail.GroupType);
            OnScreenLog.Add("   CompetitionType : " + matchDetail.CompetitionType);
            OnScreenLog.Add("   ResultsType : " + matchDetail.ResultsType);
            OnScreenLog.Add("   ZoneId : " + matchDetail.ZoneId);
            OnScreenLog.Add("   ExpirationTime : " + matchDetail.ExpirationTime);
            OnScreenLog.Add("   StartTimeStamp : " + matchDetail.StartTimeStamp);
            OnScreenLog.Add("   EndTimeStamp : " + matchDetail.EndTimeStamp);
            OnScreenLog.Add("   PauseTimeStamp : " + matchDetail.LastPausedTimeStamp);

            int numPlayers = matchDetail.Players != null ? matchDetail.Players.Count : 0;
            OnScreenLog.Add("   Players : " + numPlayers);

            for (int i = 0; i < numPlayers; i++)
            {
                var player = matchDetail.Players[i];

                OnScreenLog.Add("       PlayerId : " + player.PlayerId);
                OnScreenLog.Add("          PlayerType : " + player.PlayerType);
                OnScreenLog.Add("          PlayerName : " + player.PlayerName);
                OnScreenLog.Add("          AccountId : " + player.AccountId);
                OnScreenLog.Add("          OnlineId : " + player.OnlineId);
                OnScreenLog.Add("          Joined : " + player.Joined);
            }

            int numTeams = matchDetail.Teams != null ? matchDetail.Teams.Count : 0;
            OnScreenLog.Add("   Teams : " + numTeams);

            for (int i = 0; i < numTeams; i++)
            {
                var team = matchDetail.Teams[i];

                OnScreenLog.Add("       TeamId : " + team.TeamId);
                OnScreenLog.Add("          TeamName : " + team.TeamName);

                int numMembers = team.Members != null ? team.Members.Count : 0;
                OnScreenLog.Add("          TeamMembers : " + numMembers);

                for (int j = 0; j < numMembers; j++)
                {
                    var member = team.Members[j];

                    OnScreenLog.Add("             PlayerId : " + member.PlayerId);
                    OnScreenLog.Add("                Joined : " + member.Joined);
                }
            }

            var results = matchDetail.Results;

            if (results != null)
            {
                OnScreenLog.Add("   Results : (Version) " + results.Version);
                OnScreenLog.Add("   CooperativeResult : " + results.CooperativeResult);

                OnScreenLog.Add("   CooperativeResult : " + results.CooperativeResult);

                if (results.PlayerResults != null)
                {
                    var playerResults = results.PlayerResults;

                    OnScreenLog.Add("   PlayerResults : " + playerResults.Count);

                    for (int i = 0; i < playerResults.Count; i++)
                    {
                        var playerResult = playerResults[i];

                        OnScreenLog.Add("       PlayerId : " + playerResult.PlayerId);
                        OnScreenLog.Add("          Rank : " + playerResult.Rank);
                        OnScreenLog.Add("          Score : " + playerResult.Score);
                    }
                }

                if (results.TeamResults != null)
                {
                    var teamResults = results.TeamResults;

                    OnScreenLog.Add("   TeamResults : " + teamResults.Count);

                    for (int i = 0; i < teamResults.Count; i++)
                    {
                        var teamResult = teamResults[i];

                        OnScreenLog.Add("       TeamId : " + teamResult.TeamId);
                        OnScreenLog.Add("          Rank : " + teamResult.Rank);
                        OnScreenLog.Add("          Score : " + teamResult.Score);

                        if (teamResult.TeamMemberResults != null)
                        {
                            var memberResults = teamResult.TeamMemberResults;
                            OnScreenLog.Add("          TeamMemberResults : " + memberResults.Count);

                            for (int j = 0; j < memberResults.Count; j++)
                            {
                                var memberResult = memberResults[j];

                                OnScreenLog.Add("          PlayerId : " + memberResult.PlayerId);
                                OnScreenLog.Add("             Score : " + memberResult.Score);
                            }
                        }
                    }
                }
            }
            else
            {
                OnScreenLog.Add("   Results : null");
            }

            var stats = matchDetail.Stats;

            if (stats != null)
            {
                OnScreenLog.Add("   Stats : ");

                if (stats.PlayerStats != null)
                {
                    OnScreenLog.Add("   PlayerStats : " + stats.PlayerStats.Count);

                    for (int i = 0; i < stats.PlayerStats.Count; i++)
                    {
                        var playerStats = stats.PlayerStats[i];
                        OnScreenLog.Add("     PlayerId : " + playerStats.PlayerId);

                        var addStats = playerStats.Stats.StatsData;

                        foreach (var pair in addStats)
                        {
                            OnScreenLog.Add("         Stat : " + pair.Key + " : " + pair.Value);
                        }
                    }
                }

                if (stats.TeamStats != null)
                {
                    OnScreenLog.Add("   TeamStats : " + stats.TeamStats.Count);

                    for (int i = 0; i < stats.TeamStats.Count; i++)
                    {
                        var teamStats = stats.TeamStats[i];
                        OnScreenLog.Add("      TeamId : " + teamStats.TeamId);

                        var teamAddStats = teamStats.Stats.StatsData;

                        foreach (var pair in teamAddStats)
                        {
                            OnScreenLog.Add("         Team Stat : " + pair.Key + " : " + pair.Value);
                        }

                        if (teamStats.TeamMemberStats != null)
                        {
                            OnScreenLog.Add("      TeamMemberStats : " + teamStats.TeamMemberStats.Count);

                            for (int j = 0; j < teamStats.TeamMemberStats.Count; j++)
                            {
                                var memberStats = teamStats.TeamMemberStats[j];

                                OnScreenLog.Add("         PlayerId : " + memberStats.PlayerId);

                                var addStats = teamStats.Stats.StatsData;

                                foreach (var pair in addStats)
                                {
                                    OnScreenLog.Add("            Stat : " + pair.Key + " : " + pair.Value);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                OnScreenLog.Add("   Stats : null");
            }
        }

        static void OutputMatch(Match match)
        {
            if (match == null)
            {
                OnScreenLog.AddError("   OutputMatch failed : match is null");
                return;
            }

            Color textColour = new Color(1.0f, 0.0f, 1.0f);

            OnScreenLog.AddNewLine();

            OnScreenLog.Add("   MatchId : " + match.MatchId, textColour);
            OnScreenLog.Add("   MatchType : " + match.MatchType, textColour);
            OnScreenLog.Add("   ActivityId : " + match.ActivityId, textColour);
            OnScreenLog.Add("   Status : " + match.Status, textColour);
            OnScreenLog.Add("   ZoneId : " + match.ZoneId, textColour);
            OnScreenLog.Add("   GroupType : " + match.GroupType, textColour);
            OnScreenLog.Add("   CompetitionType : " + match.CompetitionType, textColour);
            OnScreenLog.Add("   ResultsType : " + match.ResultsType, textColour);
            OnScreenLog.Add("   ExpirationTime : " + match.ExpirationTime, textColour);
            OnScreenLog.Add("   StartTimeStamp : " + match.StartTimeStamp, textColour);
            OnScreenLog.Add("   EndTimeStamp : " + match.EndTimeStamp, textColour);
            OnScreenLog.Add("   LastPausedTimeStamp : " + match.LastPausedTimeStamp, textColour);

            int numPlayers = match.Players != null ? match.Players.Count : 0;

            OnScreenLog.Add("   Players : " + numPlayers, textColour);

            for (int i = 0; i < numPlayers; i++)
            {
                MatchPlayer mp = match.Players[i];

                OnScreenLog.Add("      PlayerId : " + mp.PlayerId, textColour);
                OnScreenLog.Add("         PlayerName : " + mp.PlayerName, textColour);
                OnScreenLog.Add("         AccountId : " + mp.AccountId, textColour);
                OnScreenLog.Add("         PlayerType : " + mp.PlayerType, textColour);
                OnScreenLog.Add("         OnlineId : " + mp.OnlineId, textColour);
                OnScreenLog.Add("         Joined : " + mp.Joined, textColour);
            }

            int numTeams = match.Teams != null ? match.Teams.Count : 0;

            OnScreenLog.Add("   Teams : " + numTeams, textColour);

            for (int i = 0; i < numTeams; i++)
            {
                MatchTeam mt = match.Teams[i];

                OnScreenLog.Add("      TeamId : " + mt.TeamId, textColour);
                OnScreenLog.Add("         TeamName : " + mt.TeamName, textColour);

                int numMembers = mt.Members != null ? mt.Members.Count : 0;

                OnScreenLog.Add("         Members : " + numMembers, textColour);

                for (int j = 0; j < numMembers; j++)
                {
                    MatchTeamMember mp = mt.Members[j];

                    OnScreenLog.Add("            PlayerId : " + mp.PlayerId, textColour);
                    OnScreenLog.Add("            Joined : " + mp.Joined, textColour);
                }
            }

            var results = match.Results;

            if (results != null)
            {
                OnScreenLog.Add("   Results : (Version) " + results.Version);
                OnScreenLog.Add("   CooperativeResult : " + results.CooperativeResult);

                OnScreenLog.Add("   CooperativeResult : " + results.CooperativeResult);

                if (results.PlayerResults != null)
                {
                    var playerResults = results.PlayerResults;

                    OnScreenLog.Add("   PlayerResults : " + playerResults.Count);

                    for (int i = 0; i < playerResults.Count; i++)
                    {
                        var playerResult = playerResults[i];

                        OnScreenLog.Add("       PlayerId : " + playerResult.PlayerId);
                        OnScreenLog.Add("          Rank : " + playerResult.Rank);
                        OnScreenLog.Add("          Score : " + playerResult.Score);
                    }
                }

                if (results.TeamResults != null)
                {
                    var teamResults = results.TeamResults;

                    OnScreenLog.Add("   TeamResults : " + teamResults.Count);

                    for (int i = 0; i < teamResults.Count; i++)
                    {
                        var teamResult = teamResults[i];

                        OnScreenLog.Add("       TeamId : " + teamResult.TeamId);
                        OnScreenLog.Add("          Rank : " + teamResult.Rank);
                        OnScreenLog.Add("          Score : " + teamResult.Score);

                        if (teamResult.TeamMemberResults != null)
                        {
                            var memberResults = teamResult.TeamMemberResults;
                            OnScreenLog.Add("          TeamMemberResults : " + memberResults.Count);

                            for (int j = 0; j < memberResults.Count; j++)
                            {
                                var memberResult = memberResults[j];

                                OnScreenLog.Add("          PlayerId : " + memberResult.PlayerId);
                                OnScreenLog.Add("             Score : " + memberResult.Score);
                            }
                        }
                    }
                }
            }
            else
            {
                OnScreenLog.Add("   Results : null");
            }

            var stats = match.Stats;

            if (stats != null)
            {
                OnScreenLog.Add("   Stats : ");

                if (stats.PlayerStats != null)
                {
                    OnScreenLog.Add("   PlayerStats : " + stats.PlayerStats.Count);

                    for (int i = 0; i < stats.PlayerStats.Count; i++)
                    {
                        var playerStats = stats.PlayerStats[i];
                        OnScreenLog.Add("     PlayerId : " + playerStats.PlayerId);

                        var addStats = playerStats.Stats.StatsData;

                        foreach (var pair in addStats)
                        {
                            OnScreenLog.Add("         Stat : " + pair.Key + " : " + pair.Value);
                        }
                    }
                }

                if (stats.TeamStats != null)
                {
                    OnScreenLog.Add("   TeamStats : " + stats.TeamStats.Count);

                    for (int i = 0; i < stats.TeamStats.Count; i++)
                    {
                        var teamStats = stats.TeamStats[i];
                        OnScreenLog.Add("      TeamId : " + teamStats.TeamId);

                        var teamAddStats = teamStats.Stats.StatsData;

                        foreach (var pair in teamAddStats)
                        {
                            OnScreenLog.Add("         Team Stat : " + pair.Key + " : " + pair.Value);
                        }

                        if (teamStats.TeamMemberStats != null)
                        {
                            OnScreenLog.Add("      TeamMemberStats : " + teamStats.TeamMemberStats.Count);

                            for (int j = 0; j < teamStats.TeamMemberStats.Count; j++)
                            {
                                var memberStats = teamStats.TeamMemberStats[j];

                                OnScreenLog.Add("         PlayerId : " + memberStats.PlayerId);

                                var addStats = teamStats.Stats.StatsData;

                                foreach (var pair in addStats)
                                {
                                    OnScreenLog.Add("            Stat : " + pair.Key + " : " + pair.Value);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                OnScreenLog.Add("   Stats : null");
            }
        }


    }
#endif
        }
