#if UNITY_PS5
using System;
using Unity.PSN.PS5.Matches;
using Unity.PSN.PS5.Sessions;
using UnityEngine;

#if UNITY_PS4
using PlatformInput = UnityEngine.PS4.PS4Input;
#elif UNITY_PS5
using PlatformInput = UnityEngine.PS5.PS5Input;
#endif

namespace PSNSample
{
    public partial class SonySessions
    {
        public static void JoinTournamentMatch(Int32 userID, string matchID)
        {
            Match matchObj = Match.CreateMatch(matchID, out _);
            UInt64 accountId = GamePad.activeGamePad.loggedInUser.accountId;

            OnScreenLog.Add($"Attempting to join tournament match id: {matchID}");

            var joinReq = GetMatchJoinRequest(userID, accountId.ToString(), accountId, matchObj);
            SessionsManager.Schedule(joinReq);

            var detailsReq = GetMatchDetailRequest(userID, matchObj).ContinueWith(asyncOp =>
            {
                OnScreenLog.Add($"Joined Tournament Match {matchObj.MatchId}. To control the Match use the Sessions > Matches Menu", Color.yellow);
            });
            SessionsManager.Schedule(detailsReq);
        }

#if UNITY_2023_3_OR_NEWER //Tournament SDK Links were added in 2023.3.a17
        public void DoTournamentsButtons()
        {
            if (GamePad.activeGamePad == null || GamePad.activeGamePad.loggedInUser.onlineStatus != PlatformInput.OnlineStatus.SignedIn)
            {
                return;
            }

            int currentUserId = GamePad.activeGamePad.loggedInUser.userId;

            if (m_MenuSessions.AddItem("Open Active Tournament"))
            {
                int ret = UnityEngine.PS5.Utility.SystemServiceOpenTournamentOccurrence(currentUserId, "_activeTournament");
                OnScreenLog.Add("Opening Active Tournament");
                if (ret < 0)
                {
                    OnScreenLog.AddError($"Opening Active Tournament returned {ret:x8}");
                }
            }

            if (m_MenuSessions.AddItem("Open Next Available Tournament"))
            {
                int ret = UnityEngine.PS5.Utility.SystemServiceOpenTournamentOccurrence(currentUserId, "_nextAvailableTournament");
                OnScreenLog.Add("Opening Next Available Tournament");
                if (ret < 0)
                {
                    OnScreenLog.AddError($"Opening Next Available Tournament returned {ret:x8}");
                }
            }

            if (m_MenuSessions.AddBackIndex("Back"))
            {
                currentMenu = MenuTypes.SessionSelection;
            }
        }
#endif
    }
}
#endif
