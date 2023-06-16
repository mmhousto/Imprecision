#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLESTEAMWORKS
#endif

using UnityEngine;
using System.Collections;
using System.ComponentModel;
#if !DISABLESTEAMWORKS
using Steamworks;
#endif
using Unity.Services.Authentication;

namespace Com.MorganHouston.Imprecision
{
	// This is a port of StatsAndAchievements.cpp from SpaceWar, the official Steamworks Example.
	class SteamStatsAndAchievements : MonoBehaviour
	{
#if !DISABLESTEAMWORKS
		private static SteamStatsAndAchievements instance;

		public static SteamStatsAndAchievements Instance { get { return instance; } }

		private enum Achievement : int
		{
			CgkIqK61pYkHEAIQBQ,
			CgkIqK61pYkHEAIQBg,
			CgkIqK61pYkHEAIQBw,
			CgkIqK61pYkHEAIQCA,
			CgkIqK61pYkHEAIQCQ,
			CgkIqK61pYkHEAIQCg,
			CgkIqK61pYkHEAIQCw,
			CgkIqK61pYkHEAIQFA,
			CgkIqK61pYkHEAIQDA,
			CgkIqK61pYkHEAIQDQ,
			CgkIqK61pYkHEAIQDg,
			CgkIqK61pYkHEAIQDw,
			CgkIqK61pYkHEAIQEA,
			CgkIqK61pYkHEAIQEQ,
			CgkIqK61pYkHEAIQEg,
			CgkIqK61pYkHEAIQEw
		};

		private Achievement_t[] m_Achievements = new Achievement_t[] {
		new Achievement_t(Achievement.CgkIqK61pYkHEAIQBQ, "Red Delicious \"Coffee Grinds in a Leather Glove\"", "Shoot the apple, on any level."),
		new Achievement_t(Achievement.CgkIqK61pYkHEAIQBg, "Golden Delicious \"The West Virginia Has-Been\"", "Shoot the apple, on 10 different levels."),
		new Achievement_t(Achievement.CgkIqK61pYkHEAIQBw, "Granny Smith \"The Original Sour Apple\"", "Shoot the apple, on 25 different levels."),
		new Achievement_t(Achievement.CgkIqK61pYkHEAIQCA, "Honey Crisp \"The Worldwide Favorite\"", "Shoot the apple, on all 50 levels."),
		new Achievement_t(Achievement.CgkIqK61pYkHEAIQCQ, "Perfect!", "Three Star a Level."),
		new Achievement_t(Achievement.CgkIqK61pYkHEAIQCg, "Perfection!", "Three Star 25 Levels."),
		new Achievement_t(Achievement.CgkIqK61pYkHEAIQCw, "Perfectionist!", "Three Star Every Level."),
		new Achievement_t(Achievement.CgkIqK61pYkHEAIQFA, "Bullseye!", "Hit a Bullseye on a Target."),
		new Achievement_t(Achievement.CgkIqK61pYkHEAIQDA, "Precise!", "Hit a Bullseye on Every Target, on a Level."),
		new Achievement_t(Achievement.CgkIqK61pYkHEAIQDQ, "Precision!", "Hit a Bullseye on Every Target, on 25 Levels."),
		new Achievement_t(Achievement.CgkIqK61pYkHEAIQDg, "Precisionist!", "Hit a Bullseye on Every Target, on 50 Levels."),
		new Achievement_t(Achievement.CgkIqK61pYkHEAIQDw, "That's a Start", "Beat the first Precision level."),
		new Achievement_t(Achievement.CgkIqK61pYkHEAIQEA, "Keep It Up", "Beat 10 Precision levels."),
		new Achievement_t(Achievement.CgkIqK61pYkHEAIQEQ, "Now You Are Getting Somewhere", "Beat 25 Precision levels."),
		new Achievement_t(Achievement.CgkIqK61pYkHEAIQEg, "Robin Hood", "Beat Every Precision level."),
		new Achievement_t(Achievement.CgkIqK61pYkHEAIQEw, "What is this for?", "Obtain a Jewel.")
	};

		// Our GameID
		private CGameID m_GameID;

		// Did we get the stats from Steam?
		private bool m_bRequestedStats;
		private bool m_bStatsValid;

		// Should we store stats this frame?
		private bool m_bStoreStats;

		// Current Stat details
		private float m_flGameFeetTraveled;
		private float m_ulTickCountGameStart;
		private double m_flGameDurationSeconds;

		// Persisted Stat details
		private int m_nUserPoints;
		private int m_nUserLevel;
		private int m_nUserXP;
		private int m_nJewels;
		private int m_nArrowsFired;
		private int m_nTargetsHit;
		private int m_nBullseyes;
		private int m_nStars;
		private int m_nThreeStars;
		private int m_nPerfects;
		private int m_nApplesShot;
		private int m_nLevelsBeat;

		private float m_flAccuracy;

		private Player player;

		protected Callback<UserStatsReceived_t> m_UserStatsReceived;
		protected Callback<UserStatsStored_t> m_UserStatsStored;
		protected Callback<UserAchievementStored_t> m_UserAchievementStored;

        private void Awake()
        {
			if (instance != null && instance != this)
			{
				Destroy(this.gameObject);
				return;
			}
			else
			{
				instance = this;
				DontDestroyOnLoad(Instance.gameObject);
			}
        }

        void OnEnable()
		{
			if (!SteamManager.Initialized || !AuthenticationService.Instance.IsSignedIn || CloudSaveLogin.Instance.currentSSO != CloudSaveLogin.ssoOption.Steam)
				return;

			player = Player.Instance;
			// Cache the GameID for use in the Callbacks
			m_GameID = new CGameID(SteamUtils.GetAppID());

			m_UserStatsReceived = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
			m_UserStatsStored = Callback<UserStatsStored_t>.Create(OnUserStatsStored);
			m_UserAchievementStored = Callback<UserAchievementStored_t>.Create(OnAchievementStored);

			// These need to be reset to get the stats upon an Assembly reload in the Editor.
			m_bRequestedStats = false;
			m_bStatsValid = false;
		}

		private void Update()
		{
			if (!SteamManager.Initialized || !AuthenticationService.Instance.IsSignedIn || CloudSaveLogin.Instance.currentSSO != CloudSaveLogin.ssoOption.Steam)
				return;

			if(player == null)
				player = Player.Instance;

			if (!m_bRequestedStats)
			{
				// Is Steam Loaded? if no, can't get stats, done
				if (!SteamManager.Initialized)
				{
					m_bRequestedStats = true;
					return;
				}

				// If yes, request our stats
				bool bSuccess = SteamUserStats.RequestCurrentStats();

				// This function should only return false if we weren't logged in, and we already checked that.
				// But handle it being false again anyway, just ask again later.
				m_bRequestedStats = bSuccess;
			}

			if (!m_bStatsValid)
				return;

			// Get info from sources
			m_nUserPoints = player.UserPoints;
			m_nUserLevel = player.UserLevel;
			m_nUserXP = player.UserXP;
			m_nJewels = player.Jewels;
			m_nArrowsFired = player.ArrowsFired;
			m_nTargetsHit = player.TargetsHit;
			m_flAccuracy = (m_nArrowsFired == 0) ? 0 : (m_nTargetsHit * 100) / m_nArrowsFired;
			m_nBullseyes = player.BullseyesHit;
			m_nStars = player.GetTotalStars();
			m_nThreeStars = player.GetThreeStars();
			m_nPerfects = player.GetPerfects();
			m_nApplesShot = player.GetApplesShotOnLevels();
			m_nLevelsBeat = player.GetTotalLevelsBeat();

			// Evaluate achievements
			foreach (Achievement_t achievement in m_Achievements)
			{
				if (achievement.m_bAchieved)
					continue;

				switch (achievement.m_eAchievementID)
				{
					// APPLES
					case Achievement.CgkIqK61pYkHEAIQBQ:
						if (m_nApplesShot != 0)
						{
							UnlockAchievement(achievement);
						}
						break;
					case Achievement.CgkIqK61pYkHEAIQBg:
						if (m_nApplesShot >= 10)
						{
							UnlockAchievement(achievement);
						}
						break;
					case Achievement.CgkIqK61pYkHEAIQBw:
						if (m_nApplesShot >= 25)
						{
							UnlockAchievement(achievement);
						}
						break;
					case Achievement.CgkIqK61pYkHEAIQCA:
						if (m_nApplesShot >= 50)
						{
							UnlockAchievement(achievement);
						}
						break;

					// THREE STARS
					case Achievement.CgkIqK61pYkHEAIQCQ:
						if (m_nThreeStars != 0)
						{
							UnlockAchievement(achievement);
						}
						break;
					case Achievement.CgkIqK61pYkHEAIQCg:
						if (m_nThreeStars >= 25)
						{
							UnlockAchievement(achievement);
						}
						break;
					case Achievement.CgkIqK61pYkHEAIQCw:
						if (m_nThreeStars >= 50)
						{
							UnlockAchievement(achievement);
						}
						break;

					// BULLSEYES/PERFECTS
					case Achievement.CgkIqK61pYkHEAIQFA:
						if (m_nBullseyes != 0)
						{
							UnlockAchievement(achievement);
						}
						break;
					case Achievement.CgkIqK61pYkHEAIQDA:
						if (m_nPerfects != 0)
						{
							UnlockAchievement(achievement);
						}
						break;
					case Achievement.CgkIqK61pYkHEAIQDQ:
						if (m_nPerfects >= 25)
						{
							UnlockAchievement(achievement);
						}
						break;
					case Achievement.CgkIqK61pYkHEAIQDg:
						if (m_nPerfects >= 50)
						{
							UnlockAchievement(achievement);
						}
						break;

					// PRECISION LEVELS BEAT
					case Achievement.CgkIqK61pYkHEAIQDw:
						if (m_nLevelsBeat != 0)
						{
							UnlockAchievement(achievement);
						}
						break;
					case Achievement.CgkIqK61pYkHEAIQEA:
						if (m_nLevelsBeat >= 10)
						{
							UnlockAchievement(achievement);
						}
						break;
					case Achievement.CgkIqK61pYkHEAIQEQ:
						if (m_nLevelsBeat >= 25)
						{
							UnlockAchievement(achievement);
						}
						break;
					case Achievement.CgkIqK61pYkHEAIQEg:
						if (m_nLevelsBeat >= 50)
						{
							UnlockAchievement(achievement);
						}
						break;

					// JEWELS
					case Achievement.CgkIqK61pYkHEAIQEw:
						if (m_nJewels != 0)
						{
							UnlockAchievement(achievement);
						}
						break;
				}
			}

			//Store stats in the Steam database if necessary
			if (m_bStoreStats)
			{
				// already set any achievements in UnlockAchievement

				// set stats
				SteamUserStats.SetStat("stat_points", m_nUserPoints);
				SteamUserStats.SetStat("stat_level", m_nUserLevel);
				SteamUserStats.SetStat("stat_xp", m_nUserXP);
				SteamUserStats.SetStat("stat_jewels", m_nJewels);
				SteamUserStats.SetStat("stat_arrows_fired", m_nArrowsFired);
				SteamUserStats.SetStat("stat_targets_hit", m_nTargetsHit);
				SteamUserStats.SetStat("stat_accuracy", m_flAccuracy);
				SteamUserStats.SetStat("stat_bullseyes", m_nBullseyes);
				SteamUserStats.SetStat("stat_stars", m_nStars);
				SteamUserStats.SetStat("stat_three_stars", m_nThreeStars);
				SteamUserStats.SetStat("stat_perfects", m_nPerfects);
				SteamUserStats.SetStat("stat_apples", m_nApplesShot);
				SteamUserStats.SetStat("stat_precision_levels_beat", m_nLevelsBeat);
				
				
				/*// Update average feet / second stat
				SteamUserStats.UpdateAvgRateStat("AverageSpeed", m_flGameFeetTraveled, m_flGameDurationSeconds);
				// The averaged result is calculated for us
				SteamUserStats.GetStat("AverageSpeed", out m_flAverageSpeed);*/

				bool bSuccess = SteamUserStats.StoreStats();
				// If this failed, we never sent anything to the server, try
				// again later.
				m_bStoreStats = !bSuccess;
			}
		}

		//-----------------------------------------------------------------------------
		// Purpose: Accumulate distance traveled
		//-----------------------------------------------------------------------------
		public void AddDistanceTraveled(float flDistance)
		{
			m_flGameFeetTraveled += flDistance;
		}



		//-----------------------------------------------------------------------------
		// Purpose: Unlock this achievement
		//-----------------------------------------------------------------------------
		private void UnlockAchievement(Achievement_t achievement)
		{
			achievement.m_bAchieved = true;

			// the icon may change once it's unlocked
			//achievement.m_iIconImage = 0;

			// mark it down
			SteamUserStats.SetAchievement(achievement.m_eAchievementID.ToString());

			// Store stats end of frame
			m_bStoreStats = true;
		}

		//-----------------------------------------------------------------------------
		// Purpose: We have stats data from Steam. It is authoritative, so update
		//			our data with those results now.
		//-----------------------------------------------------------------------------
		private void OnUserStatsReceived(UserStatsReceived_t pCallback)
		{
			if (!SteamManager.Initialized)
				return;

			// we may get callbacks for other games' stats arriving, ignore them
			if ((ulong)m_GameID == pCallback.m_nGameID)
			{
				if (EResult.k_EResultOK == pCallback.m_eResult)
				{
					Debug.Log("Received stats and achievements from Steam\n");

					m_bStatsValid = true;

					// load achievements
					foreach (Achievement_t ach in m_Achievements)
					{
						bool ret = SteamUserStats.GetAchievement(ach.m_eAchievementID.ToString(), out ach.m_bAchieved);
						if (ret)
						{
							ach.m_strName = SteamUserStats.GetAchievementDisplayAttribute(ach.m_eAchievementID.ToString(), "name");
							ach.m_strDescription = SteamUserStats.GetAchievementDisplayAttribute(ach.m_eAchievementID.ToString(), "desc");
						}
						else
						{
							Debug.LogWarning("SteamUserStats.GetAchievement failed for Achievement " + ach.m_eAchievementID + "\nIs it registered in the Steam Partner site?");
						}
					}

					// load stats
					SteamUserStats.GetStat("stat_points", out m_nUserPoints);
					SteamUserStats.GetStat("stat_level", out m_nUserLevel);
					SteamUserStats.GetStat("stat_xp", out m_nUserXP);
					SteamUserStats.GetStat("stat_jewels", out m_nJewels);
					SteamUserStats.GetStat("stat_arrows_fired", out m_nArrowsFired);
					SteamUserStats.GetStat("stat_targets_hit", out m_nTargetsHit);
					SteamUserStats.GetStat("stat_accuracy", out m_flAccuracy);
					SteamUserStats.GetStat("stat_bullseyes", out m_nBullseyes);
					SteamUserStats.GetStat("stat_stars", out m_nStars);
					SteamUserStats.GetStat("stat_three_stars", out m_nThreeStars);
					SteamUserStats.GetStat("stat_perfects", out m_nPerfects);
					SteamUserStats.GetStat("stat_apples", out m_nApplesShot);
					SteamUserStats.GetStat("stat_precision_levels_beat", out m_nLevelsBeat);
				}
				else
				{
					Debug.Log("RequestStats - failed, " + pCallback.m_eResult);
				}
			}
		}

		//-----------------------------------------------------------------------------
		// Purpose: Our stats data was stored!
		//-----------------------------------------------------------------------------
		private void OnUserStatsStored(UserStatsStored_t pCallback)
		{
			// we may get callbacks for other games' stats arriving, ignore them
			if ((ulong)m_GameID == pCallback.m_nGameID)
			{
				if (EResult.k_EResultOK == pCallback.m_eResult)
				{
					Debug.Log("StoreStats - success");
				}
				else if (EResult.k_EResultInvalidParam == pCallback.m_eResult)
				{
					// One or more stats we set broke a constraint. They've been reverted,
					// and we should re-iterate the values now to keep in sync.
					Debug.Log("StoreStats - some failed to validate");
					// Fake up a callback here so that we re-load the values.
					UserStatsReceived_t callback = new UserStatsReceived_t();
					callback.m_eResult = EResult.k_EResultOK;
					callback.m_nGameID = (ulong)m_GameID;
					OnUserStatsReceived(callback);
				}
				else
				{
					Debug.Log("StoreStats - failed, " + pCallback.m_eResult);
				}
			}
		}

		//-----------------------------------------------------------------------------
		// Purpose: An achievement was stored
		//-----------------------------------------------------------------------------
		private void OnAchievementStored(UserAchievementStored_t pCallback)
		{
			// We may get callbacks for other games' stats arriving, ignore them
			if ((ulong)m_GameID == pCallback.m_nGameID)
			{
				if (0 == pCallback.m_nMaxProgress)
				{
					Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' unlocked!");
				}
				else
				{
					Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' progress callback, (" + pCallback.m_nCurProgress + "," + pCallback.m_nMaxProgress + ")");
				}
			}
		}

		//-----------------------------------------------------------------------------
		// Purpose: Display the user's stats and achievements
		//-----------------------------------------------------------------------------
		public void Render()
		{
			if (!SteamManager.Initialized)
			{
				GUILayout.Label("Steamworks not Initialized");
				return;
			}

			GUILayout.Label("m_ulTickCountGameStart: " + m_ulTickCountGameStart);
			GUILayout.Label("m_flGameDurationSeconds: " + m_flGameDurationSeconds);
			GUILayout.Label("m_flGameFeetTraveled: " + m_flGameFeetTraveled);
			GUILayout.Space(10);
			GUILayout.Label("ApplesShot: " + m_nApplesShot);

			GUILayout.BeginArea(new Rect(Screen.width - 300, 0, 300, 800));
			foreach (Achievement_t ach in m_Achievements)
			{
				GUILayout.Label(ach.m_eAchievementID.ToString());
				GUILayout.Label(ach.m_strName + " - " + ach.m_strDescription);
				GUILayout.Label("Achieved: " + ach.m_bAchieved);
				GUILayout.Space(20);
			}

			// FOR TESTING PURPOSES ONLY!
			if (GUILayout.Button("RESET STATS AND ACHIEVEMENTS"))
			{
				SteamUserStats.ResetAllStats(true);
				SteamUserStats.RequestCurrentStats();
			}
			GUILayout.EndArea();
		}

		private class Achievement_t
		{
			public Achievement m_eAchievementID;
			public string m_strName;
			public string m_strDescription;
			public bool m_bAchieved;

			/// <summary>
			/// Creates an Achievement. You must also mirror the data provided here in https://partner.steamgames.com/apps/achievements/yourappid
			/// </summary>
			/// <param name="achievement">The "API Name Progress Stat" used to uniquely identify the achievement.</param>
			/// <param name="name">The "Display Name" that will be shown to players in game and on the Steam Community.</param>
			/// <param name="desc">The "Description" that will be shown to players in game and on the Steam Community.</param>
			public Achievement_t(Achievement achievementID, string name, string desc)
			{
				m_eAchievementID = achievementID;
				m_strName = name;
				m_strDescription = desc;
				m_bAchieved = false;
			}
		}
#endif
	}
}
