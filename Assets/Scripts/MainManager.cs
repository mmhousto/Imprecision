using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Com.MorganHouston.Imprecision
{
    public class MainManager : MonoBehaviour
    {

        public GameObject appleLogoutScreen;
        public TextMeshProUGUI playButtonText;

        // Update is called once per frame
        void Update()
        {
            if (playButtonText.text != $"PLAY LEVEL {GameManager.Instance.LevelSelected + 1}")
            {
                playButtonText.text = $"PLAY LEVEL {GameManager.Instance.LevelSelected + 1}";
            }
        }

        public void Logout()
        {
            if (CloudSaveLogin.Instance.currentSSO == CloudSaveLogin.ssoOption.Apple)
            {
                appleLogoutScreen.SetActive(true);
            }
            else
            {
                CloudSaveLogin.Instance.Logout();
            }

                
        }

        public void SelectLevel(int levelToPlay)
        {
            GameManager.Instance.SetLevel(levelToPlay);
        }

        public void PlayGame()
        {
            SceneLoader.LoadThisScene(2);
        }

        public void SetPlayerName(string name)
        {
            Player.Instance.SetPlayerName(name);
        }
    }
}
