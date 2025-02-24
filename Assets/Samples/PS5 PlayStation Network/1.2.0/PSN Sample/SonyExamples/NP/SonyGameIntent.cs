
#if UNITY_PS5 || UNITY_PS4
using Unity.PSN.PS5;
using Unity.PSN.PS5.GameIntent;
#endif

namespace PSNSample
{
#if UNITY_PS5 || UNITY_PS4

    public class SonyGameIntentNotifications
    {
        public SonyGameIntentNotifications()
        {
            GameIntentSystem.OnGameIntentNotification += OnGameIntentNotification;
        }

        private void OnGameIntentNotification(GameIntentSystem.GameIntent gameIntent)
        {
            OnScreenLog.Add("Game Intent");
            OnScreenLog.Add(System.String.Format("    UserId : 0x{0:X}", gameIntent.UserId));
            OnScreenLog.Add("    Intent Type : " + gameIntent.IntentType);

            switch (gameIntent)
            {
                case GameIntentSystem.LaunchActivity launchActivity:
                    OnScreenLog.Add("    Activity Id : " + launchActivity.ActivityId);
                    break;
                case GameIntentSystem.JoinSession join:
                    OnScreenLog.Add("    Player Session Id : " + join.PlayerSessionId);
                    OnScreenLog.Add("    Member Type : " + join.MemberType);
                    break;
                case GameIntentSystem.LaunchMultiplayerActivity activity:
                    OnScreenLog.Add("    Activity Id : " + activity.ActivityId);
                    OnScreenLog.Add("    Player Session Id : " + activity.PlayerSessionId);
                    break;
                case GameIntentSystem.LaunchTournamentMatch tournament:
                    OnScreenLog.Add("    Activity Id : " + tournament.ActivityId);
                    OnScreenLog.Add("    Match Id: " + tournament.MatchId);
                    #if UNITY_PS5
                    SonySessions.JoinTournamentMatch(tournament.UserId, tournament.MatchId);
                    #endif
                    break;
                default:
                    OnScreenLog.AddError("Unknown Game Intent type");
                    break;
            }
        }
    }
#endif
}
