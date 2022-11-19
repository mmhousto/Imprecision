using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GooglePlayGames;
using UnityEngine.SocialPlatforms;

namespace Com.MorganHouston.Imprecision
{
    public class SocialManager : MonoBehaviour
    {
        
        public void ShowLeaderboards()
        {
            Social.ShowLeaderboardUI();
        }

        public void ShowAchievements()
        {
            Social.ShowAchievementsUI();
        }
    }
}
