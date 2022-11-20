using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GooglePlayGames;
using UnityEngine.SocialPlatforms;

namespace Com.MorganHouston.Imprecision
{
    public static class LeaderboardManager
    {

        public static void UpdateLeaderboard(int value, string leaderboardID)
        {
            Social.ReportScore(value, leaderboardID, (bool success) =>
            {
                // handle success or failure
            });

        }

        #region APPLES


        public static void UnlockApple1()
        {
            Social.ReportProgress("CgkIqK61pYkHEAIQBQ", 100.0, (bool success) =>
            {
                    // handle success or failure
            });
        }

        public static void UnlockApple2(double step)
        {
#if UNITY_IOS
            Social.ReportProgress("CgkIqK61pYkHEAIQBg", step*10, (bool success) =>
            {
                // handle success or failure
            });
#elif UNITY_ANDROID
            PlayGamesPlatform.Instance.IncrementAchievement(
                "CgkIqK61pYkHEAIQBg", (int)step, (bool success) => {
            // handle success or failure
        });
#endif
        }

        public static void UnlockApple3(double step)
        {
#if UNITY_IOS
            Social.ReportProgress("CgkIqK61pYkHEAIQBw", step*4, (bool success) =>
            {
                // handle success or failure
            });
#elif UNITY_ANDROID
            PlayGamesPlatform.Instance.IncrementAchievement(
                "CgkIqK61pYkHEAIQBw", (int)step, (bool success) => {
                    // handle success or failure
                });
#endif
        }

        public static void UnlockApple4(double step)
        {
#if UNITY_IOS
            Social.ReportProgress("CgkIqK61pYkHEAIQCA", step*2, (bool success) =>
            {
                // handle success or failure
            });
#elif UNITY_ANDROID
            PlayGamesPlatform.Instance.IncrementAchievement(
                "CgkIqK61pYkHEAIQCA", (int)step, (bool success) => {
                    // handle success or failure
                });
#endif
        }

        public static void CheckAppleAchievements()
        {
            int count = 0;
            foreach (int shot in Player.Instance.AppleShotOnLevels)
            {
                if (shot == 1)
                {
                    count++;
                }
            }

            if (count == 1)
            {
                UnlockApple1();
            }
            else if (count > 1 && count <= 10)
            {
                UnlockApple2(count);
            }
            else if (count > 10 && count <= 25)
            {
                UnlockApple3(count);
            }
            else if (count > 25 && count <= 50)
            {
                UnlockApple4(count);
            }
        }


        #endregion


        #region PERFECTS




        public static void UnlockPerfect1()
        {
            Social.ReportProgress("CgkIqK61pYkHEAIQCQ", 100.0, (bool success) =>
            {
                    // handle success or failure
                });
        }

        public static void UnlockPerfect2(double step)
        {
#if UNITY_IOS
            Social.ReportProgress("CgkIqK61pYkHEAIQCg", step*4, (bool success) =>
            {
                // handle success or failure
            });
#elif UNITY_ANDROID
            PlayGamesPlatform.Instance.IncrementAchievement(
                "CgkIqK61pYkHEAIQCg", (int)step, (bool success) => {
                    // handle success or failure
                });
#endif
        }

        public static void UnlockPerfect3(double step)
        {
#if UNITY_IOS
            Social.ReportProgress("CgkIqK61pYkHEAIQCw", step*2, (bool success) =>
            {
                // handle success or failure
            });
#elif UNITY_ANDROID
            PlayGamesPlatform.Instance.IncrementAchievement(
                "CgkIqK61pYkHEAIQCw", (int)step, (bool success) => {
                    // handle success or failure
                });
#endif
        }

        public static void CheckPerfectAchievements()
        {
            int count = 0;
            foreach (int threeStar in Player.Instance.Levels)
            {
                if (threeStar == 3)
                {
                    count++;
                }
            }

            if (count == 1)
            {
                UnlockPerfect1();
            }
            else if (count > 1 && count <= 25)
            {
                UnlockPerfect2(count);
            }
            else if (count > 25 && count <= 50)
            {
                UnlockPerfect3(count);
            }
        }


        #endregion


        #region PRECISE


        public static void UnlockBullseye()
        {
            Social.ReportProgress("CgkIqK61pYkHEAIQFA", 100.0, (bool success) =>
            {
                // handle success or failure
            });
        }

        public static void UnlockPrecise1()
        {
            Social.ReportProgress("CgkIqK61pYkHEAIQDA", 100.0, (bool success) =>
            {
                // handle success or failure
            });
        }

        public static void UnlockPrecise2(double step)
        {
#if UNITY_IOS
            Social.ReportProgress("CgkIqK61pYkHEAIQDQ", step*4, (bool success) =>
            {
                // handle success or failure
            });
#elif UNITY_ANDROID
            PlayGamesPlatform.Instance.IncrementAchievement(
                "CgkIqK61pYkHEAIQDQ", (int)step, (bool success) => {
                    // handle success or failure
                });
#endif
        }

        public static void UnlockPrecise3(double step)
        {
#if UNITY_IOS
            Social.ReportProgress("CgkIqK61pYkHEAIQDg", step*2, (bool success) =>
            {
                // handle success or failure
            });
#elif UNITY_ANDROID
            PlayGamesPlatform.Instance.IncrementAchievement(
                "CgkIqK61pYkHEAIQDg", (int)step, (bool success) => {
                    // handle success or failure
                });
#endif
        }

        public static void CheckBullseyeAchievements()
        {
            int count = 0;
            foreach (int bullseye in Player.Instance.BullseyesOnLevels)
            {
                if (bullseye == 1)
                {
                    count++;
                }
            }

            if (count == 1)
            {
                UnlockPrecise1();
            }
            else if (count > 1 && count <= 25)
            {
                UnlockPrecise2(count);
            }
            else if (count > 25 && count <= 50)
            {
                UnlockPrecise3(count);
            }
        }


        #endregion


        #region ARCHER


        public static void UnlockArcher1()
        {
            Social.ReportProgress("CgkIqK61pYkHEAIQDw", 100.0, (bool success) =>
            {
                // handle success or failure
            });
        }

        public static void UnlockArcher2(double step)
        {
#if UNITY_IOS
            Social.ReportProgress("CgkIqK61pYkHEAIQEA", step*10, (bool success) =>
            {
                // handle success or failure
            });
#elif UNITY_ANDROID
            PlayGamesPlatform.Instance.IncrementAchievement(
                "CgkIqK61pYkHEAIQEA", (int)step, (bool success) => {
                    // handle success or failure
                });
#endif
        }

        public static void UnlockArcher3(double step)
        {
#if UNITY_IOS
            Social.ReportProgress("CgkIqK61pYkHEAIQEQ", step*4, (bool success) =>
            {
                // handle success or failure
            });
#elif UNITY_ANDROID
            PlayGamesPlatform.Instance.IncrementAchievement(
                "CgkIqK61pYkHEAIQEQ", (int)step, (bool success) => {
                    // handle success or failure
                });
#endif
        }

        public static void UnlockArcher4(double step)
        {
#if UNITY_IOS
            Social.ReportProgress("CgkIqK61pYkHEAIQEg", step*2, (bool success) =>
            {
                // handle success or failure
            });
#elif UNITY_ANDROID
            PlayGamesPlatform.Instance.IncrementAchievement(
                "CgkIqK61pYkHEAIQEg", (int)step, (bool success) => {
                    // handle success or failure
                });
#endif
        }

        public static void CheckArcherAchievements()
        {
            int count = 0;
            foreach (int level in Player.Instance.Levels)
            {
                if (level >= 1)
                {
                    count++;
                }
            }

            if (count == 1)
            {
                UnlockArcher1();
            } 
            else if (count > 1 && count <= 10)
            {
                UnlockArcher2(count);
            }
            else if (count > 10 && count <= 25)
            {
                UnlockArcher3(count);
            }
            else if (count > 25 && count <= 50)
            {
                UnlockArcher4(count);
            }
        }


        #endregion


        public static void UnlockJewel()
        {
            Social.ReportProgress("CgkIqK61pYkHEAIQEw", 100.0, (bool success) =>
            {
                // handle success or failure
            });
        }

        
    }
}