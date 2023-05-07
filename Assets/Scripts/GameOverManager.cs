using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

namespace Com.MorganHouston.Imprecision
{
    public class GameOverManager : MonoBehaviour
    {
        public Button nextLevelBtn;
        public TextMeshProUGUI xpGainedText;
        public MobileDisableAutoSwitchControls uiControls;
        private GameManager gameManager;
        private Player player;

        private void Start()
        {
            gameManager = GameManager.Instance;
            player = Player.Instance;

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

            if (gameManager != null && player != null)
                CheckIfBeatLevel();
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
                else if (player.StoryLevels[gameManager.LevelSelected] != 0 && nextLevelBtn.enabled == false)
                {
                    nextLevelBtn.enabled = true;
                }
                else if (player.StoryLevels[gameManager.LevelSelected] == 0 && nextLevelBtn.enabled == true)
                {
                    nextLevelBtn.gameObject.SetActive(false);
                }
            }
            else
            {
                if (player.Levels[gameManager.LevelSelected] != 0 && nextLevelBtn.enabled == false)
                {
                    nextLevelBtn.enabled = true;
                }
                else if (player.Levels[gameManager.LevelSelected] == 0 && nextLevelBtn.enabled == true)
                {
                    nextLevelBtn.gameObject.SetActive(false);
                }
            }
            

        }

        public void RestartGame()
        {
            SceneLoader.LoadThisScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void ToMainMenu()
        {
            SceneLoader.LoadThisScene(1);
        }

        public void LoadNextLevel()
        {
            gameManager.SetLevel(gameManager.LevelSelected + 1);
            RestartGame();
        }

        public void LoadStoryLevel(int sceneIndex)
        {
            gameManager.SetLevel(gameManager.LevelSelected + 1);
            SceneLoader.LoadThisScene(sceneIndex);
        }

    }
}
