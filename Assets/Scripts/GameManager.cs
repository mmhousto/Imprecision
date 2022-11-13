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

        public int LevelSelected { get { return levelSelected; } private set { levelSelected = value; } }

        private GameObject gameOverScreen, player, restartButton;

        private GameObject[] stars;

        private TextMeshProUGUI gameOverText;

        public Material[] starMats;

        private int maxPointsForLevel, threeStars, twoStars, oneStar, perfection;

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

        private void LoadGame(Scene level, LoadSceneMode mode)
        {
            if(level.buildIndex == 2)
            {
                SetUpGame();
            }
        }

        private void SetUpGame()
        {
            GetGameOverComponents();
            GetMaxPointsForLevel();
        }

        private void GetGameOverComponents()
        {
            gameOverScreen = GameObject.FindWithTag("GameOver");
            gameOverText = gameOverScreen.GetComponentInChildren<TextMeshProUGUI>();
            stars = GameObject.FindGameObjectsWithTag("Star");
            gameOverScreen.transform.parent.gameObject.SetActive(false);
            player = GameObject.FindWithTag("Player");
        }

        private void GetMaxPointsForLevel()
        {
            perfection = (10 + (int)(levelSelected * 1.25f)) * 200;
            maxPointsForLevel = (10 + (int)(levelSelected * 1.25f)) * 100;
            threeStars = (int)(maxPointsForLevel * 0.9f);
            twoStars = (int)(maxPointsForLevel * 0.75f);
            oneStar = (int)(maxPointsForLevel * 0.5f);
        }

        public void GameOver()
        {
            gameOverScreen.transform.parent.gameObject.SetActive(true);
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(restartButton);

            player.GetComponent<PlayerInput>().actions = null; // Removes odd Player Input component error
            player.GetComponent<StarterAssets.StarterAssetsInputs>().SetCursorState(false);
            Destroy(player);

            DetermineStars();
        }

        private void DetermineStars()
        {
            var score = Score.score;
            if (score >= threeStars)
            {
                ActivateStars(3);
                gameOverText.text = "Perfection";
                Player.Instance.SetStarForLevel(levelSelected, 3);
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

            if (score >= perfection && Player.Instance.BullseyesOnLevels[levelSelected] != 1)
            {
                Player.Instance.SetBullseyeForLevel(levelSelected, 1);
                LeaderboardManager.CheckBullseyeAchievements();
            }

            CloudSaveLogin.Instance.SaveCloudData();
        }

        private void ActivateStars(int starsToActivate)
        {
            for(int i = 0; i < starsToActivate; i++)
            {
                stars[i].GetComponentInChildren<MeshRenderer>().material = starMats[1];
                stars[i].GetComponent<RotateThisObject>().enabled = true;
            }
        }

        public void SetLevel(int level)
        {
            levelSelected = level;
        }
    }
}
