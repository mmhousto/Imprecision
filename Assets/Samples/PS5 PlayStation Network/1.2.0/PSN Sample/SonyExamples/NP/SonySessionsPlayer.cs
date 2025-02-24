
#if UNITY_PS5 || UNITY_PS4
using System;
using System.Collections.Generic;
using Unity.PSN.PS5.Aysnc;
using Unity.PSN.PS5.GameIntent;
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
    public partial class  SonySessions : IScreen
    {
        // ***************************************************************************
        // Notifications
        // ***************************************************************************

        private void OnGameIntentNotification(GameIntentSystem.GameIntent gameIntent)
        {
            OnScreenLog.Add("Player Session - GameIntent");

            if (gameIntent.IntentType == GameIntentSystem.GameIntent.IntentTypes.JoinSession)
            {
                OnScreenLog.Add("Player Session - GameIntent - JoinSession");
                if (gameIntent is GameIntentSystem.JoinSession)
                {
                    GameIntentSystem.JoinSession joinSession = gameIntent as GameIntentSystem.JoinSession;

                    OnScreenLog.AddNewLine();
                    OnScreenLog.Add("User " + joinSession.UserId.ToString("X8") + " : Joining session invite " + joinSession.PlayerSessionId);

                    AsyncOp requestOp = JoinRequest(joinSession.UserId, joinSession.PlayerSessionId, joinSession.MemberType == GameIntentSystem.JoinSession.MemberTypes.Spectator);

                    SessionsManager.Schedule(requestOp);
                }
            }
        }

        public void OnPlayerSessionUpdated(PlayerSession.Notification notificationData)
        {
            if (notificationData.Session != null)
            {
                if (notificationData.Member != null)
                {
                    OnScreenLog.Add("OnPlayerSessionUpdated : " + notificationData.NotificationType + " : " + notificationData.Session.SessionId + " : " + notificationData.SessionParamUpdates + " : " + notificationData.Member.AccountId, Color.cyan);
                }
                else
                {
                    OnScreenLog.Add("OnPlayerSessionUpdated : " + notificationData.NotificationType + " : " + notificationData.Session.SessionId + " : " + notificationData.SessionParamUpdates, Color.cyan);
                }

                if (notificationData.NotificationType != PlayerSessionNotifications.NotificationTypes.Deleted)
                {
                    PlayerSession.ParamTypes update = notificationData.SessionParamUpdates == PlayerSession.ParamTypes.NotSet ? PlayerSession.ParamTypes.All : notificationData.SessionParamUpdates;

                    RefreshPlayerSession(GamePad.activeGamePad.loggedInUser.userId, notificationData.Session, update, "Refresh session after OnSessionUpdated failed " + notificationData.NotificationType);
                }
            }
            else
            {
                OnScreenLog.AddError("OnPlayerSessionUpdated  " + notificationData.NotificationType + " : Session is null");
            }
        }

        public void RefreshPlayerSession(int currentUserId, PlayerSession session, PlayerSession.ParamTypes updateParams, string errorMsg)
        {
            var requestOp = GetPlayerSessionsRequestSimple(currentUserId, session, updateParams);

            requestOp.ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                    OutputSessionsData(antecedent.Request.SessionsData);

                    foreach( var session in antecedent.Request.SessionsData.Sessions)
                        SessionsManager.UpdateSession(session);

                    OutputPlayerSession(session);
                }
                else
                {
                    OnScreenLog.AddError(errorMsg);
                }
            });

            OnScreenLog.Add("Refreshing player session...");

            SessionsManager.Schedule(requestOp);
        }

        public void SetupGameIntentCallback()
        {
            GameIntentSystem.OnGameIntentNotification += OnGameIntentNotification;
        }

        // ***************************************************************************
        // Menus
        // ***************************************************************************

        void DoPlayerSessionButtons()
        {
            // Test the current user and calculate if they have a session and what state is it in.
            if (GamePad.activeGamePad != null && GamePad.activeGamePad.loggedInUser.onlineStatus == PlatformInput.OnlineStatus.SignedIn)
            {
                int currentUserId = GamePad.activeGamePad.loggedInUser.userId;
                bool isUserRegistered = SessionsManager.IsUserRegistered(currentUserId);

                if (isUserRegistered == true)
                {
                    // Do they have a session already?
                    // If not is there a player session created by another user.
                    PlayerSession currentPS = SessionsManager.FindPlayerSessionFromUserId(currentUserId);

                    PlayerSession altPS = null;

                    if (currentPS == null)
                    {
                        var activeSessions = SessionsManager.ActivePlayerSessions;

                        if (activeSessions != null && activeSessions.Count > 0)
                        {
                            altPS = activeSessions[0];
                        }
                    }

                    // If the player doesn't have a session show the create button.
                    // If they do have a session then display a different create button.
                    // This will allow testing on what happens if a user tries to create a session when they are already a member of another session
                    DoPSCreateButton(currentUserId, currentPS != null);

                    // If there is an alternate player session availble show the join button
                    DoPSJoinButtons(currentUserId, altPS);

                    // If there is a current session do the follow buttons
                    DoPSLeaveButtons(currentUserId, currentPS);
                    DoPSRefreshButton(currentUserId, currentPS);
                    DoPSGetPlayerSessionsButton(currentUserId, currentPS);
                    DoPSSendInvitationButtons(currentUserId, currentPS);

                    DoPSGetInvitationButtons(currentUserId);

                    DoPSUpdateSessionButtons(currentUserId, currentPS);

                    DoPSSwapButtons(currentUserId, currentPS);

                    DoPSJoinableButtons(currentUserId, currentPS);

                    DoPSSetMemberCustomDataButton(currentUserId, currentPS);

                    DoPSSendMessage(currentUserId, currentPS);

                    DoPSFindButton(currentUserId);

                    DoPSFindFriendButton(currentUserId);
                }
            }

            if (m_MenuSessions.AddBackIndex("Back"))
            {
                currentMenu = MenuTypes.SessionSelection;
            }
        }
        void DoPSCreateButton(Int32 currentUserId, bool alreadyInASession = false)
        {
            string buttonText = "Create Player Session";
            string helpText = "Create a new Player session.";

            if (alreadyInASession == true)
            {
                buttonText = "Create another Player Session";
                helpText = "Creating another Player session will force the player to leave their current session";
            }

            if (m_MenuSessions.AddItem(buttonText, helpText))
            {
                LocalisedSessionNames sessionNames = new LocalisedSessionNames()
                {
                    DefaultLocale = "en-US",
                    LocalisedNames = new List<LocalisedText>()
                    {
                        new LocalisedText() { Locale = "en-US", Text = "Unity Session Name" },
                        new LocalisedText() { Locale = "ja-JP", Text = "Japanese のセッション名" },
                    }
                };

                PlayerSessionCreationParams sessionParams = new PlayerSessionCreationParams()
                {
                    MaxPlayers = 16,
                    MaxSpectators = 5,
                    SwapSupported = false,
                    NonPsnSupported = false,
                    //  JoinableUserType = JoinableUserTypes.SpecifiedUsers,
                    JoinableUserType = JoinableUserTypes.Anyone,
                    InvitableUserType = InvitableUserTypes.Member,
                    SupportedPlatforms = SessionPlatforms.PS5 | SessionPlatforms.PS4,
                    LocalisedNames = sessionNames,
                    CustomData1 = MakeData(10, 100),
                    CustomData2 = MakeData(10, 101),
                    //LeaderPrivileges = LeaderPrivilegeFlags.Kick,
                    //ExclusiveLeaderPrivileges = LeaderPrivilegeFlags.UpdateJoinableUserType | LeaderPrivilegeFlags.UpdateInvitableUerType,
                    //DisableSystemUiMenu = LeaderPrivilegeFlags.PromoteToLeader | LeaderPrivilegeFlags.Kick,
                    Callbacks = new PlayerSessionCallbacks()
                    {
                        OnSessionUpdated = OnPlayerSessionUpdated,
                        WebApiNotificationCallback = RawSessionEventHandler
                    }
                };



                try
                {
                    AsyncOp requestOp = CreateRequest(currentUserId, sessionParams);
                    OnScreenLog.AddNewLine();
                    OnScreenLog.Add("Creating session...");
                    SessionsManager.Schedule(requestOp);
                }
                catch(Unity.PSN.PS5.PSNException e)
                {
                    OnScreenLog.AddError("CreateRequest PSNException : " + e.Message);
                    OnScreenLog.AddError(e.StackTrace);
                }

            }
        }

        void DoPSJoinButtons(Int32 currentUserId, PlayerSession ps)
        {
            if (ps == null) return;

            if (m_MenuSessions.AddItem("Join as Player", "Join to session as player"))
            {
                AsyncOp requestOp = JoinRequest(currentUserId, ps.SessionId, false);

                OnScreenLog.Add("Joining session as player... " + ps.SessionId);

                SessionsManager.Schedule(requestOp);
            }

            if (m_MenuSessions.AddItem("Join as Spectator", "Join to session as spectator"))
            {
                AsyncOp requestOp = JoinRequest(currentUserId, ps.SessionId, true);

                OnScreenLog.Add("Joining session as spectator... " + ps.SessionId);

                SessionsManager.Schedule(requestOp);
            }

            int numPlayerSessions = SessionsManager.ActivePlayerSessions != null ? SessionsManager.ActivePlayerSessions.Count : 0;

            string helpText = $"Remove the unused session from the active list. Number of tracked player sessions = {numPlayerSessions}";

            if (m_MenuSessions.AddItem("Forget Session", helpText))
            {
                SessionsManager.RemovePlayerSession(ps);

                numPlayerSessions = SessionsManager.ActivePlayerSessions != null ? SessionsManager.ActivePlayerSessions.Count : 0;

                OnScreenLog.Add("Session Removed : " + ps.SessionId);
                OnScreenLog.Add("New Session Count : " + numPlayerSessions);

            }
        }

        void DoPSLeaveButtons(Int32 currentUserId, PlayerSession currentPS)
        {
            if (currentPS == null) return;

            foreach (var player in currentPS.Players)
            {
                DoPSLeaveButton(currentUserId, player, currentPS);
            }

            foreach (var spectator in currentPS.Spectators)
            {
                DoPSLeaveButton(currentUserId, spectator, currentPS);
            }
        }

        void DoPSLeaveButton(int currentUserId, SessionMember member, PlayerSession currentPS)
        {
            if (m_MenuSessions.AddItem($"Leave Player Session {member.OnlineId}", $"Leave the current PlayerSession. {member.AccountId}"))
            {
                AsyncOp requestOp = LeaveRequest(currentUserId, member.AccountId, currentPS);

                OnScreenLog.Add("Leaving session...");

                SessionsManager.Schedule(requestOp);
            }
        }

        void DoPSRefreshButton(Int32 currentUserId, PlayerSession currentPS)
        {
            if (currentPS == null) return;

            if (m_MenuSessions.AddItem("Refresh Player Session", "Refresh the players session info. This will update the PlayerSession object with its latest state.", currentPS != null))
            {
                AsyncOp requestOp = GetPlayerSessionsRequest(currentUserId, currentPS);

                OnScreenLog.AddNewLine();
                OnScreenLog.Add("Refreshing player session...");

                SessionsManager.Schedule(requestOp);
            }
        }

        void DoPSGetPlayerSessionsButton(Int32 currentUserId, PlayerSession currentPS)
        {
            if (currentPS == null) return;

            if (m_MenuSessions.AddItem("Get Player Sessions", "Get multiple players session info.", currentPS != null))
            {
                var request = new PlayerSessionRequests.GetPlayerSessionsRequest()
                {
                    UserId = currentUserId,
                    SessionIds = $"{currentPS.SessionId},567f7110-7d37-4fd1-a8ff-5940667e9d8a",   // comma separated session id's
                    RequiredFields = PlayerSession.ParamTypes.All
                };
                var requestOp = new AsyncRequest<PlayerSessionRequests.GetPlayerSessionsRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Got player sessions");
                        OutputSessionsData(antecedent.Request.SessionsData);
                    }
                });

                OnScreenLog.AddNewLine();
                OnScreenLog.Add("Getting multiple player session...");

                SessionsManager.Schedule(requestOp);


            }
        }

        void DoPSSendInvitationButtons(Int32 currentUserId, PlayerSession currentPS)
        {
            if (currentPS == null) return;

            if (m_MenuSessions.AddItem("Send Invitiations", "Send invitiations to friends"))
            {
                AsyncOp requestOp = SendInvitationsRequest(currentUserId, currentPS);

                OnScreenLog.AddNewLine();
                OnScreenLog.Add("Sending invitations to friends...");

                SessionsManager.Schedule(requestOp);
            }
        }

        void DoPSSwapButtons(Int32 currentUserId, PlayerSession currentPS)
        {
            if (currentPS == null) return;

            // Check to see if user is a player or spectator
            SessionMember member = currentPS.FindFromUserId(currentUserId);

            if (member == null)
            {
                return;
            }

            if (member.IsSpectator == true)
            {
                if (m_MenuSessions.AddItem("Swap to Player", "Swap to player"))
                {
                    AsyncOp requestOp = SwapRequest(currentUserId, currentPS.SessionId, false);

                    OnScreenLog.AddNewLine();
                    OnScreenLog.Add("Swapping to player...");

                    SessionsManager.Schedule(requestOp);
                }
            }

            if (member.IsSpectator == false)
            {
                if (m_MenuSessions.AddItem("Swap to Spectator", "Swap to spectator"))
                {
                    AsyncOp requestOp = SwapRequest(currentUserId, currentPS.SessionId, true);

                    OnScreenLog.AddNewLine();
                    OnScreenLog.Add("Swapping to spectator...");

                    SessionsManager.Schedule(requestOp);
                }
            }
        }

        void DoPSGetInvitationButtons(Int32 currentUserId)
        {
            if (m_MenuSessions.AddItem("Get Invitations", "Get invitations for a session"))
            {
                AsyncOp requestOp = GetInvitationsRequest(currentUserId);

                OnScreenLog.AddNewLine();
                OnScreenLog.Add("Getting invitations for user...");

                SessionsManager.Schedule(requestOp);
            }
        }

        void DoPSUpdateSessionButtons(int currentUserId, PlayerSession currentPS)
        {
            if (currentPS == null) return;

            if (m_MenuSessions.AddItem("Increase Max Members", "Increase the number of maximum players and spectators. To see the changes use the refresh session button afterwards."))
            {
                AsyncOp requestOp = SetSessionProperties(currentUserId, currentPS, PlayerSession.ParamTypes.MaxPlayers);

                SessionsManager.Schedule(requestOp);

                requestOp = SetSessionProperties(currentUserId, currentPS, PlayerSession.ParamTypes.MaxSpectators);

                SessionsManager.Schedule(requestOp);

                OnScreenLog.AddNewLine();
                OnScreenLog.Add("Increasing the number of maximum players and spectators...");
            }

            if (m_MenuSessions.AddItem("Update Localisation Names", "Change the set of localisation names for the session. To see the changes use the refresh session button afterwards."))
            {
                AsyncOp requestOp = SetSessionProperties(currentUserId, currentPS, PlayerSession.ParamTypes.LocalizedSessionName);

                SessionsManager.Schedule(requestOp);

                OnScreenLog.AddNewLine();
                OnScreenLog.Add("Changing the set of localisation names for the session...");
            }

            if (m_MenuSessions.AddItem("Update CustomData1", "Update custom data "))
            {
                AsyncOp requestOp = SetSessionProperties(currentUserId, currentPS, PlayerSession.ParamTypes.CustomData1);

                SessionsManager.Schedule(requestOp);

                OnScreenLog.AddNewLine();
                OnScreenLog.Add("Changing the custom data for the session...");
            }

            if (m_MenuSessions.AddItem("Change SwapSupported", "Change swap allowed to either enable or display swapping"))
            {
                AsyncOp requestOp = SetSessionProperties(currentUserId, currentPS, PlayerSession.ParamTypes.SwapSupported);

                SessionsManager.Schedule(requestOp);

                OnScreenLog.AddNewLine();
                OnScreenLog.Add("Changing the SwapSupported flag for the session...");
            }

            //bool isLeader = false;
            //var member = currentPs.FindFromUserId(currentUserId);
            //if(member != null && member.IsLeader == true)
            //{
            //    isLeader = true;
            //}

            UInt64 newleaderAccountId = GetFirstNonLeaderAccountId(currentUserId, currentPS);

            if (m_MenuSessions.AddItem("Change Leader", "Change player session leader", newleaderAccountId != SessionMember.InvalidAccountId)) // && isLeader == true))
            {
                AsyncOp requestOp = ChangeLeader(currentUserId, newleaderAccountId, currentPS);

                SessionsManager.Schedule(requestOp);

                OnScreenLog.AddNewLine();
                OnScreenLog.Add("Changing player session leader..." + newleaderAccountId);
            }
        }

        void DoPSJoinableButtons(int currentUserId, PlayerSession currentPS)
        {
            if (currentPS == null) return;

            if (m_MenuSessions.AddItem("Add Specified Users", "Add joinable specified users to player session"))
            {
                AsyncOp requestOp = AddSepecifiedUsers(currentUserId, currentPS);

                SessionsManager.Schedule(requestOp);

                OnScreenLog.AddNewLine();
                OnScreenLog.Add("Add current user and friends as specified Users...");
            }

            if (m_MenuSessions.AddItem("Delete Specified Users", "Delete all joinable specified users from player session"))
            {
                AsyncOp requestOp = DeleteSepecifiedUsers(currentUserId, currentPS);

                SessionsManager.Schedule(requestOp);

                OnScreenLog.AddNewLine();
                OnScreenLog.Add("Deleting all specified Users...");
            }
        }

        void DoPSSetMemberCustomDataButton(int currentUserId, PlayerSession currentPS)
        {
            if (currentPS == null) return;

            if (m_MenuSessions.AddItem("Set Member System Properties", "Set member system properties in player session"))
            {
                AsyncOp requestOp = SetPlayerSessionMemberSystemProperties(currentUserId, currentPS);

                SessionsManager.Schedule(requestOp);

                OnScreenLog.AddNewLine();
                OnScreenLog.Add("Set member properties...");
            }
        }

        void DoPSSendMessage(int currentUserId, PlayerSession currentPS)
        {
            if (currentPS == null) return;

            if (m_MenuSessions.AddItem("Send Message", "Send message to all players in session"))
            {
                AsyncOp requestOp = SendPlayerSessionMessage(currentUserId, currentPS);

                SessionsManager.Schedule(requestOp);

                OnScreenLog.AddNewLine();
                OnScreenLog.Add("Sending message to all players...");
            }
        }

        void DoPSFindButton(int currentUserId)
        {
            if (m_MenuSessions.AddItem("Find Session By User", "Find player session by user"))
            {
                AsyncOp requestOp = GetJoinedPlayerSessions(currentUserId);

                SessionsManager.Schedule(requestOp);

                OnScreenLog.AddNewLine();
                OnScreenLog.Add("Searching player sessions...");
            }
        }

        void DoPSFindFriendButton(int currentUserId)
        {
            if (m_MenuSessions.AddItem("Find Friends Sessions", "Find Sessions of your friends"))
            {
                AsyncOp requestOp = GetFriendPlayerSessions(currentUserId);

                SessionsManager.Schedule(requestOp);

                OnScreenLog.AddNewLine();
                OnScreenLog.Add("Finding friends Player Sessions... ");
            }
        }

        // ***************************************************************************
        // Make Requests
        // ***************************************************************************

        AsyncRequest<PlayerSessionRequests.GetPlayerSessionsRequest> GetPlayerSessionsRequestSimple(Int32 currentUserId, PlayerSession currentPS, PlayerSession.ParamTypes updateParams)
        {
            PlayerSessionRequests.GetPlayerSessionsRequest request = new PlayerSessionRequests.GetPlayerSessionsRequest()
            {
                UserId = currentUserId,
                SessionIds = currentPS.SessionId,
                RequiredFields = updateParams
            };

            var requestOp = new AsyncRequest<PlayerSessionRequests.GetPlayerSessionsRequest>(request);

            return requestOp;
        }

        AsyncOp CreateRequest(Int32 currentUserId, PlayerSessionCreationParams sessionParams)
        {
            PlayerSessionRequests.CreatePlayerSessionRequest request = new PlayerSessionRequests.CreatePlayerSessionRequest()
            {
                UserId = currentUserId,
                CreatorsCustomData1 = MakeData(10, 50),
                CreationParams = sessionParams
            };

            var requestOp = new AsyncRequest<PlayerSessionRequests.CreatePlayerSessionRequest>(request).ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                    OnScreenLog.Add("CreatePlayerSessionRequest success:");

                    OutputPlayerSession(antecedent.Request.Session);
                }
            });

            return requestOp;
        }

        AsyncOp JoinRequest(Int32 currentUserId, string sessionId, bool joinAsSpectator)
        {
            PlayerSessionRequests.JoinPlayerSessionRequest request = new PlayerSessionRequests.JoinPlayerSessionRequest()
            {
                UserId = currentUserId,
                SessionId = sessionId,
                JoinAsSpectator = joinAsSpectator,
                Callbacks = new PlayerSessionCallbacks()
                {
                    OnSessionUpdated = OnPlayerSessionUpdated,
                    WebApiNotificationCallback = RawSessionEventHandler
                }
            };

            var requestOp = new AsyncRequest<PlayerSessionRequests.JoinPlayerSessionRequest>(request).ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                    OnScreenLog.Add("Joined session");

                    //  OutputPlayerSession(antecedent.Request.Session);
                }
            });

            return requestOp;
        }

        AsyncOp SwapRequest(Int32 currentUserId, string sessionId, bool joinAsSpectator)
        {
            PlayerSessionRequests.SwapPlayerSessionMemberRequest request = new PlayerSessionRequests.SwapPlayerSessionMemberRequest()
            {
                UserId = currentUserId,
                SessionId = sessionId,
                JoinAsSpectator = joinAsSpectator,
            };

            var requestOp = new AsyncRequest<PlayerSessionRequests.SwapPlayerSessionMemberRequest>(request).ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                    OnScreenLog.Add("Swapped");

                    //  OutputPlayerSession(antecedent.Request.Session);
                }
            });

            return requestOp;
        }

        AsyncOp LeaveRequest(Int32 currentUserId, UInt64 accountIdToLeave, PlayerSession currentPS)
        {
            PlayerSessionRequests.LeavePlayerSessionRequest request = new PlayerSessionRequests.LeavePlayerSessionRequest()
            {
                UserId = currentUserId,
                AccountIDToLeave = accountIdToLeave,
                SessionId = currentPS.SessionId,
            };

            var requestOp = new AsyncRequest<PlayerSessionRequests.LeavePlayerSessionRequest>(request).ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                    OnScreenLog.Add("LeavePlayerSessionRequest done : " + antecedent.Request.SessionId);
                }
            });

            return requestOp;
        }

        AsyncOp GetPlayerSessionsRequest(Int32 currentUserId, PlayerSession currentPS)
        {
            PlayerSessionRequests.GetPlayerSessionsRequest request = new PlayerSessionRequests.GetPlayerSessionsRequest()
            {
                UserId = currentUserId,
                SessionIds = currentPS.SessionId,
                RequiredFields = PlayerSession.ParamTypes.All
            };

            var requestOp = new AsyncRequest<PlayerSessionRequests.GetPlayerSessionsRequest>(request).ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                    OnScreenLog.Add("Got player sessions");

                    OutputSessionsData(antecedent.Request.SessionsData);

                    foreach( var session in antecedent.Request.SessionsData.Sessions)
                        SessionsManager.UpdateSession(session);

                    OutputPlayerSession(currentPS);
                }
            });

            return requestOp;
        }

        AsyncRequest<UserSystem.GetFriendsRequest> GetFriendsRequest(Int32 currentUserId)
        {
            UserSystem.GetFriendsRequest request = new UserSystem.GetFriendsRequest()
            {
                UserId = GamePad.activeGamePad.loggedInUser.userId,
                Offset = 0,
                Limit = 95,
                Filter = UserSystem.GetFriendsRequest.Filters.NotSet,
                SortOrder = UserSystem.GetFriendsRequest.Order.OnlineId
            };

            var requestOp = new AsyncRequest<UserSystem.GetFriendsRequest>(request);

            return requestOp;
        }

        AsyncOp SendInvitationsRequest(Int32 currentUserId, PlayerSession currentPS)
        {
            var friendRequestOp = GetFriendsRequest(currentUserId);

            PlayerSessionRequests.SendPlayerSessionInvitationsRequest inviteRequest = new PlayerSessionRequests.SendPlayerSessionInvitationsRequest()
            {
                UserId = currentUserId,
                SessionId = currentPS.SessionId,
                AccountIds = friendRequestOp.Request.RetrievedAccountIds,
            };

            var inviteRequestOp = new AsyncRequest<PlayerSessionRequests.SendPlayerSessionInvitationsRequest>(inviteRequest).ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                    OnScreenLog.Add("Invitations sent and invitation ids returned");

                    if (antecedent.Request.InvitationIds != null)
                    {
                        for (int i = 0; i < antecedent.Request.InvitationIds.Count; i++)
                        {
                            OnScreenLog.Add("     Invitation Id : " + antecedent.Request.InvitationIds[i]);
                        }
                    }
                }
            });

            friendRequestOp.ContinueWith(inviteRequestOp);

            return friendRequestOp;
        }

        AsyncOp GetInvitationsRequest(Int32 currentUserId)
        {
            PlayerSessionRequests.GetPlayerSessionInvitationsRequest request = new PlayerSessionRequests.GetPlayerSessionInvitationsRequest()
            {
                UserId = currentUserId,
                RequiredFields = PlayerSessionRequests.RetrievedInvitation.ParamTypes.Default,
                Filter = PlayerSessionRequests.GetPlayerSessionInvitationsRequest.RetrievalFilters.ValidOnly
            };

            var requestOp = new AsyncRequest<PlayerSessionRequests.GetPlayerSessionInvitationsRequest>(request).ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                    OnScreenLog.Add("Got invitations");

                    OutputRetrievedInvitations(antecedent.Request.Invitations);
                }

            });


            return requestOp;
        }

        AsyncOp SetSessionProperties(Int32 currentUserId, PlayerSession currentPS, PlayerSession.ParamTypes paramToUpdate)
        {
            PlayerSessionRequests.SetPlayerSessionPropertiesRequest request = new PlayerSessionRequests.SetPlayerSessionPropertiesRequest()
            {
                UserId = currentUserId,
                SessionId = currentPS.SessionId,
                ParamToSet = paramToUpdate,
            };

            // ONLY ONE paramter can actually be applied at a time.

            if ((request.ParamToSet & PlayerSession.ParamTypes.MaxPlayers) != 0)
            {
                request.MaxPlayers = currentPS.MaxPlayers + 1;
            }

            if ((request.ParamToSet & PlayerSession.ParamTypes.MaxSpectators) != 0)
            {
                request.MaxSpectators = currentPS.MaxSpectators + 1;
            }

            if ((request.ParamToSet & PlayerSession.ParamTypes.JoinDisabled) != 0)
            {
                request.JoinDisabled = !currentPS.JoinDisabled;
            }

            if ((request.ParamToSet & PlayerSession.ParamTypes.JoinableUserType) != 0)
            {
                var nextJoinableUserType = currentPS.JoinableUserType == JoinableUserTypes.NoOne ? JoinableUserTypes.Friends : JoinableUserTypes.NoOne;
                request.JoinableUserType = nextJoinableUserType;
            }

            if ((request.ParamToSet & PlayerSession.ParamTypes.InvitableUserType) != 0)
            {
                var nextInvitableUserType = currentPS.InvitableUserType == InvitableUserTypes.Leader ? InvitableUserTypes.NoOne : InvitableUserTypes.Leader;
                request.InvitableUserType = nextInvitableUserType;
            }

            if ((request.ParamToSet & PlayerSession.ParamTypes.LocalizedSessionName) != 0)
            {
                LocalisedSessionNames sessionNames = new LocalisedSessionNames()
                {
                    DefaultLocale = "en-GB",
                    LocalisedNames = new List<LocalisedText>()
                        {
                            new LocalisedText() { Locale = "ja-JP", Text = "Japanese New Unity のセッション名" },
                            new LocalisedText() { Locale = "en-GB", Text = "Unity New Session Name (UK)" },
                            new LocalisedText() { Locale = "fr-FR", Text = "Unity New Nom de la session" },
                        }
                };

                request.LocalisedNames = sessionNames;
            }

            if ((request.ParamToSet & PlayerSession.ParamTypes.LeaderPrivileges) != 0)
            {
                request.LeaderPrivileges = LeaderPrivilegeFlags.Kick;
            }

            if ((request.ParamToSet & PlayerSession.ParamTypes.ExclusiveLeaderPrivileges) != 0)
            {
                request.ExclusiveLeaderPrivileges = LeaderPrivilegeFlags.Kick;
            }

            if ((request.ParamToSet & PlayerSession.ParamTypes.DisableSystemUiMenu) != 0)
            {
                request.DisableSystemUiMenu = LeaderPrivilegeFlags.PromoteToLeader;
            }

            if ((request.ParamToSet & PlayerSession.ParamTypes.CustomData1) != 0)
            {
                byte[] someData = MakeData(100, OnScreenLog.FrameCount);
                request.CustomData1 = someData;
            }

            if ((request.ParamToSet & PlayerSession.ParamTypes.CustomData2) != 0)
            {
                byte[] someData = MakeData(100, OnScreenLog.FrameCount);
                request.CustomData2 = someData;
            }

            if ((request.ParamToSet & PlayerSession.ParamTypes.SwapSupported) != 0)
            {
                request.SwapSupported = !currentPS.SwapSupported;
            }

            var requestOp = new AsyncRequest<PlayerSessionRequests.SetPlayerSessionPropertiesRequest>(request).ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                    OnScreenLog.Add("Properties set : " + antecedent.Request.ParamToSet);
                }
            });

            return requestOp;
        }

        AsyncOp ChangeLeader(Int32 currentUserId, UInt64 newLeader, PlayerSession currentPS)
        {
            PlayerSessionRequests.ChangePlayerSessionLeaderRequest request = new PlayerSessionRequests.ChangePlayerSessionLeaderRequest()
            {
                UserId = currentUserId,
                SessionId = currentPS.SessionId,
                AccountId = newLeader,
            };

            var requestOp = new AsyncRequest<PlayerSessionRequests.ChangePlayerSessionLeaderRequest>(request).ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                    OnScreenLog.Add("Leader set");
                }
            });

            return requestOp;
        }

        AsyncOp AddSepecifiedUsers(Int32 currentUserId, PlayerSession currentPS)
        {
            // New copy of the local account ids. This can be changed
            List<UInt64> localAccountIds = GetAllLocalAcountIds();

            var friendRequestOp = GetFriendsRequest(currentUserId);

            friendRequestOp.ContinueWith((friendsResults) =>
            {
                // NOT a COPY of the friends list. Don't modify this
                List<UInt64> friendsAccountIds = friendsResults.Request.RetrievedAccountIds;

                // Add the two lists together
                localAccountIds.AddRange(friendsAccountIds);

                PlayerSessionRequests.AddPlayerSessionJoinableSpecifiedUsersRequest request = new PlayerSessionRequests.AddPlayerSessionJoinableSpecifiedUsersRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                    SessionId = currentPS.SessionId,
                    JoinableAccountIds = friendRequestOp.Request.RetrievedAccountIds,
                };

                var requestOp = new AsyncRequest<PlayerSessionRequests.AddPlayerSessionJoinableSpecifiedUsersRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Specified users added");

                        OutputJoinableUsers(antecedent.Request.RetrievedAccountIds);
                    }
                });

                SessionsManager.Schedule(requestOp);
            });

            return friendRequestOp;
        }

        AsyncOp DeleteSepecifiedUsers(Int32 currentUserId, PlayerSession currentPS)
        {
            PlayerSessionRequests.DeletePlayerSessionJoinableSpecifiedUsersRequest request = new PlayerSessionRequests.DeletePlayerSessionJoinableSpecifiedUsersRequest()
            {
                UserId = currentUserId,
                SessionId = currentPS.SessionId,
                JoinableAccountIds = new List<UInt64>(currentPS.JoinableSpecifiedUsers)
            };

            var requestOp = new AsyncRequest<PlayerSessionRequests.DeletePlayerSessionJoinableSpecifiedUsersRequest>(request).ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                    OnScreenLog.Add("Specified users deleted");
                }
            });

            return requestOp;
        }

        AsyncOp SetPlayerSessionMemberSystemProperties(Int32 currentUserId, PlayerSession currentPS)
        {
            byte[] someData = MakeData(100, 0);

            PlayerSessionRequests.SetPlayerSessionMemberSystemPropertiesRequest request = new PlayerSessionRequests.SetPlayerSessionMemberSystemPropertiesRequest()
            {
                UserId = currentUserId,
                SessionId = currentPS.SessionId,
                CustomData1 = someData
            };

            var requestOp = new AsyncRequest<PlayerSessionRequests.SetPlayerSessionMemberSystemPropertiesRequest>(request).ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                    OnScreenLog.Add("Member properties set");
                }
            });

            return requestOp;
        }

        AsyncOp SendPlayerSessionMessage(Int32 currentUserId, PlayerSession currentPS)
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
                    OnScreenLog.Add($"Sending to : {recipient.accountId} on {recipient.platform}");
                }
            }

            PlayerSessionRequests.SendPlayerSessionMessageRequest request = new PlayerSessionRequests.SendPlayerSessionMessageRequest()
            {
                UserId = currentUserId,
                SessionId = currentPS.SessionId,
                Recipients = recipients,
                Payload = "This is a test message " + OnScreenLog.FrameCount,
            };

            var requestOp = new AsyncRequest<PlayerSessionRequests.SendPlayerSessionMessageRequest>(request).ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                    OnScreenLog.Add("Message sent");
                }
            });

            return requestOp;
        }

        AsyncOp GetJoinedPlayerSessions(Int32 currentUserId)
        {
            PlayerSessionRequests.GetJoinedPlayerSessionsByUserRequest request = new PlayerSessionRequests.GetJoinedPlayerSessionsByUserRequest()
            {
                UserId = currentUserId,
            };

            var requestOp = new AsyncRequest<PlayerSessionRequests.GetJoinedPlayerSessionsByUserRequest>(request).ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                    OnScreenLog.Add("Player sessions received");

                    OutputJoinedPlayerSession(antecedent.Request.FoundPlayerSessions);
                }
            });

            return requestOp;
        }

        AsyncOp GetFriendPlayerSessions(Int32 currentUserId)
        {
            PlayerSessionRequests.GetFriendsPlayerSessionsRequest request = new PlayerSessionRequests.GetFriendsPlayerSessionsRequest()
            {
                UserId = currentUserId
            };

            var requestOp = new AsyncRequest<PlayerSessionRequests.GetFriendsPlayerSessionsRequest>(request).ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                    OnScreenLog.Add("Player sessions received");

                    OutputFriendPlayerSessions(antecedent.Request.FriendsPlayerSessions);
                }
            });

            return requestOp;
        }

        static void OutputFriendPlayerSessions(List<PlayerSessionRequests.FriendPlayerSessions> requestFriendsPlayerSessions)
        {
            OnScreenLog.Add("-- Friend Session Output --");

            foreach (var friendWithSessions in requestFriendsPlayerSessions)
            {
                OnScreenLog.Add($"Account ID: {friendWithSessions.accountID}");
                OnScreenLog.Add($"Online ID: {friendWithSessions.onlineID}");
                foreach (var session in friendWithSessions.playerSessions)
                {
                    OnScreenLog.Add($"  SessionID: {session.SessionId}");
                    OnScreenLog.Add($"  SessionName: {session.SessionName}");
                    OnScreenLog.Add($"  PlayerJoinableStatus: {session.PlayerJoinableStatus}");
                    OnScreenLog.Add($"  SpectatorJoinableStatus: {session.SpectatorJoinableStatus}");
                }
            }
        }

        // ***************************************************************************
        // Output
        // ***************************************************************************

        void OutputPlayerSession(PlayerSession playerSession)
        {
            if (playerSession == null)
            {
                OnScreenLog.AddError("   OutputPlayerSession failed : playerSession is null");
                return;
            }

            Color textColour = new Color(1.0f, 1.0f, 0.0f);

            OnScreenLog.AddNewLine();

            OnScreenLog.Add("   Player Session Id : " + playerSession.SessionId, textColour);

            OnScreenLog.Add("   SessionName : " + playerSession.SessionName, textColour);
            OnScreenLog.Add("   CreatedTimeStamp : " + playerSession.CreatedTimestamp, textColour);

            if (playerSession.LocalisedNames != null)
            {
                LocalisedSessionNames names = playerSession.LocalisedNames;
                OnScreenLog.Add("   Session Names : ", textColour);
                OnScreenLog.Add("         Default Locale : " + names.DefaultLocale, textColour);

                if (names.LocalisedNames != null)
                {
                    for (int i = 0; i < names.LocalisedNames.Count; i++)
                    {
                        OnScreenLog.Add("         Text = " + names.LocalisedNames[i].Text + " : Locale = " + names.LocalisedNames[i].Locale, textColour);
                    }
                }
            }

            OnScreenLog.Add("   MaxPlayers : " + playerSession.MaxPlayers, textColour);
            OnScreenLog.Add("   MaxSpectators : " + playerSession.MaxSpectators, textColour);
            OnScreenLog.Add("   SwapSupported : " + playerSession.SwapSupported, textColour);
            OnScreenLog.Add("   JoinDisabled : " + playerSession.JoinDisabled, textColour);
            OnScreenLog.Add("   JoinableUserType : " + playerSession.JoinableUserType, textColour);
            OnScreenLog.Add("   InvitableUserType : " + playerSession.InvitableUserType, textColour);
            OnScreenLog.Add("   SupportedPlatforms : " + playerSession.SupportedPlatforms, textColour);
            OnScreenLog.Add("   LeaderPrivileges : " + playerSession.LeaderPrivileges, textColour);
            OnScreenLog.Add("   ExclusiveLeaderPrivileges : " + playerSession.ExclusiveLeaderPrivileges, textColour);
            OnScreenLog.Add("   DisableSystemUiMenu : " + playerSession.DisableSystemUiMenu, textColour);
            OnScreenLog.Add("   LeaderAccountId : " + playerSession.LeaderAccountId, textColour);

            if (playerSession.JoinableSpecifiedUsers != null)
            {
                OnScreenLog.Add("   JoinableSpecifiedUsers : " + playerSession.JoinableSpecifiedUsers.Length, textColour);
                for (int i = 0; i < playerSession.JoinableSpecifiedUsers.Length; i++)
                {
                    OnScreenLog.Add("        AccountId : " + playerSession.JoinableSpecifiedUsers[i], textColour);
                }
            }

            //public SessionName SessionName { get; internal set; } = new SessionName();

            if (playerSession.Players == null || playerSession.Players.Count == 0)
            {
                OnScreenLog.Add("   Players : None", textColour);
            }
            else
            {
                OnScreenLog.Add("   Players : ", textColour);

                for (int i = 0; i < playerSession.Players.Count; i++)
                {
                    SessionMember member = playerSession.Players[i];

                    if (member != null)
                    {
                        OnScreenLog.Add("        Account Id : " + member.AccountId, textColour);
                        OnScreenLog.Add("              UserId : " + member.UserId.ToString("X8"), textColour);
                        OnScreenLog.Add("              OnlineId : " + member.OnlineId, textColour);
                        OnScreenLog.Add("              Platform : " + member.Platform, textColour);
                        OnScreenLog.Add("              JoinTimestamp : " + member.JoinTimestamp, textColour);
                        OnScreenLog.Add("              IsSpectator : " + member.IsSpectator, textColour);
                        OnScreenLog.Add("              IsLocal : " + member.IsLocal, textColour);
                        OnScreenLog.Add("              IsLeader : " + member.IsLeader, textColour);

                        WebApiPushEvent wpe = playerSession.FindPushEvent(member.UserId);
                        if(wpe != null)
                        {
                            OnScreenLog.Add("              PushEvent Id : " + wpe.PushCallbackId, textColour);
                        }

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

            if (playerSession.Spectators == null || playerSession.Spectators.Count == 0)
            {
                OnScreenLog.Add("   Spectators : None", textColour);
            }
            else
            {
                OnScreenLog.Add("   Spectators : ", textColour);

                for (int i = 0; i < playerSession.Spectators.Count; i++)
                {
                    SessionMember member = playerSession.Spectators[i];

                    if (member != null)
                    {
                        OnScreenLog.Add("        Account Id : " + member.AccountId, textColour);
                        OnScreenLog.Add("              UserId : " + member.UserId.ToString("X8"), textColour);
                        OnScreenLog.Add("              OnlineId : " + member.OnlineId, textColour);
                        OnScreenLog.Add("              Platform : " + member.Platform, textColour);
                        OnScreenLog.Add("              JoinTimestamp : " + member.JoinTimestamp, textColour);
                        OnScreenLog.Add("              IsSpectator : " + member.IsSpectator, textColour);
                        OnScreenLog.Add("              IsLocal : " + member.IsLocal, textColour);

                        WebApiPushEvent wpe = playerSession.FindPushEvent(member.UserId);
                        if (wpe != null)
                        {
                            OnScreenLog.Add("              PushEvent Id : " + wpe.PushCallbackId, textColour);
                        }

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

            if (playerSession.CustomData1 != null)
            {
                OnScreenLog.Add("   CustomData1 : " + OutputBinaryData(playerSession.CustomData1, 10), textColour);
            }

            if (playerSession.CustomData2 != null)
            {
                OnScreenLog.Add("   CustomData2 : " + OutputBinaryData(playerSession.CustomData2, 10), textColour);
            }

            OnScreenLog.Add("Player Session output complete...\n");
        }

        void OutputJoinedPlayerSession(List<PlayerSessionRequests.JoinedPlayerSession> joinedSessions)
        {
            OnScreenLog.Add("Joined Sessions... # " + joinedSessions.Count);

            for (int i = 0; i < joinedSessions.Count; i++)
            {
                OnScreenLog.Add("      SessionId : " + joinedSessions[i].SessionId);
                OnScreenLog.Add("           Platform : " + joinedSessions[i].Platform);
            }
        }

        void OutputSessionsData(PlayerSessionRequests.RetrievedSessionsData retrievedSessionsData)
        {
            OnScreenLog.Add($"{retrievedSessionsData.Sessions.Count} player sessions returned");
            foreach (var session in retrievedSessionsData.Sessions)
                OutputSessionData(session);
        }

        void OutputSessionData(PlayerSessionRequests.RetrievedSessionData retrievedSessionData)
        {
            if (retrievedSessionData == null)
            {
                OnScreenLog.AddError("   OutputSessionData failed : retrievedSessionData is null");
                return;
            }

            OnScreenLog.Add("Retrieved Player Session Data:");

            OnScreenLog.Add("   Flags : " + retrievedSessionData.SetFlags.ToString());

            var flags = retrievedSessionData.SetFlags;

            // SessionId should always be set and will be done internally during the request.
            // If this isn't set the system won't know which session to update
            if (PlayerSession.IsParamFlagSet(flags, PlayerSession.ParamTypes.SessionId))
            {
                OnScreenLog.Add("   SessionId : " + retrievedSessionData.SessionId);
            }

            if (PlayerSession.IsParamFlagSet(flags, PlayerSession.ParamTypes.SessionName))
            {
                OnScreenLog.Add("   SessionName : " + retrievedSessionData.SessionName);
            }
            if (PlayerSession.IsParamFlagSet(flags, PlayerSession.ParamTypes.CreatedTimeStamp))
            {
                OnScreenLog.Add("   CreatedTimeStamp : " + retrievedSessionData.CreatedTimeStamp);
            }
            if (PlayerSession.IsParamFlagSet(flags, PlayerSession.ParamTypes.MaxPlayers))
            {
                OnScreenLog.Add("   MaxPlayers : " + retrievedSessionData.MaxPlayers);
            }
            if (PlayerSession.IsParamFlagSet(flags, PlayerSession.ParamTypes.MaxSpectators))
            {
                OnScreenLog.Add("   MaxSpectators : " + retrievedSessionData.MaxSpectators);
            }
            if (PlayerSession.IsParamFlagSet(flags, PlayerSession.ParamTypes.JoinDisabled))
            {
                OnScreenLog.Add("   JoinDisabled : " + retrievedSessionData.JoinDisabled);
            }
            if (PlayerSession.IsParamFlagSet(flags, PlayerSession.ParamTypes.SupportedPlatforms))
            {
                OnScreenLog.Add("   SupportedPlatforms : " + retrievedSessionData.SupportedPlatforms);
            }
            if (PlayerSession.IsParamFlagSet(flags, PlayerSession.ParamTypes.JoinableUserType))
            {
                OnScreenLog.Add("   JoinableUserType : " + retrievedSessionData.JoinableUserType);
            }
            if (PlayerSession.IsParamFlagSet(flags, PlayerSession.ParamTypes.InvitableUserType))
            {
                OnScreenLog.Add("   InvitableUserType : " + retrievedSessionData.InvitableUserType);
            }
            if (PlayerSession.IsParamFlagSet(flags, PlayerSession.ParamTypes.Leader))
            {
                OnScreenLog.Add("   LeaderAccountId : " + retrievedSessionData.LeaderAccountId);
                OnScreenLog.Add("   LeaderPlatform : " + retrievedSessionData.LeaderPlatform);
            }
            if (PlayerSession.IsParamFlagSet(flags, PlayerSession.ParamTypes.LeaderPrivileges))
            {
                OnScreenLog.Add("   LeaderPrivileges : " + retrievedSessionData.LeaderPrivileges);
            }
            if (PlayerSession.IsParamFlagSet(flags, PlayerSession.ParamTypes.ExclusiveLeaderPrivileges))
            {
                OnScreenLog.Add("   ExclusiveLeaderPrivileges : " + retrievedSessionData.ExclusiveLeaderPrivileges);
            }
            if (PlayerSession.IsParamFlagSet(flags, PlayerSession.ParamTypes.DisableSystemUiMenu))
            {
                OnScreenLog.Add("   DisableSystemUiMenu : " + retrievedSessionData.DisableSystemUiMenu);
            }
            if (PlayerSession.IsParamFlagSet(flags, PlayerSession.ParamTypes.SwapSupported))
            {
                OnScreenLog.Add("   SwapSupported : " + retrievedSessionData.SwapSupported);
            }

            if (PlayerSession.IsParamFlagSet(flags, PlayerSession.ParamTypes.JoinableSpecifiedUsers))
            {
                if (retrievedSessionData.JoinableSpecifiedUsers != null)
                {
                    OnScreenLog.Add("   JoinableSpecifiedUsers :");

                    for (int i = 0; i < retrievedSessionData.JoinableSpecifiedUsers.Length; i++)
                    {
                        OnScreenLog.Add("           Id : " + retrievedSessionData.JoinableSpecifiedUsers[i]);
                    }
                }
                else
                {
                    OnScreenLog.Add("   JoinableSpecifiedUsers : Notset");
                }
            }

            if (PlayerSession.IsParamFlagSet(flags, PlayerSession.ParamTypes.LocalizedSessionName))
            {
                if (retrievedSessionData.LocalisedNames != null)
                {
                    LocalisedSessionNames names = retrievedSessionData.LocalisedNames;
                    OnScreenLog.Add("   Session Names : ");
                    OnScreenLog.Add("         Default Locale : " + names.DefaultLocale);

                    if (names.LocalisedNames != null)
                    {
                        for (int i = 0; i < names.LocalisedNames.Count; i++)
                        {
                            OnScreenLog.Add("         Text = " + names.LocalisedNames[i].Text + " : Locale = " + names.LocalisedNames[i].Locale);
                        }
                    }
                }
                else
                {
                    OnScreenLog.Add("   LocalisedNames : Notset");
                }
            }

            if (PlayerSession.IsParamFlagSet(flags, PlayerSession.ParamTypes.CustomData1))
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

            if (PlayerSession.IsParamFlagSet(flags, PlayerSession.ParamTypes.CustomData2))
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

            if (PlayerSession.IsParamFlagSet(flags, PlayerSession.ParamTypes.MemberPlayers) || PlayerSession.IsParamFlagSet(flags, PlayerSession.ParamTypes.MemberPlayersCustomData1))
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

            if (PlayerSession.IsParamFlagSet(flags, PlayerSession.ParamTypes.MemberSpectators) || PlayerSession.IsParamFlagSet(flags, PlayerSession.ParamTypes.MemberSpectatorsCustomData1))
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


        void OutputRetrievedInvitations(List<PlayerSessionRequests.RetrievedInvitation> invitations)
        {
            OnScreenLog.AddNewLine();

            if (invitations != null)
            {
                OnScreenLog.Add("Invitations : " + invitations.Count);

                for (int i = 0; i < invitations.Count; i++)
                {
                    PlayerSessionRequests.RetrievedInvitation invite = invitations[i];

                    if (invite != null)
                    {
                        OnScreenLog.Add("    InvitationId : " + invite.InvitationId);
                        OnScreenLog.Add("        Flags : " + invite.SetFlags);
                        OnScreenLog.Add("        FromAccountId : " + invite.FromAccountId);
                        OnScreenLog.Add("        FromOnLineId : " + invite.FromOnLineId);
                        OnScreenLog.Add("        FromPlatform : " + invite.FromPlatform);
                        OnScreenLog.Add("        SessionId : " + invite.SessionId);
                        OnScreenLog.Add("        SupportedPlatforms : " + invite.SupportedPlatforms);
                        OnScreenLog.Add("        ReceivedTimestamp : " + invite.ReceivedTimestamp);
                        OnScreenLog.Add("        InvitationInvalid : " + invite.InvitationInvalid);
                    }
                }
            }
            else
            {
                OnScreenLog.Add("Invitations : 0");
            }
        }

        void OutputJoinableUsers(List<UInt64> accountIds)
        {
            OnScreenLog.AddNewLine();

            if (accountIds != null)
            {
                OnScreenLog.Add("Joinable Account Ids : " + accountIds.Count);

                for (int i = 0; i < accountIds.Count; i++)
                {
                    OnScreenLog.Add("        Account Id : " + accountIds[i]);
                }
            }
        }

    }
#endif
}
