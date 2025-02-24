#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLESTEAMWORKS
#endif

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

        public GameObject appleLogin, googleLogin, /*facebookLogin,*/ autoLogin, signInPanel, mainMenuPanel, levelSelectButton, steamStatsAchieveLeader, anonymousButton, exitButton;

        public TextMeshProUGUI leaderboardsAchievementsLabel;

        private void Awake()
        {
                instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {
#if UNITY_ANDROID
            appleLogin.SetActive(false);
            googleLogin.SetActive(false);
#elif UNITY_IOS
            appleLogin.SetActive(true);
            googleLogin.SetActive(false);
#else
            appleLogin.SetActive(false);
            googleLogin.SetActive(false);
#endif

#if UNITY_WSA
            //facebookLogin.SetActive(false);
            autoLogin.SetActive(true);
#else
            //facebookLogin.SetActive(true);
#endif

#if DISABLESTEAMWORKS
            autoLogin.SetActive(true);
#else
            //facebookLogin.SetActive(false);
            autoLogin.SetActive(true);
#endif

#if UNITY_PS5
            exitButton.SetActive(false);
#endif

        }

        private void Update()
        {
            if(signInPanel.activeInHierarchy && CloudSaveLogin.Instance.loggedIn)
            {
                Login();
            }

            if (signInPanel.activeInHierarchy == false && EventSystem.current.currentSelectedGameObject == anonymousButton)
                EventSystem.current.SetSelectedGameObject(levelSelectButton);

            if (CloudSaveLogin.Instance.loggedIn && leaderboardsAchievementsLabel.text != "You Must be signed in to view leaderboards or achievements!" && CloudSaveLogin.Instance.currentSSO != CloudSaveLogin.ssoOption.Steam)
                leaderboardsAchievementsLabel.text = "You Must be signed in to view leaderboards or achievements!";
            else if (CloudSaveLogin.Instance.loggedIn && leaderboardsAchievementsLabel.text != "View Leaderboards & Achievements on the Steam Community Hub Page!" && CloudSaveLogin.Instance.currentSSO == CloudSaveLogin.ssoOption.Steam)
                leaderboardsAchievementsLabel.text = "View Leaderboards & Achievements on the Steam Community Hub Page!";
        }

        public void SignInAuto()
        {
#if UNITY_ANDROID
            SignInAnon();
#elif UNITY_IOS
            SignInApple();
#elif UNITY_PS5 && !UNITY_EDITOR
            CloudSaveLogin.Instance.PSSignIn();
#else
            if (CloudSaveLogin.Instance.isSteam && Application.internetReachability != NetworkReachability.NotReachable)
                SignInSteam();
            else
                SignInAnonymously();
#endif
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
            //CloudSaveLogin.Instance.SignInFacebook();
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
