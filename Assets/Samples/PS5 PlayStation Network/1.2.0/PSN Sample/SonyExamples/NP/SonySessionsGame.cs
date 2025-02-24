
#if UNITY_PS5 || UNITY_PS4
using System;
using System.Collections.Generic;
using System.Text;
using Unity.PSN.PS5.Aysnc;
using Unity.PSN.PS5.Sessions;
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

        public void OnGameSessionUpdated(GameSession.Notification notificationData)
        {
            if (notificationData.Session != null)
            {
                if (notificationData.Member != null)
                {
                    OnScreenLog.Add("OnGameSessionUpdated : " + notificationData.NotificationType + " : " + notificationData.Session.SessionId + " : " + notificationData.Member.AccountId, Color.cyan);
                }
                else
                {
                    OnScreenLog.Add("OnGameSessionUpdated : " + notificationData.NotificationType + " : " + notificationData.Session.SessionId, Color.cyan);
                }

                if (notificationData.NotificationType != GameSessionNotifications.NotificationTypes.Deleted)
                {
                    GameSession.ParamTypes update = notificationData.SessionParamUpdates == GameSession.ParamTypes.NotSet ? GameSession.ParamTypes.All : notificationData.SessionParamUpdates;

                    RefreshGameSession(GamePad.activeGamePad.loggedInUser.userId, notificationData.Session, update, "Refresh session after OnSessionUpdated failed " + notificationData.NotificationType);
                }
            }
            else
            {
                OnScreenLog.AddError("OnGameSessionUpdated  " + notificationData.NotificationType + " : Session is null");
            }
        }

        public void RefreshGameSession(int currentUserId, GameSession session, GameSession.ParamTypes updateParams, string errorMsg)
        {
            var requestOp = GetGameSessionsRequestSimple(currentUserId, session, updateParams);

            requestOp.ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                    OutputSessionsData(antecedent.Request.SessionsData);

                    foreach( var session in antecedent.Request.SessionsData.Sessions)
                        SessionsManager.UpdateSession(session);

                    OutputGameSession(session);
                }
                else
                {
                    OnScreenLog.AddError(errorMsg);
                }
            });

            OnScreenLog.Add("Refreshing game session...");

            SessionsManager.Schedule(requestOp);
        }

        // ***************************************************************************
        // Menus
        // ***************************************************************************

        void DoGameSessionButtons()
        {
            // Test the current user and calculate if they have a session and what state is it in.
            if (GamePad.activeGamePad != null && GamePad.activeGamePad.loggedInUser.onlineStatus == PlatformInput.OnlineStatus.SignedIn)
            {
                int currentUserId = GamePad.activeGamePad.loggedInUser.userId;
                bool isUserRegistered = SessionsManager.IsUserRegistered(currentUserId);

                if (isUserRegistered == true)
                {
                    GameSession currentGS = SessionsManager.FindGameSessionFromUserId(currentUserId);

                    GameSession altGS = null;

                    if (currentGS == null)
                    {
                        var activeSessions = SessionsManager.ActiveGameSessions;

                        if (activeSessions != null && activeSessions.Count > 0)
                        {
                            altGS = activeSessions[0];
                        }
                    }

                    DoGameSessionButton(currentUserId);
                    DoGSJoinMatchedButton(currentUserId);
                    DoGSSearchButton(currentUserId, currentGS);
                    DoGSTouchButton(currentUserId, currentGS);

                    // If there is an alternate game session availble show the join button
                    DoGSJoinButtons(currentUserId, altGS);

                    DoGSLeaveButton(currentUserId, currentGS);
                    DoGSRefreshButton(currentUserId, currentGS);

                    DoGSUpdateSessionButtons(currentUserId, currentGS);

                    DoGSPatchSearchAttributeButtons(currentUserId, currentGS);

                    DoGSSetMemberCustomDataButton(currentUserId, currentGS);

                    DoGSSendMessage(currentUserId, currentGS);

                    DoGSFindButton(currentUserId);

                    DoGSDeleteButton(currentUserId, currentGS);

                }
            }

            if (m_MenuSessions.AddBackIndex("Back"))
            {
                currentMenu = MenuTypes.SessionSelection;
            }
        }

        void DoGSPatchSearchAttributeButtons(int currentUserId, GameSession currentGs)
        {
            if (currentGs == null)
            {
                return;
            }

            if (m_MenuSessions.AddItem("Patch Session Search Attributes", ""))
            {
                string sessionId = currentGs.SessionId;

                AsyncOp requestOp = PatchSearchParams(currentUserId, sessionId);

                OnScreenLog.AddNewLine();
                OnScreenLog.Add("Patching game session...");

                SessionsManager.Schedule(requestOp);
            }
        }

        void DoGameSessionButton(Int32 currentUserId)
        {
            if (m_MenuSessions.AddItem("Create Game Session", "Create a game session for the current user."))
            {
                GameSessionCreationParams sessionParams = new GameSessionCreationParams()
                {
                    MaxPlayers = 32,
                    MaxSpectators = 10,
                    SupportedPlatforms = SessionPlatforms.PS5 | SessionPlatforms.PS4,
                    JoinDisabled = false,
                    UsePlayerSession = true,
                    ReservationTimeoutSeconds = 400,
                    CustomData1 = MakeData(10, 100),
                    CustomData2 = MakeData(10, 101),

                    SearchIndex = "GameSessionSearchIndex",
                    Searchable = true,

                    Callbacks = new GameSessionCallbacks()
                    {
                        OnSessionUpdated = OnGameSessionUpdated,
                        WebApiNotificationCallback = RawSessionEventHandler
                    }

                };

                sessionParams.SearchAttributes = new SearchAttributesType();

                sessionParams.SearchAttributes.ints[0]=9;
                sessionParams.SearchAttributes.bools[0]=true;
                sessionParams.SearchAttributes.strings[0]="myfirststring";

                AsyncOp requestOp = CreateGameRequest(currentUserId, sessionParams);

                OnScreenLog.AddNewLine();
                OnScreenLog.Add("Creating game session...");

                SessionsManager.Schedule(requestOp);
            }
        }

        // The id of the most-recently-offered session.
        // This is set in SonySessions.cs in response to a
        // GameSessionNotifications.NotificationTypes.InvitationsCreated event.
        public static string s_matchedSessionId = null;
        void DoGSJoinMatchedButton (Int32 currentUserId)
        {
            // idToDisplay is a version of s_matchedSessionId that is suitable to display to the user.
            // (i.e. Protected against null and wrapped in quotes)
            string idToDisplay = string.IsNullOrEmpty(s_matchedSessionId) ? "(null)" : $"\"{s_matchedSessionId}\"";
            if (m_MenuSessions.AddItem("Join Game Session", $"Join game session from provided sessionId: {idToDisplay}."))
            {
                AsyncOp requestOp = JoinGameSessionRequest(currentUserId, s_matchedSessionId, false);
                SessionsManager.Schedule(requestOp);
            }
        }

        void DoGSLeaveButton(Int32 currentUserId, GameSession currentGS)
        {
            if (currentGS == null) return;

            if (m_MenuSessions.AddItem("Leave Game Session", "Leave the current GameSession.", currentGS != null))
            {
                AsyncOp requestOp = LeaveRequest(currentUserId, currentGS);

                OnScreenLog.Add("Leaving session...");

                SessionsManager.Schedule(requestOp);
            }
        }

        void DoGSJoinButtons(Int32 currentUserId, GameSession gs)
        {
            if (gs == null) return;

            if (m_MenuSessions.AddItem("Join as Player", "Join to session as player"))
            {
                AsyncOp requestOp = JoinGameSessionRequest(currentUserId, gs.SessionId, false);

                OnScreenLog.Add("Joining session as player... " + gs.SessionId);

                SessionsManager.Schedule(requestOp);
            }

            if (m_MenuSessions.AddItem("Join as Spectator", "Join to session as spectator"))
            {
                AsyncOp requestOp = JoinGameSessionRequest(currentUserId, gs.SessionId, true);

                OnScreenLog.Add("Joining session as spectator... " + gs.SessionId);

                SessionsManager.Schedule(requestOp);
            }

            int numGameSessions = SessionsManager.ActiveGameSessions != null ? SessionsManager.ActiveGameSessions.Count : 0;

            string helpText = $"Remove the unused session from the active list. Number of tracked game sessions = {numGameSessions}";

            if (m_MenuSessions.AddItem("Forget Session", helpText))
            {
                SessionsManager.RemoveGameSession(gs);

                numGameSessions = SessionsManager.ActiveGameSessions != null ? SessionsManager.ActiveGameSessions.Count : 0;

                OnScreenLog.Add("Session Removed : " + gs.SessionId);
                OnScreenLog.Add("New Session Count : " + numGameSessions);

            }
        }

        void DoGSRefreshButton(Int32 currentUserId, GameSession currentGS)
        {
            if (currentGS == null) return;

            if (m_MenuSessions.AddItem("Refresh Game Session", "Refresh the game session info. This will update the GameSession object with its latest state.", currentGS != null))
            {
                AsyncOp requestOp = GetGameSessionsRequest(currentUserId, currentGS);

                OnScreenLog.AddNewLine();
                OnScreenLog.Add("Refreshing game session...");

                SessionsManager.Schedule(requestOp);
            }
        }

        void DoGSUpdateSessionButtons(int currentUserId, GameSession currentGS)
        {
            if (currentGS == null) return;

            if (m_MenuSessions.AddItem("Increase Max Members", "Increase the number of maximum players and spectators. To see the changes use the refresh session button afterwards."))
            {
                AsyncOp requestOp = SetSessionProperties(currentUserId, currentGS, GameSession.ParamTypes.MaxPlayers);

                SessionsManager.Schedule(requestOp);

                requestOp = SetSessionProperties(currentUserId, currentGS, GameSession.ParamTypes.MaxSpectators);

                SessionsManager.Schedule(requestOp);

                OnScreenLog.AddNewLine();
                OnScreenLog.Add("Increasing the number of maximum players and spectators...");
            }

            if (m_MenuSessions.AddItem("Update CustomData1", "Update custom data "))
            {
                AsyncOp requestOp = SetSessionProperties(currentUserId, currentGS, GameSession.ParamTypes.CustomData1);

                SessionsManager.Schedule(requestOp);

                OnScreenLog.AddNewLine();
                OnScreenLog.Add("Changing custom data for the session...");
            }
        }

        void DoGSSetMemberCustomDataButton(int currentUserId, GameSession currentGS)
        {
            if (currentGS == null) return;

            if (m_MenuSessions.AddItem("Set Member System Properties", "Set member system properties in game session"))
            {
                AsyncOp requestOp = SetGameSessionMemberSystemProperties(currentUserId, currentGS);

                SessionsManager.Schedule(requestOp);

                OnScreenLog.AddNewLine();
                OnScreenLog.Add("Set member properties...");
            }
        }

        void DoGSSendMessage(int currentUserId, GameSession currentGS)
        {
            if (currentGS == null) return;

            if (m_MenuSessions.AddItem("Send Message", "Send message to all players in session"))
            {
                AsyncOp requestOp = SendGameSessionMessage(currentUserId, currentGS);

                SessionsManager.Schedule(requestOp);

                OnScreenLog.AddNewLine();
                OnScreenLog.Add("Sending message to all players...");
            }
        }

        void DoGSFindButton(int currentUserId)
        {
            if (m_MenuSessions.AddItem("Find Session By User", "Find game session by user"))
            {
                AsyncOp requestOp = GetJoinedGameSessions(currentUserId);

                SessionsManager.Schedule(requestOp);

                OnScreenLog.AddNewLine();
                OnScreenLog.Add("Searching game sessions...");
            }
        }

        void DoGSDeleteButton(int currentUserId, GameSession currentGS)
        {
            if (m_MenuSessions.AddItem("Delete Game Session", "Delete the game session"))
            {
                AsyncOp requestOp = DeleteGameSessions(currentUserId, currentGS);

                SessionsManager.Schedule(requestOp);

                OnScreenLog.AddNewLine();
                OnScreenLog.Add("Deleting game sessions...");
            }
        }

        void DoGSSearchButton(int currentUserId, GameSession currentGS)
        {
            if (m_MenuSessions.AddItem("Game Session Search", "Search for game sessions"))
            {
                AsyncOp requestOp = GameSessionsSearch(currentUserId);

                SessionsManager.Schedule(requestOp);

                OnScreenLog.AddNewLine();
                OnScreenLog.Add("Searching for game sessions...");
            }
        }

        void DoGSTouchButton(int currentUserId, GameSession currentGS)
        {
            if (currentGS == null)
            {
                return;
            }


            if (m_MenuSessions.AddItem("Touch Game Session", "Reset the amount of time for session excluded from search"))
            {
                AsyncOp requestOp = GameSessionTouch(currentUserId, currentGS);

                SessionsManager.Schedule(requestOp);

                OnScreenLog.AddNewLine();
                OnScreenLog.Add("Touch Game Session...");
            }
        }
        // ***************************************************************************
        // Make Requests
        // ***************************************************************************

        AsyncOp GameSessionTouch(int currentUserId, GameSession currentGS)
        {
            GameSessionRequests.GameSessionTouchRequest request = new GameSessionRequests.GameSessionTouchRequest()
            {
                UserId = currentUserId,
                SessionId = currentGS.SessionId
            };

            var requestOp = new AsyncRequest<GameSessionRequests.GameSessionTouchRequest>(request).ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                    OnScreenLog.Add("PostGameSessionTouchRequest success:");

                    OnScreenLog.Add($"Last Touched DateTime: {request.LastTouchedDateTime.ToString()}");
                }
            });

            return requestOp;
        }


        AsyncOp CreateGameRequest(Int32 currentUserId, GameSessionCreationParams sessionParams)
        {
            GameSessionRequests.CreateGameSessionRequest request = new GameSessionRequests.CreateGameSessionRequest()
            {
                UserId = currentUserId,
                CreatorsCustomData1 = MakeData(10, 50),
                CreationParams = sessionParams
            };

            var requestOp = new AsyncRequest<GameSessionRequests.CreateGameSessionRequest>(request).ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                    OnScreenLog.Add("CreateGameSessionRequest success:");

                    OutputGameSession(antecedent.Request.Session);
                }
            });

            return requestOp;
        }

        AsyncOp JoinGameSessionRequest(Int32 currentUserId, string sessionId, bool joinAsSpectator)
        {
            GameSessionRequests.JoinGameSessionRequest request = new GameSessionRequests.JoinGameSessionRequest()
            {
                UserId = currentUserId,
                SessionId = sessionId,
                JoinAsSpectator = joinAsSpectator,
                Callbacks = new GameSessionCallbacks()
                {
                    OnSessionUpdated = OnGameSessionUpdated,
                    WebApiNotificationCallback = RawSessionEventHandler
                }
            };

            var requestOp = new AsyncRequest<GameSessionRequests.JoinGameSessionRequest>(request).ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                    OnScreenLog.Add("Joined session");

                    //  OutputPlayerSession(antecedent.Request.Session);
                }
            });

            return requestOp;
        }

        AsyncOp LeaveRequest(Int32 currentUserId, GameSession currentPS)
        {
            GameSessionRequests.LeaveGameSessionRequest request = new GameSessionRequests.LeaveGameSessionRequest()
            {
                UserId = currentUserId,
                SessionId = currentPS.SessionId,
            };

            var requestOp = new AsyncRequest<GameSessionRequests.LeaveGameSessionRequest>(request).ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                    OnScreenLog.Add("LeaveGameSessionRequest done : " + antecedent.Request.SessionId);
                }
            });

            return requestOp;
        }

        AsyncOp GetGameSessionsRequest(Int32 currentUserId, GameSession currentGS)
        {
            GameSessionRequests.GetGameSessionsRequest request = new GameSessionRequests.GetGameSessionsRequest()
            {
                UserId = currentUserId,
                SessionIds = currentGS.SessionId,
                RequiredFields = GameSession.ParamTypes.All
            };

            var requestOp = new AsyncRequest<GameSessionRequests.GetGameSessionsRequest>(request).ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                    OnScreenLog.Add("Got game sessions");

                    OutputSessionsData(antecedent.Request.SessionsData);

                    foreach (var session in antecedent.Request.SessionsData.Sessions)
                        SessionsManager.UpdateSession(session);

                    OutputGameSession(currentGS);
                }
            });

            return requestOp;
        }

        AsyncOp PatchSearchParams(Int32 currentUserId, string gameSessionId)
        {
            var newAttributes = new SearchAttributesType();
            newAttributes.bools[1] = true;
            newAttributes.strings[0] = "overwritten string";
            //Don't update the int's array

            GameSessionRequests.PatchGameSessionSearchAttributesRequest request = new GameSessionRequests.PatchGameSessionSearchAttributesRequest()
            {
                UserId = currentUserId,
                SessionId = gameSessionId,
                SearchAttributes = newAttributes
            };

            var requestOp = new AsyncRequest<GameSessionRequests.PatchGameSessionSearchAttributesRequest>(request).ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                    OnScreenLog.Add("Did patch Session Search Attributes");
                }
            });

            return requestOp;
        }

        AsyncOp GetGameSessionsRequest(Int32 currentUserId, string gameSessionId)
        {
            GameSessionRequests.GetGameSessionsRequest request = new GameSessionRequests.GetGameSessionsRequest()
            {
                UserId = currentUserId,
                SessionIds = gameSessionId,
                RequiredFields = GameSession.ParamTypes.All
            };

            var requestOp = new AsyncRequest<GameSessionRequests.GetGameSessionsRequest>(request).ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                    OnScreenLog.Add("Got game sessions");

                    OutputSessionsData(antecedent.Request.SessionsData);
                }
            });

            return requestOp;
        }

        AsyncRequest<GameSessionRequests.GetGameSessionsRequest> GetGameSessionsRequestSimple(Int32 currentUserId, GameSession currentGS, GameSession.ParamTypes updateParams)
        {
            GameSessionRequests.GetGameSessionsRequest request = new GameSessionRequests.GetGameSessionsRequest()
            {
                UserId = currentUserId,
                SessionIds = currentGS.SessionId,
                RequiredFields = updateParams
            };

            var requestOp = new AsyncRequest<GameSessionRequests.GetGameSessionsRequest>(request);

            return requestOp;
        }

        AsyncOp SetGameSessionMemberSystemProperties(Int32 currentUserId, GameSession currentGS)
        {
            byte[] someData = MakeData(100, 0);

            GameSessionRequests.SetGameSessionMemberSystemPropertiesRequest request = new GameSessionRequests.SetGameSessionMemberSystemPropertiesRequest()
            {
                UserId = currentUserId,
                SessionId = currentGS.SessionId,
                CustomData1 = someData
            };

            var requestOp = new AsyncRequest<GameSessionRequests.SetGameSessionMemberSystemPropertiesRequest>(request).ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                    OnScreenLog.Add("Member properties set");
                }
            });

            return requestOp;
        }

        AsyncOp SendGameSessionMessage(Int32 currentUserId, GameSession currentPS)
        {
            List<SessionMemberIdentifier> recipients = new List<SessionMemberIdentifier>();
            List<SessionMember> players = currentPS.Players;

            OnScreenLog.Add("Send Message: Players # " + players.Count);

            for (int i = 0; i < players.Count; i++)
            {
                SessionMember player = players[i];
                if (player.UserId != currentUserId &&
                   player.AccountId != SessionMember.InvalidAccountId)
                {
                    SessionMemberIdentifier recipient = new SessionMemberIdentifier { accountId = player.AccountId, platform = player.Platform };
                    recipients.Add(recipient);
                    OnScreenLog.Add($"Sending to : {recipient.accountId} on {recipient.platform}.");
                }
            }

            GameSessionRequests.SendGameSessionMessageRequest request = new GameSessionRequests.SendGameSessionMessageRequest()
            {
                UserId = currentUserId,
                SessionId = currentPS.SessionId,
                Recipients = recipients,
                Payload = "This is a test message " + OnScreenLog.FrameCount,
            };

            var requestOp = new AsyncRequest<GameSessionRequests.SendGameSessionMessageRequest>(request).ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                    OnScreenLog.Add("Message sent");
                }
            });

            return requestOp;
        }

        AsyncOp GetJoinedGameSessions(Int32 currentUserId)
        {
            GameSessionRequests.GetJoinedGameSessionsByUserRequest request = new GameSessionRequests.GetJoinedGameSessionsByUserRequest()
            {
                UserId = currentUserId,
            };

            var requestOp = new AsyncRequest<GameSessionRequests.GetJoinedGameSessionsByUserRequest>(request).ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                    OnScreenLog.Add("Game sessions received");

                    OutputJoinedGameSession(antecedent.Request.FoundPlayerSessions, currentUserId);
                }
            });

            return requestOp;
        }

        AsyncOp DeleteGameSessions(Int32 currentUserId, GameSession currentGS)
        {
            GameSessionRequests.DeleteGameSessionRequest request = new GameSessionRequests.DeleteGameSessionRequest()
            {
                UserId = currentUserId,
                SessionId = currentGS.SessionId
            };

            var requestOp = new AsyncRequest<GameSessionRequests.DeleteGameSessionRequest>(request).ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                    OnScreenLog.Add("Game session deleted");
                }
            });

            return requestOp;
        }

        AsyncOp GameSessionsSearch(Int32 currentUserId)
        {
            GameSessionRequests.GameSessionsSearchRequest request = new GameSessionRequests.GameSessionsSearchRequest()
            {
                UserId = currentUserId,
                /*
                    Basic Usage of Game Session Search
                    https://game.develop.playstation.net/resources/documents/WebAPI/1/Session_Manager_WebAPI-Overview/0003.html#__document_toc_00000008
                    Specify searchIndex when creating a Game Session to make it searchable with Game Session Search. searchIndex cannot be modified after a Game Session has been created.
                    An array of up to 64 arbitrary alphanumeric characters can be specified for searchIndex; no prior configuration, requests, or other actions are required.
                    One title can use a maximum of 100 different searchIndex values. Once a searchIndex has been specified for a Game Session, a record of what was specified will remain.
                    Be careful not to make specifications without reason.
                */
                SearchIndex = "GameSessionSearchIndex",
            };

            request.Conditions.Add(new Condition(SearchAttribute.kMaxplayers, SearchOperator.kGreaterThan, "2") );
            request.Conditions.Add(new Condition(SearchAttribute.kSearchattributesInteger1, SearchOperator.kEqual, "9") ); // sample game session created with sessionParams.SearchAttributes.ints[0]=9;
//            request.Conditions.Add(new Condition() {Attribute=SearchAttribute.kSearchattributesInteger1, Operator=SearchOperator.kIn, Values=new List<string>{"1", "2", "3"} } );

            var requestOp = new AsyncRequest<GameSessionRequests.GameSessionsSearchRequest>(request).ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                    OnScreenLog.Add($"Search has found {antecedent.Request.FoundGameSessions.Count} sessions");
                    if (antecedent.Request.FoundGameSessions.Count>0)
                    {
                        s_matchedSessionId = antecedent.Request.FoundGameSessions[0];   // so that the join game session option works
                        foreach( var sessionid in antecedent.Request.FoundGameSessions)
                        {
                            OnScreenLog.Add($"sessionid:{sessionid}");
                        }
                    }
                }
            });

            return requestOp;
        }

        void OutputJoinedGameSession(List<GameSessionRequests.JoinedGameSession> joinedSessions, Int32 currentUserId)
        {
            OnScreenLog.Add("Joined Sessions... # " + joinedSessions.Count);

            string searchIDs = "";

            for (int i = 0; i < joinedSessions.Count; i++)
            {
                OnScreenLog.Add("      SessionId : " + joinedSessions[i].SessionId);
                OnScreenLog.Add("           Platform : " + joinedSessions[i].Platform);
                if (i > 0)
                {
                    searchIDs += ",";
                }

                searchIDs += joinedSessions[i].SessionId;
            }

            //Do search here
            if (joinedSessions.Count > 0)
            {
                AsyncOp requestOp = GetGameSessionsRequest(currentUserId, searchIDs);

                OnScreenLog.AddNewLine();
                OnScreenLog.Add("Searching game sessions...");

                SessionsManager.Schedule(requestOp);
            }
        }

        // ***************************************************************************
        // Output
        // ***************************************************************************

        void OutputGameSession(GameSession gameSession)
        {
            if (gameSession == null)
            {
                OnScreenLog.AddError("   OutputGameSession failed : playerSession is null");
                return;
            }

            Color textColour = new Color(1.0f, 0.0f, 1.0f);

            OnScreenLog.AddNewLine();

            OnScreenLog.Add("   Game Session Id : " + gameSession.SessionId, textColour);

            OnScreenLog.Add("   CreatedTimeStamp : " + gameSession.CreatedTimestamp, textColour);

            OnScreenLog.Add("   MaxPlayers : " + gameSession.MaxPlayers, textColour);
            OnScreenLog.Add("   MaxSpectators : " + gameSession.MaxSpectators, textColour);

            OnScreenLog.Add("   JoinDisabled : " + gameSession.JoinDisabled, textColour);
            OnScreenLog.Add("   SupportedPlatforms : " + gameSession.SupportedPlatforms, textColour);

            if (gameSession.Representative != null)
            {
                OnScreenLog.Add("   Representative : ", textColour);
                OnScreenLog.Add("        Account Id : " + gameSession.Representative.AccountId, textColour);
                OnScreenLog.Add("        OnlineId : " + gameSession.Representative.OnlineId, textColour);
                OnScreenLog.Add("        Platform : " + gameSession.Representative.Platform, textColour);
            }
            else
            {
                OnScreenLog.Add("   Representative : null", textColour);
            }

            OnScreenLog.Add("   UsePlayerSession : " + gameSession.UsePlayerSession, textColour);
            OnScreenLog.Add("   MatchmakingOfferId : " + gameSession.MatchmakingOfferId, textColour);
            OnScreenLog.Add("   ReservationTimeoutSeconds : " + gameSession.ReservationTimeoutSeconds, textColour);
            OnScreenLog.Add("   NatType : " + gameSession.NatType, textColour);
            OnScreenLog.Add("   Searchable : " + gameSession.Searchable, textColour);
            OnScreenLog.Add("   SearchIndex : " + gameSession.SearchIndex, textColour);

            if (gameSession.Players == null || gameSession.Players.Count == 0)
            {
                OnScreenLog.Add("   Players : None", textColour);
            }
            else
            {
                OnScreenLog.Add("   Players : ", textColour);

                for (int i = 0; i < gameSession.Players.Count; i++)
                {
                    SessionMember member = gameSession.Players[i];

                    if (member != null)
                    {
                        OnScreenLog.Add("        Account Id : " + member.AccountId, textColour);
                        OnScreenLog.Add("              UserId : " + member.UserId.ToString("X8"), textColour);
                        OnScreenLog.Add("              OnlineId : " + member.OnlineId, textColour);
                        OnScreenLog.Add("              Platform : " + member.Platform, textColour);
                        OnScreenLog.Add("              JoinTimestamp : " + member.JoinTimestamp, textColour);
                        OnScreenLog.Add("              IsSpectator : " + member.IsSpectator, textColour);
                        OnScreenLog.Add("              IsLocal : " + member.IsLocal, textColour);
                        OnScreenLog.Add("              JoinState : " + member.JoinState, textColour);

                        if (member.CustomData1 != null)
                        {
                            OnScreenLog.Add("              CustomData : " + OutputBinaryData(member.CustomData1, 10), textColour);
                        }
                    }
                    else
                    {
                        OnScreenLog.AddError("Member in Players list is null");
                    }
                }
            }

            if (gameSession.Spectators == null || gameSession.Spectators.Count == 0)
            {
                OnScreenLog.Add("   Spectators : None", textColour);
            }
            else
            {
                OnScreenLog.Add("   Spectators : ", textColour);

                for (int i = 0; i < gameSession.Spectators.Count; i++)
                {
                    SessionMember member = gameSession.Spectators[i];

                    if (member != null)
                    {
                        OnScreenLog.Add("        Account Id : " + member.AccountId, textColour);
                        OnScreenLog.Add("              UserId : " + member.UserId.ToString("X8"), textColour);
                        OnScreenLog.Add("              OnlineId : " + member.OnlineId, textColour);
                        OnScreenLog.Add("              Platform : " + member.Platform, textColour);
                        OnScreenLog.Add("              JoinTimestamp : " + member.JoinTimestamp, textColour);
                        OnScreenLog.Add("              IsSpectator : " + member.IsSpectator, textColour);
                        OnScreenLog.Add("              IsLocal : " + member.IsLocal, textColour);
                        OnScreenLog.Add("              JoinState : " + member.JoinState, textColour);

                        if (member.CustomData1 != null)
                        {
                            OnScreenLog.Add("              CustomData : " + OutputBinaryData(member.CustomData1, 10), textColour);
                        }
                    }
                    else
                    {
                        OnScreenLog.AddError("Member in Spectators list is null");
                    }
                }
            }

            if (gameSession.CustomData1 != null)
            {
                OnScreenLog.Add("   CustomData1 : " + OutputBinaryData(gameSession.CustomData1, 10), textColour);
            }

            if (gameSession.CustomData2 != null)
            {
                OnScreenLog.Add("   CustomData2 : " + OutputBinaryData(gameSession.CustomData2, 10), textColour);
            }

            if (gameSession.SearchAttributes != null)
            {
                if (gameSession.SearchAttributes.ints.setBits != 0 ||
                    gameSession.SearchAttributes.bools.setBits != 0 ||
                    gameSession.SearchAttributes.strings.setBits != 0)
                {
                    string output = "    Search Attributes\n";

                    output += "        Ints: ";

                    for (int i = 0; i < SearchAttributesType.kMaxAttributesPerType; i++)
                    {
                        if (gameSession.SearchAttributes.ints.IsSet(i))
                        {
                            output += ($"{i}: {gameSession.SearchAttributes.ints[i]} ");
                        }
                    }

                    output +=("\n        Strings: ");

                    for (int i = 0; i < SearchAttributesType.kMaxAttributesPerType; i++)
                    {
                        if (gameSession.SearchAttributes.strings.IsSet(i))
                        {
                            output += ($"{i}: {gameSession.SearchAttributes.strings[i]} ");
                        }
                    }

                    output +=("\n        Booleans: ");

                    for (int i = 0; i < SearchAttributesType.kMaxAttributesPerType; i++)
                    {
                        if (gameSession.SearchAttributes.bools.IsSet(i))
                        {
                            output += ($"{i}: {gameSession.SearchAttributes.bools[i]} ");
                        }
                    }

                    OnScreenLog.Add(output, textColour, true);
                }
            }

            OnScreenLog.Add("Game Session output complete...\n");
        }

        void OutputSessionsData(GameSessionRequests.RetrievedSessionsData retrievedSessionsData)
        {
            OnScreenLog.Add($"{retrievedSessionsData.Sessions.Count} game sessions returned");
            foreach (var session in retrievedSessionsData.Sessions)
                OutputSessionData(session);
        }


        void OutputSessionData(GameSessionRequests.RetrievedSessionData retrievedSessionData)
        {
            if (retrievedSessionData == null)
            {
                OnScreenLog.AddError("   OutputSessionData failed : retrievedSessionData is null");
                return;
            }

            OnScreenLog.Add("Retrieved Game Session Data:");

            OnScreenLog.Add("   Flags : " + retrievedSessionData.SetFlags.ToString());

            var flags = retrievedSessionData.SetFlags;

            // SessionId should always be set and will be done internally during the request.
            // If this isn't set the system won't know which session to update
            if (GameSession.IsParamFlagSet(flags, GameSession.ParamTypes.SessionId))
            {
                OnScreenLog.Add("   SessionId : " + retrievedSessionData.SessionId);
            }

            if (GameSession.IsParamFlagSet(flags, GameSession.ParamTypes.CreatedTimeStamp))
            {
                OnScreenLog.Add("   CreatedTimeStamp : " + retrievedSessionData.CreatedTimeStamp);
            }
            if (GameSession.IsParamFlagSet(flags, GameSession.ParamTypes.MaxPlayers))
            {
                OnScreenLog.Add("   MaxPlayers : " + retrievedSessionData.MaxPlayers);
            }
            if (GameSession.IsParamFlagSet(flags, GameSession.ParamTypes.MaxSpectators))
            {
                OnScreenLog.Add("   MaxSpectators : " + retrievedSessionData.MaxSpectators);
            }
            if (GameSession.IsParamFlagSet(flags, GameSession.ParamTypes.JoinDisabled))
            {
                OnScreenLog.Add("   JoinDisabled : " + retrievedSessionData.JoinDisabled);
            }
            if (GameSession.IsParamFlagSet(flags, GameSession.ParamTypes.SupportedPlatforms))
            {
                OnScreenLog.Add("   SupportedPlatforms : " + retrievedSessionData.SupportedPlatforms);
            }
            if (GameSession.IsParamFlagSet(flags, GameSession.ParamTypes.Representative))
            {
                if (retrievedSessionData.Representative != null)
                {
                    OnScreenLog.Add("   Representative : ");
                    OnScreenLog.Add("        Account Id : " + retrievedSessionData.Representative.AccountId);
                    OnScreenLog.Add("        OnlineId : " + retrievedSessionData.Representative.OnlineId);
                    OnScreenLog.Add("        Platform : " + retrievedSessionData.Representative.Platform);
                }
                else
                {
                    OnScreenLog.Add("   Representative : null");
                }
            }

            if (GameSession.IsParamFlagSet(flags, GameSession.ParamTypes.UsePlayerSession))
            {
                OnScreenLog.Add("   UsePlayerSession : " + retrievedSessionData.UsePlayerSession);
            }
            if (GameSession.IsParamFlagSet(flags, GameSession.ParamTypes.Matchmaking))
            {
                OnScreenLog.Add("   MatchmakingOfferId : " + retrievedSessionData.MatchmakingOfferId);
            }
            if (GameSession.IsParamFlagSet(flags, GameSession.ParamTypes.ReservationTimeoutSeconds))
            {
                OnScreenLog.Add("   ReservationTimeoutSeconds : " + retrievedSessionData.ReservationTimeoutSeconds);
            }
            if (GameSession.IsParamFlagSet(flags, GameSession.ParamTypes.Searchable))
            {
                OnScreenLog.Add("   Searchable : " + retrievedSessionData.Searchable);
            }

            if (GameSession.IsParamFlagSet(flags, GameSession.ParamTypes.CustomData1))
            {
                if (retrievedSessionData.CustomData1 != null && retrievedSessionData.CustomData1.Length > 0)
                {
                    int size = Math.Min(retrievedSessionData.CustomData1.Length, 10);

                    string output = "";
                    for (int i = 0; i < size; i++)
                    {
                        output += retrievedSessionData.CustomData1[i] + ", ";
                    }
                    OnScreenLog.Add("   CustomData1 : " + output);
                }
                else
                {
                    OnScreenLog.Add("   CustomData1 : None");
                }
            }

            if (GameSession.IsParamFlagSet(flags, GameSession.ParamTypes.CustomData2))
            {
                if (retrievedSessionData.CustomData2 != null && retrievedSessionData.CustomData2.Length > 0)
                {
                    int size = Math.Min(retrievedSessionData.CustomData2.Length, 10);

                    string output = "";
                    for (int i = 0; i < size; i++)
                    {
                        output += retrievedSessionData.CustomData2[i] + ", ";
                    }
                    OnScreenLog.Add("   CustomData2 : " + output);
                }
                else
                {
                    OnScreenLog.Add("   CustomData2 : None");
                }
            }

            if (GameSession.IsParamFlagSet(flags, GameSession.ParamTypes.MemberPlayers) || GameSession.IsParamFlagSet(flags, GameSession.ParamTypes.MemberPlayersCustomData1))
            {
                if (retrievedSessionData.Players != null && retrievedSessionData.Players.Length > 0)
                {
                    OnScreenLog.Add("   Players :");
                    for (int i = 0; i < retrievedSessionData.Players.Length; i++)
                    {
                        SessionMember member = retrievedSessionData.Players[i];

                        OnScreenLog.Add("      AccountId : " + member.AccountId);
                        OnScreenLog.Add("      OnlineId : " + member.OnlineId);
                        OnScreenLog.Add("      Platform : " + member.Platform);
                        OnScreenLog.Add("      JoinTimestamp : " + member.JoinTimestamp);
                        OnScreenLog.Add("      IsSpectator : " + member.IsSpectator);
                        OnScreenLog.Add("      JoinState : " + member.JoinState);

                        if (member.CustomData1 != null)
                        {
                            OnScreenLog.Add("      CustomData1 : Yes");
                        }
                    }
                }
                else
                {
                    OnScreenLog.Add("   Players : None");
                }
            }

            if (GameSession.IsParamFlagSet(flags, GameSession.ParamTypes.MemberSpectators) || GameSession.IsParamFlagSet(flags, GameSession.ParamTypes.MemberSpectatorsCustomData1))
            {
                if (retrievedSessionData.Spectators != null && retrievedSessionData.Spectators.Length > 0)
                {
                    OnScreenLog.Add("   Spectators :");
                    for (int i = 0; i < retrievedSessionData.Spectators.Length; i++)
                    {
                        SessionMember member = retrievedSessionData.Spectators[i];

                        OnScreenLog.Add("      AccountId : " + member.AccountId);
                        OnScreenLog.Add("      OnlineId : " + member.OnlineId);
                        OnScreenLog.Add("      Platform : " + member.Platform);
                        OnScreenLog.Add("      JoinTimestamp : " + member.JoinTimestamp);
                        OnScreenLog.Add("      IsSpectator : " + member.IsSpectator);
                        OnScreenLog.Add("      JoinState : " + member.JoinState);

                        if (member.CustomData1 != null)
                        {
                            OnScreenLog.Add("      CustomData1 : Yes");
                        }
                    }
                }
                else
                {
                    OnScreenLog.Add("   Spectators : None");
                }
            }
        }

        AsyncOp SetSessionProperties(Int32 currentUserId, GameSession currentGS, GameSession.ParamTypes paramToUpdate)
        {
            GameSessionRequests.SetGameSessionPropertiesRequest request = new GameSessionRequests.SetGameSessionPropertiesRequest()
            {
                UserId = currentUserId,
                SessionId = currentGS.SessionId,
                ParamToSet = paramToUpdate,
            };

            // ONLY ONE paramter can actually be applied at a time.

            if ((request.ParamToSet & GameSession.ParamTypes.MaxPlayers) != 0)
            {
                request.MaxPlayers = currentGS.MaxPlayers + 1;
            }

            if ((request.ParamToSet & GameSession.ParamTypes.MaxSpectators) != 0)
            {
                request.MaxSpectators = currentGS.MaxSpectators + 1;
            }

            if ((request.ParamToSet & GameSession.ParamTypes.JoinDisabled) != 0)
            {
                request.JoinDisabled = !currentGS.JoinDisabled;
            }

            if ((request.ParamToSet & GameSession.ParamTypes.CustomData1) != 0)
            {
                byte[] someData = MakeData(100, OnScreenLog.FrameCount);
                request.CustomData1 = someData;
            }

            if ((request.ParamToSet & GameSession.ParamTypes.CustomData2) != 0)
            {
                byte[] someData = MakeData(100, OnScreenLog.FrameCount);
                request.CustomData2 = someData;
            }

            var requestOp = new AsyncRequest<GameSessionRequests.SetGameSessionPropertiesRequest>(request).ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                    OnScreenLog.Add("Properties set : " + antecedent.Request.ParamToSet);
                }
            });

            return requestOp;
        }



    }
#endif
}
