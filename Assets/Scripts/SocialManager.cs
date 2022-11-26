using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using GooglePlayGames;
using UnityEngine.SocialPlatforms;

namespace Com.MorganHouston.Imprecision
{
    public class SocialManager : MonoBehaviour
    {
        public GameObject signInScreen, okayButton;

        public void ShowLeaderboards()
        {
            if (CloudSaveLogin.Instance.currentSSO == CloudSaveLogin.ssoOption.Google || CloudSaveLogin.Instance.currentSSO == CloudSaveLogin.ssoOption.Apple)
                Social.ShowLeaderboardUI();
            else
            {
                signInScreen.SetActive(true);
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(okayButton);
            }
                
        }

        public void ShowAchievements()
        {
            if (CloudSaveLogin.Instance.currentSSO == CloudSaveLogin.ssoOption.Google || CloudSaveLogin.Instance.currentSSO == CloudSaveLogin.ssoOption.Apple)
                Social.ShowAchievementsUI();
            else
            {
                signInScreen.SetActive(true);
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(okayButton);
            }
        }
    }
}
