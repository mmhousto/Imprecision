using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MorganHouston.Imprecision
{

    public class LoginManager : MonoBehaviour
    {

        public GameObject appleLogin, googleLogin;

        // Start is called before the first frame update
        void Start()
        {
#if UNITY_ANDROID
            appleLogin.SetActive(false);
            googleLogin.SetActive(true);
#elif UNITY_IOS
        appleLogin.SetActive(true);
        googleLogin.SetActive(false);
#endif
        }

        public void SignInAnonymously()
        {
            CloudSaveLogin.Instance.SignInAnonymously();
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

    }
}
