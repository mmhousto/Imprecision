using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

namespace Com.MorganHouston.Imprecision
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager instance;

        public static GameManager Instance { get { return instance; } }

        [SerializeField]
        private int levelSelected = 0; 

        public int LevelSelected { get { return levelSelected; } private set { levelSelected = value; } } // The Precision Level User has selected in Main Menu/From Game Using Next Level Button

        private GameObject gameOverScreen, player, restartButton;

        private GameObject[] stars; // Stars at end of game to show player how well they did

        private TextMeshProUGUI gameOverText; // Text displayed on game over

        public Material[] starMats; // Enabled and Disabled materials for stars

        private int maxPointsForLevel, threeStars, twoStars, oneStar, perfection; // points for level

        public bool playingStoryMode;

        public bool isGameOver;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                instance = this;
                DontDestroyOnLoad(Instance.gameObject);
            }
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += LoadGame;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= LoadGame;
        }

        /// <summary>
        /// If game scene, sets up the game
        /// </summary>
        /// <param name="level"></param>
        /// <param name="mode"></param>
        private void LoadGame(Scene level, LoadSceneMode mode)
        {
            if(level.buildIndex == 1)
            {
                playingStoryMode = false;
                SetUpGame();
            }else if (level.buildIndex >= 2)
            {
                playingStoryMode = true;
                SetUpGame();
            }
        }

        /// <summary>
        /// Called when level is loaded to set star rating and get game over components
        /// </summary>
        private void SetUpGame()
        {
            GetGameOverComponents();
            GetMaxPointsForLevel();
            isGameOver = false;
        }

        /// <summary>
        /// Gets all Game Over Components, self explanatory lol
        /// </summary>
        private void GetGameOverComponents()
        {
            gameOverScreen = GameObject.FindWithTag("GameOver");
            gameOverText = gameOverScreen.GetComponentInChildren<TextMeshProUGUI>();
            stars = GameObject.FindGameObjectsWithTag("Star");
            gameOverScreen.transform.parent.gameObject.SetActive(false);
            player = GameObject.FindWithTag("Player");
        }

        /// <summary>
        /// Determines max points possible for Perfection, 3 star, 2 star, and 1 star
        /// </summary>
        private void GetMaxPointsForLevel()
        {
            if (playingStoryMode)
            {
                threeStars = 3;
                twoStars = 2;
                oneStar = 1;
            }
            else
            {
                perfection = (10 + (int)(levelSelected * 1.25f)) * 200;
                maxPointsForLevel = (10 + (int)(levelSelected * 1.25f)) * 100;
                threeStars = (int)(maxPointsForLevel * 0.9f);
                twoStars = (int)(maxPointsForLevel * 0.75f);
                oneStar = (int)(maxPointsForLevel * 0.5f);
            }
            
        }

        /// <summary>
        /// Enables GameOver Screen and Calls Method to Determine stars and save/update player data
        /// </summary>
        public void GameOver()
        {
            isGameOver = true;
            gameOverScreen.transform.parent.gameObject.SetActive(true);
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(restartButton);

            player.GetComponent<PlayerInput>().actions = null; // Removes odd Player Input component error
            player.GetComponent<StarterAssets.StarterAssetsInputs>().SetCursorState(false);
            Destroy(player);

            if (playingStoryMode)
                DetermineStars(StoryManager.Instance.GetStarRating());
            else
                DetermineStars();
        }

        /// <summary>
        /// Determines Amount of Stars recieved based on Precision level, checks achievements, and updates player stats
        /// </summary>
        private void DetermineStars()
        {
            var score = Score.score;
            if (score >= threeStars)
            {
                ActivateStars(3);
                gameOverText.text = "Perfection";
                Player.Instance.SetStarForLevel(levelSelected, 3);

#if (UNITY_IOS || UNITY_ANDROID)
                LeaderboardManager.CheckPerfectAchievements();
#endif
            }
            else if (score >= twoStars)
            {
                ActivateStars(2);
                gameOverText.text = "Excellent";
                Player.Instance.SetStarForLevel(levelSelected, 2);
            }
            else if (score >= oneStar)
            {
                ActivateStars(1);
                gameOverText.text = "Good";
                Player.Instance.SetStarForLevel(levelSelected, 1);
            }
            else
            {
                gameOverText.text = "Try Again";
                Player.Instance.SetStarForLevel(levelSelected, 0);
            }
            Player.Instance.GainPoints(score);

            // If Perfect, updates user stats and checks/updates progress on bullseye achievements
            if (score >= perfection && Player.Instance.BullseyesOnLevels[levelSelected] != 1)
            {
                Player.Instance.SetBullseyeForLevel(levelSelected, 1);

#if (UNITY_IOS || UNITY_ANDROID)
                LeaderboardManager.CheckBullseyeAchievements();
#endif
            }

            // Save data to cloud/local
            CloudSaveLogin.Instance.SaveCloudData();

            // Update Leaderboards
#if (UNITY_IOS || UNITY_ANDROID)
            LeaderboardManager.UpdateAllLeaderboards();
#endif
        }

        /// <summary>
        /// Determines Amount of Stars recieved based on Story level, checks achievements, and updates player stats
        /// </summary>
        private void DetermineStars(int starsToShow)
        {
            var score = Score.score;

            switch (starsToShow)
            {
                case 3:
                    ActivateStars(3);
                    gameOverText.text = "Perfection";
                    Player.Instance.SetStarForStoryLevel(levelSelected, 3);
                    break;
                case 2:
                    ActivateStars(2);
                    gameOverText.text = "Excellent";
                    Player.Instance.SetStarForStoryLevel(levelSelected, 2);
                    break;
                case 1:
                    ActivateStars(1);
                    gameOverText.text = "Good";
                    Player.Instance.SetStarForStoryLevel(levelSelected, 1);
                    break;
                case 0:
                    gameOverText.text = "Try Again";
                    Player.Instance.SetStarForStoryLevel(levelSelected, 0);
                    break;
                default:
                    break;
            }
            
            Player.Instance.GainPoints(score);

            // Save data to cloud/local
            CloudSaveLogin.Instance.SaveCloudData();

            // Update Leaderboards
#if (UNITY_IOS || UNITY_ANDROID)
            LeaderboardManager.UpdateAllLeaderboards();
#endif
        }


        /// <summary>
        /// Shows how many stars the player recieved on the level
        /// </summary>
        /// <param name="starsToActivate"># of Stars to Activate</param>
        private void ActivateStars(int starsToActivate)
        {
            for(int i = 0; i < starsToActivate; i++)
            {
                stars[i].GetComponentInChildren<MeshRenderer>().material = starMats[1];
                stars[i].GetComponent<RotateThisObject>().enabled = true;
            }
        }

        /// <summary>
        /// Sets precision level to the level user selected
        /// </summary>
        /// <param name="level">The Selected Level</param>
        public void SetLevel(int level)
        {
            levelSelected = level;
        }
    }
}
