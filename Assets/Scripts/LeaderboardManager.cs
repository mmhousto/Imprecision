using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GooglePlayGames;
using UnityEngine.SocialPlatforms;

namespace Com.MorganHouston.Imprecision
{
    public static class LeaderboardManager
    {
        public static void UpdateMostPointsLeaderboard()
        {
            Social.ReportScore(Player.Instance.UserPoints, "CgkI07-ynroOEAIQBQ", (bool success) =>
            {
                // handle success or failure
            });

        }

        public static void UpdateSoloHighestWaveLeaderboard()
        {
            Social.ReportScore(2, "CgkI07-ynroOEAIQAQ", (bool success) =>
            {
                    // handle success or failure
                });
        }

        public static void UpdatePartyHighestWaveLeaderboard()
        {
            Social.ReportScore(2, "CgkI07-ynroOEAIQAg", (bool success) =>
            {
                    // handle success or failure
                });
        }

        public static void UpdateCubesDestroyedLeaderboard()
        {
            Social.ReportScore(2, "CgkI07-ynroOEAIQBA", (bool success) =>
            {
                    // handle success or failure
                });

        }

        public static void UpdateAccuracyLeaderboard()
        {
            Social.ReportScore((long)(Player.Instance.Accuracy * 100), "CgkI07-ynroOEAIQAw", (bool success) =>
            {
                    // handle success or failure
                });
        }

        public static void UnlockStayinAlive()
        {
            Social.ReportProgress("CgkI07-ynroOEAIQBg", 100.0f, (bool success) =>
            {
                    // handle success or failure
                });
        }

        public static void UnlockStayinAliveTogether()
        {
            Social.ReportProgress("CgkI07-ynroOEAIQDw", 100.0f, (bool success) =>
            {
                    // handle success or failure
                });
        }

        public static void UnlockCubeDestroyerI()
        {
            Social.ReportProgress("CgkI07-ynroOEAIQBw", 100.0f, (bool success) =>
            {
                    // handle success or failure
                });
        }

        public static void UnlockCubeDestroyerII()
        {
            Social.ReportProgress("CgkI07-ynroOEAIQCA", 100.0f, (bool success) =>
            {
                    // handle success or failure
                });
        }

        public static void UnlockCubeDestroyerIII()
        {
            Social.ReportProgress("CgkI07-ynroOEAIQCQ", 100.0f, (bool success) =>
            {
                    // handle success or failure
                });
        }

        public static void UnlockRicochetKing()
        {
            Social.ReportProgress("CgkI07-ynroOEAIQCg", 100.0f, (bool success) =>
            {
                    // handle success or failure
                });
        }

        public static void UnlockTriggerHappyI()
        {
            Social.ReportProgress("CgkI07-ynroOEAIQCw", 100.0f, (bool success) =>
            {
                    // handle success or failure
                });
        }

        public static void UnlockTriggerHappyII()
        {
            Social.ReportProgress("CgkI07-ynroOEAIQDA", 100.0f, (bool success) =>
            {
                    // handle success or failure
                });
        }

        public static void UnlockTriggerHappyIII()
        {
            Social.ReportProgress("CgkI07-ynroOEAIQDQ", 100.0f, (bool success) =>
            {
                    // handle success or failure
                });
        }

        public static void UnlockNGamer1()
        {
            Social.ReportProgress("CgkI07-ynroOEAIQDg", 100.0f, (bool success) =>
            {
                    // handle success or failure
                });
        }

    }
}