using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Com.MorganHouston.Imprecision
{
    public class GameOverManager : MonoBehaviour
    {
        public Button nextLevelBtn;
        public TextMeshProUGUI xpGainedText;
        public TextMeshProUGUI totalTimeText;
        public MobileDisableAutoSwitchControls uiControls;
        public GameObject restartButton;
        public StoryManager storyManager;
        private GameManager gameManager;
        private Player player;
        int hours, minutes, seconds, milliseconds;

        private void Start()
        {
            gameManager = GameManager.Instance;
            player = Player.Instance;

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(restartButton);

            if(gameManager != null && player != null)
                CheckIfBeatLevel();

#if (UNITY_IOS || UNITY_ANDROID)
            uiControls.DisableScreenControls();
#endif

        }

        private void Update()
        {
            if (xpGainedText.text != $"XP GAINED: {(Score.score / 10)}")
                xpGainedText.text = $"XP GAINED: {(Score.score / 10)}";

            if(totalTimeText != null && totalTimeText.text != $"Total Time: {hours}:{minutes}:{seconds}.{milliseconds}")
            {
                float currentTime = storyManager.GetTotalTime();
                hours = Mathf.FloorToInt(currentTime / 3600f);
                minutes = Mathf.FloorToInt((currentTime - (hours * 3600f)) / 60f);
                seconds = Mathf.FloorToInt(currentTime - (hours * 3600f) - (minutes * 60f));
                milliseconds = Mathf.FloorToInt((currentTime - Mathf.Floor(currentTime)) * 1000f);

                totalTimeText.text = $"Total Time: {hours}:{minutes}:{seconds}.{milliseconds}";
            }

            if (gameManager != null && player != null)
                CheckIfBeatLevel();

            if(EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(restartButton);
            }

        }

        /// <summary>
        /// Checks if player has beat the level and IF they have, enable the next level button
        /// ELSE disables button.
        /// </summary>
        private void CheckIfBeatLevel()
        {
            if (gameManager.playingStoryMode)
            {
                if (gameManager.LevelSelected == 3)
                {
                    nextLevelBtn.gameObject.SetActive(false);
                }
                else if (storyManager.finishedLevel == false)
                {
                    nextLevelBtn.gameObject.SetActive(false);
                }
                else
                    nextLevelBtn.gameObject.SetActive(true);
            }
            else
            {
                if (player.Levels[gameManager.LevelSelected] != 0 && nextLevelBtn.enabled == false)
                {
                    nextLevelBtn.enabled = true;
                }
                else if ((player.Levels[gameManager.LevelSelected] == 0 || player.Levels[gameManager.LevelSelected] == 49) && nextLevelBtn.enabled == true)
                {
                    nextLevelBtn.gameObject.SetActive(false);
                }
            }
            

        }

        public void RestartGame()
        {
            Time.timeScale = 1;
            //SceneLoader.LoadThisScene(SceneManager.GetActiveScene().buildIndex);
            UnLoadLevel.Instance.LoadUnLoad(SceneManager.GetActiveScene().buildIndex);
        }

        public void RestartStoryGame()
        {
            Time.timeScale = 1;
            StoryManager.currentRunTimes.Clear();
            GameManager.Instance.SetLevel(0);
            SceneLoader.LoadThisScene(3);
            //UnLoadLevel.Instance.LoadUnLoad(3);
        }

        public void ToMainMenu()
        {
            Cursor.lockState = CursorLockMode.Confined;
            Time.timeScale = 1;
            //SceneLoader.LoadThisScene(1);
            UnLoadLevel.Instance.LoadUnLoad(1);
        }

        public void LoadNextLevel()
        {
            Time.timeScale = 1;
            gameManager.SetLevel(gameManager.LevelSelected + 1);
            RestartGame();
        }

        public void LoadStoryLevel(int sceneIndex)
        {
            Time.timeScale = 1;
            gameManager.SetLevel(gameManager.LevelSelected + 1);
            //SceneLoader.LoadThisScene(sceneIndex);
            UnLoadLevel.Instance.LoadUnLoad(sceneIndex);
        }

    }
}
