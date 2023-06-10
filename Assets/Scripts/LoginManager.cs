using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace Com.MorganHouston.Imprecision
{

    public class LoginManager : MonoBehaviour
    {

        private static LoginManager instance;

        public static LoginManager Instance { get { return instance; } }

        public GameObject appleLogin, googleLogin, facebookLogin, signInPanel, mainMenuPanel, levelSelectButton, steamStatsAchieveLeader;

        public TextMeshProUGUI leaderboardsAchievementsLabel;

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
            }
        }

        // Start is called before the first frame update
        void Start()
        {
#if UNITY_ANDROID
            appleLogin.SetActive(false);
            googleLogin.SetActive(true);
#elif UNITY_IOS
            appleLogin.SetActive(true);
            googleLogin.SetActive(false);
#else
            appleLogin.SetActive(false);
            googleLogin.SetActive(false);
#endif

#if UNITY_WSA
            facebookLogin.SetActive(false);
#else
            facebookLogin.SetActive(true);
#endif

        }

        private void Update()
        {
            if(signInPanel.activeInHierarchy && CloudSaveLogin.Instance.loggedIn)
            {
                Login();
            }

            if (CloudSaveLogin.Instance.loggedIn && leaderboardsAchievementsLabel.text != "You Must be signed in to view leaderboards or achievements!" && CloudSaveLogin.Instance.currentSSO != CloudSaveLogin.ssoOption.Steam)
                leaderboardsAchievementsLabel.text = "You Must be signed in to view leaderboards or achievements!";
            else if (CloudSaveLogin.Instance.loggedIn && leaderboardsAchievementsLabel.text != "View Leaderboards & Achievements on the Steam Community Hub Page!" && CloudSaveLogin.Instance.currentSSO == CloudSaveLogin.ssoOption.Steam)
                leaderboardsAchievementsLabel.text = "View Leaderboards & Achievements on the Steam Community Hub Page!";
        }

        public void SignInAnonymously()
        {
            CloudSaveLogin.Instance.SignInAnonymously();
        }

        public void SignInSteam()
        {
            CloudSaveLogin.Instance.SignInWithSteam();
        }

        public void SignInFacebook()
        {
            CloudSaveLogin.Instance.SignInFacebook();
        }

        public void SignInApple()
        {
            CloudSaveLogin.Instance.SignInApple();
        }

        public void SignInGoogle()
        {
#if UNITY_ANDROID
            CloudSaveLogin.Instance.LoginGooglePlayGames();
#endif
        }

        public void QuitGame()
        {
            Application.Quit();
        }

        public void Login()
        {
            if (steamStatsAchieveLeader)
                steamStatsAchieveLeader.SetActive(true);

            signInPanel.SetActive(false);
            mainMenuPanel.SetActive(true);
            EventSystem.current.SetSelectedGameObject(levelSelectButton);
        }

    }
}
