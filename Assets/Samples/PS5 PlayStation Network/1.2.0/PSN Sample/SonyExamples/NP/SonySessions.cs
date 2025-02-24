
#if UNITY_PS5 || UNITY_PS4
using System;
using System.Collections.Generic;
using Unity.PSN.PS5.Aysnc;
using Unity.PSN.PS5.Sessions;
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
        MenuLayout m_MenuSessions;

        public SonySessions()
        {
            Initialize();
        }

        public MenuLayout GetMenu()
        {
            return m_MenuSessions;
        }

        public void OnEnter()
        {
        }

        public void OnExit()
        {
        }

        public void Process(MenuStack stack)
        {
            MenuSessions(stack);
        }

        public void Initialize()
        {
            m_MenuSessions = new MenuLayout(this, 450, 20);

            SessionsManager.OnRawUserEvent = RawUserEventHandler;
            SessionsManager.OnUserEvent = UserEventHandler;
        }

        public void SetupSessionNotifications()
        {
            SetupGameIntentCallback();
            SetupMatchMakingCallbacks();
        }

        // ***************************************************************************
        // Notifications
        // ***************************************************************************

        public void RawSessionEventHandler(Session session, WebApiNotifications.CallbackParams eventData)
        {
            OnScreenLog.Add("Sample.RawEventHandler : " + eventData.DataType, Color.green);
        }

        public void RawUserEventHandler(WebApiNotifications.CallbackParams eventData)
        {
            if (eventData.Data != null)
            {
                OnScreenLog.Add("Sample.RawUserEventHandler : " + eventData.DataType + " : " + eventData.Data, Color.green);
            }
            else
            {
                OnScreenLog.Add("Sample.RawUserEventHandler : " + eventData.DataType, Color.green);
            }
        }

        public void UserEventHandler(SessionsManager.Notification notificationData)
        {
            if (notificationData is SessionsManager.PlayerNotification)
            {
                SessionsManager.PlayerNotification pn = notificationData as SessionsManager.PlayerNotification;

                if (pn.NotificationType == PlayerSessionNotifications.NotificationTypes.SessionMessage)
                {
                    OnScreenLog.Add("UserEventHandler (PlayerSession) : " + pn.NotificationType + " : " + notificationData.SessionId + " : " + notificationData.FromAccountId + " : " + notificationData.ToAccountId + " : " + notificationData.MessagePayload, Color.cyan);
                }
                else
                {
                    OnScreenLog.Add("UserEventHandler (PlayerSession) : " + pn.NotificationType + " : " + notificationData.SessionId + " : " + notificationData.FromAccountId + " : " + notificationData.ToAccountId, Color.cyan);
                }
            }
            else if (notificationData is SessionsManager.GameNotification)
            {
                SessionsManager.GameNotification gn = notificationData as SessionsManager.GameNotification;

                if (gn.NotificationType == GameSessionNotifications.NotificationTypes.InvitationsCreated)
                {
                    OnScreenLog.Add("UserEventHandler (GameSession) : " + gn.NotificationType + " : " + notificationData.SessionId + " : " + notificationData.FromAccountId + " : " + notificationData.ToAccountId, Color.cyan);
                    s_matchedSessionId = notificationData.SessionId;

                    // Get the game session
                    if (GamePad.activeGamePad != null)
                    {
                        int userId = GamePad.activeGamePad.loggedInUser.userId;

                        AsyncOp requestOp = GetGameSessionsRequest(userId, notificationData.SessionId);

                        SessionsManager.Schedule(requestOp);
                    }
                }
                else if (gn.NotificationType == GameSessionNotifications.NotificationTypes.SessionMessage)
                {
                    OnScreenLog.Add("UserEventHandler (GameSession) : " + gn.NotificationType + " : " + notificationData.SessionId + " : " + notificationData.FromAccountId + " : " + notificationData.ToAccountId + " : " + notificationData.MessagePayload, Color.cyan);
                }
                else
                {
                    OnScreenLog.Add("UserEventHandler (GameSession) : " + gn.NotificationType + " : " + notificationData.SessionId + " : " + notificationData.FromAccountId + " : " + notificationData.ToAccountId, Color.cyan);
                }
            }
        }

        // ***************************************************************************
        // Helpers
        // ***************************************************************************

        public byte[] MakeData(int size, int startNumber)
        {
            byte[] someData = new byte[size];

            for (int i = 0; i < someData.Length; i++)
            {
                someData[i] = (byte)(startNumber + i);
            }

            return someData;
        }

        public UInt64 GetFirstNonLeaderAccountId(int currentUserId, PlayerSession session)
        {
            UInt64 accountId = SessionMember.InvalidAccountId;

            // Find a member in the active session that isn't the leader or the current user
            var players = session.Players;

            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].UserId != currentUserId && players[i].IsLeader == false)
                {
                    accountId = players[i].AccountId;
                    return accountId;
                }
            }

            return accountId;
        }

        public List<UInt64> GetAllLocalAcountIds()
        {
            List<UInt64> acountIds = new List<UInt64>();

            for (int i = 0; i < 4; i++)
            {
                var userDetails = PlatformInput.GetUsersDetails(i);

                if (userDetails.onlineStatus == PlatformInput.OnlineStatus.SignedIn)
                {
                    acountIds.Add(userDetails.accountId);
                }
            }

            return acountIds;
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

        // ***************************************************************************
        // Menus
        // ***************************************************************************

        enum MenuTypes
        {
            SessionSelection,
            PlayerSessions,
            GameSessions,
            Matches,
            MatchMaking,
            Tournaments
        }

        MenuTypes currentMenu = MenuTypes.SessionSelection;

        public void MenuSessions(MenuStack menuStack)
        {
            m_MenuSessions.Update();

            // Get the current user. Are they signed in an valid?
            // Do they have a session already?
            // If not is there a player session created by another user.

            if (currentMenu == MenuTypes.SessionSelection)
            {
                DoMainButtons(menuStack);
            }
            else if (currentMenu == MenuTypes.PlayerSessions)
            {
                DoPlayerSessionButtons();
            }
            else if (currentMenu == MenuTypes.GameSessions)
            {
                DoGameSessionButtons();
            }
            else if (currentMenu == MenuTypes.Matches)
            {
                DoMatchesButtons();
            }
            else if (currentMenu == MenuTypes.MatchMaking)
            {
                DoMatchMakingButtons();
            }
            #if UNITY_2023_3_OR_NEWER //Tournament SDK Links were added in 2023.3
            else if (currentMenu == MenuTypes.Tournaments)
            {
                DoTournamentsButtons();
            }
            #endif
        }

        // Menu Buttons

        void DoMainButtons(MenuStack menuStack)
        {
            if (m_MenuSessions.AddItem("Player Sessions", "Methods to control player sessions"))
            {
                currentMenu = MenuTypes.PlayerSessions;
            }

            if (m_MenuSessions.AddItem("Game Sessions", "Methods to control game sessions"))
            {
                currentMenu = MenuTypes.GameSessions;
            }

            if (m_MenuSessions.AddItem("Matches", "Methods to control matches"))
            {
                currentMenu = MenuTypes.Matches;
            }

#if UNITY_2023_3_OR_NEWER //Tournament SDK Links were added in 2023.3
            if (m_MenuSessions.AddItem("Tournaments", "Methods to show tournaments"))
            {
                currentMenu = MenuTypes.Tournaments;
            }
#endif

            if (m_MenuSessions.AddItem("MatchMaking", "Methods to control matchmaking"))
            {
                currentMenu = MenuTypes.MatchMaking;

                OnScreenLog.AddWarning("1) Matchmaking requires two or more devices.");
                OnScreenLog.AddWarning("2) On at least two devices create a Player Session with one or more local users.");
                OnScreenLog.AddWarning("3) On the same devices Submit Ticket.");
                OnScreenLog.AddWarning("4) Wait until the display shows a UserEventHandler (GameSession) : InvitiationsCreated notification.");
                OnScreenLog.AddWarning("5) Use Get Ticket to refresh the ticket. It should now have an Offer Id set.");
                OnScreenLog.AddWarning("6) Use Get Offer to view all the matched players and");
                OnScreenLog.AddWarning("      the game session id created by the servers.");
                OnScreenLog.AddWarning("Note:  If only one device is used the application will display a Ticket Timeout notification ");
                OnScreenLog.AddWarning("       after 60 seconds have passed.");
                OnScreenLog.AddWarning("Backfill:");
                OnScreenLog.AddWarning("7) Use Submit Ticket (w. Game Session) to use Game Session ID returned from offer (step 6).");
                OnScreenLog.AddWarning("8) On second device submit a ticket (step 3). ");
                OnScreenLog.AddWarning("9) On First device Get offer to get the (step 7) game session ID.");
            }

            if (m_MenuSessions.AddBackIndex("Back"))
            {
                menuStack.PopMenu();
            }
        }

    }
#endif
}
