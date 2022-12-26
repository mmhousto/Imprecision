using System;
using UnityEngine;
//using Unity.Services.Core;
//using Unity.Services.Mediation;
//using UnityEngine.UI;
//using TMPro;

namespace Com.MorganHouston.Imprecision
{
    /*
    public class RewardedAd : MonoBehaviour
    {
        public string androidAdUnitId;
        public string iosAdUnitId;
        public Button button;
        public TextMeshProUGUI buttonText;
        private bool buttonEnabled;
        private DateTime pressedTime, overTime;
        string gameIdIos = "4689670";
        string gameIdAndroid = "4689671";
        string adUnitId;
        string adUnitIdAndroid = "Rewarded_Android";
        string adUnitIdIos = "Rewarded_iOS";
        IRewardedAd rewardedAd;

        private void Start()
        {
            switch (CloudSaveLogin.Instance.currentSSO)
            {
                case CloudSaveLogin.ssoOption.Anonymous:
                    overTime = Convert.ToDateTime(PlayerPrefs.GetString("FreeJewelOverTime", DateTime.Now.AddHours(-24).ToString()));
                    break;
                case CloudSaveLogin.ssoOption.Facebook:
                    overTime = Convert.ToDateTime(PlayerPrefs.GetString("FreeJewelOverTimeFB", DateTime.Now.AddHours(-24).ToString()));
                    break;
                case CloudSaveLogin.ssoOption.Google:
                    overTime = Convert.ToDateTime(PlayerPrefs.GetString("FreeJewelOverTimeG", DateTime.Now.AddHours(-24).ToString()));
                    break;
                case CloudSaveLogin.ssoOption.Apple:
                    overTime = Convert.ToDateTime(PlayerPrefs.GetString("FreeJewelOverTimeA", DateTime.Now.AddHours(-24).ToString()));
                    break;
                default:
                    overTime = Convert.ToDateTime(PlayerPrefs.GetString("FreeJewelOverTime", DateTime.Now.AddHours(-24).ToString()));
                    break;
            }
        }

        private void Update()
        {
            if(overTime <= DateTime.Now)
            {
                buttonEnabled = true;
            }
            else
            {
                buttonEnabled = false;
            }

            button.interactable = buttonEnabled;

            HandleButtonText();
        }

        private void HandleButtonText()
        {
            if(buttonEnabled == true)
            {
                buttonText.text = "FREE JEWEL";
            }
            else
            {
                buttonText.text = $"{overTime - DateTime.Now}";
            }
        }

        public async void InitServices()
        {
            if (ServicesInitializationState.Initialized == UnityServices.State)
            {
                try
                {
                    InitializationOptions initializationOptions = new InitializationOptions();
                    // Instantiate a rewarded ad object with platform-specific Ad Unit ID
                    if (Application.platform == RuntimePlatform.Android)
                    {
                        initializationOptions.SetGameId(gameIdAndroid);
                        adUnitId = adUnitIdAndroid;
                        await UnityServices.InitializeAsync(initializationOptions);
                    }
                    else if (Application.platform == RuntimePlatform.IPhonePlayer)
                    {
                        initializationOptions.SetGameId(gameIdIos);
                        adUnitId = adUnitIdIos;
                        await UnityServices.InitializeAsync(initializationOptions);
                    }
#if UNITY_EDITOR
                    else
                    {
                        adUnitId = "myExampleAdUnitId";
                        await UnityServices.InitializeAsync();
                    }
#endif

                    InitializationComplete();
                }
                catch (Exception e)
                {
                    InitializationFailed(e);
                }
            }
            else
            {
                // Services are initialized
                if (rewardedAd == null)
                {
                    InitializationComplete();
                }
                else
                {
                    ShowAd();
                }
            }
        }

        void SetupAd()
        {
            // Creates Ad
            rewardedAd = MediationService.Instance.CreateRewardedAd(adUnitId);

            // Subscribe callback methods to load events:
            rewardedAd.OnLoaded += AdLoaded;
            rewardedAd.OnFailedLoad += AdFailedToLoad;

            // Subscribe callback methods to show events:
            rewardedAd.OnShowed += AdShown;
            rewardedAd.OnFailedShow += AdFailedToShow;
            rewardedAd.OnUserRewarded += UserRewarded;
            rewardedAd.OnClosed += AdClosed;

            // Impression Event
            MediationService.Instance.ImpressionEventPublisher.OnImpression += ImpressionEvent;
        }

        void InitializationComplete()
        {
            SetupAd();
            rewardedAd.LoadAsync();
        }

        void InitializationFailed(Exception e)
        {

        }

        // Implement load event callback methods:
        void AdLoaded(object sender, EventArgs args)
        {
            ShowAd();
        }

        void AdFailedToLoad(object sender, LoadErrorEventArgs args)
        {
            // Execute logic for the ad failing to load.
        }

        // Implement show event callback methods:
        void AdShown(object sender, EventArgs args)
        {
            // Execute logic for the ad showing successfully.
        }

        void UserRewarded(object sender, RewardEventArgs args)
        {
            buttonEnabled = false;
            pressedTime = DateTime.Now;
            overTime = pressedTime.AddHours(24);

            switch (CloudSaveLogin.Instance.currentSSO)
            {
                case CloudSaveLogin.ssoOption.Anonymous:
                    PlayerPrefs.SetString("FreeJewelOverTime", overTime.ToString());
                    break;
                case CloudSaveLogin.ssoOption.Facebook:
                    PlayerPrefs.SetString("FreeJewelOverTimeFB", overTime.ToString());
                    break;
                case CloudSaveLogin.ssoOption.Google:
                    PlayerPrefs.SetString("FreeJewelOverTimeG", overTime.ToString());
                    break;
                case CloudSaveLogin.ssoOption.Apple:
                    PlayerPrefs.SetString("FreeJewelOverTimeA", overTime.ToString());
                    break;
                default:
                    PlayerPrefs.SetString("FreeJewelOverTime", overTime.ToString());
                    break;
            }

            // Execute logic for rewarding the user.
            Player.Instance.GainJewels(1);
        }

        void AdFailedToShow(object sender, ShowErrorEventArgs args)
        {
            // Execute logic for the ad failing to show.
        }

        void AdClosed(object sender, EventArgs e)
        {
            // Execute logic for the user closing the ad.
        }

        public void ShowAd()
        {
            // Ensure the ad has loaded, then show it.
            if (rewardedAd.AdState == AdState.Loaded)
            {
                rewardedAd.ShowAsync();
            } else if (rewardedAd.AdState == AdState.Unloaded)
            {
                rewardedAd.LoadAsync();
            }
        }

        void ImpressionEvent(object sender, ImpressionEventArgs args)
        {
            var impressionData = args.ImpressionData != null ? JsonUtility.ToJson(args.ImpressionData, true) : "null";
        }
    }*/
}