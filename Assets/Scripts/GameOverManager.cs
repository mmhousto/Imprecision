using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace Com.MorganHouston.Imprecision
{
    public class GameOverManager : MonoBehaviour
    {

        public TextMeshProUGUI xpGainedText;

        private void Update()
        {
            if (xpGainedText.text != $"XP GAINED: {(Score.score / 10)}")
                xpGainedText.text = $"XP GAINED: {(Score.score / 10)}";
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
            GameManager.Instance.SetLevel(GameManager.Instance.LevelSelected+1);
            RestartGame();
        }

    }
}
