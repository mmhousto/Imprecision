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
        public GameObject cursor;

        private void Start()
        {
            cursor.SetActive(false);
            CheckIfBeatLevel();
        }

        private void Update()
        {
            if (xpGainedText.text != $"XP GAINED: {(Score.score / 10)}")
                xpGainedText.text = $"XP GAINED: {(Score.score / 10)}";

            CheckIfBeatLevel();
        }

        /// <summary>
        /// Checks if player has beat the level and IF they have, enable the next level button
        /// ELSE disables button.
        /// </summary>
        private void CheckIfBeatLevel()
        {
            if (Player.Instance.Levels[GameManager.Instance.LevelSelected] != 0 && nextLevelBtn.enabled == false)
            {
                nextLevelBtn.enabled = true;
            }
            else if(Player.Instance.Levels[GameManager.Instance.LevelSelected] == 0 && nextLevelBtn.enabled == true)
            {
                nextLevelBtn.enabled = false;
            }

        }

        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void ToMainMenu()
        {
            SceneManager.LoadScene(1);
        }

        public void LoadNextLevel()
        {
                GameManager.Instance.SetLevel(GameManager.Instance.LevelSelected + 1);
                RestartGame();
            
        }

    }
}
