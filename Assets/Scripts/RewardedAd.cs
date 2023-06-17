using System;
using UnityEngine;
using Unity.Services.Core;
using UnityEngine.Advertisements;
using UnityEngine.UI;
using TMPro;

namespace Com.MorganHouston.Imprecision
{
    
    public class RewardedAd : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
    {
        public Button button;
        public TextMeshProUGUI buttonText;

        private bool adLoaded = false;
        private bool buttonEnabled;
        private DateTime pressedTime, overTime;
        string _adUnitId = null;
        string _adUnitIdAndroid = "Rewarded_Android";
        string _adUnitIdIos = "Rewarded_iOS";
        int tries = 0;

        void Awake()
        {
            // Get the Ad Unit ID for the current platform:
#if UNITY_IOS
        _adUnitId = _adUnitIdIos;
#elif UNITY_ANDROID
            _adUnitId = _adUnitIdAndroid;
#endif

            // Disable the button until the ad is ready to show:
            button.interactable = false;

            LoadAd();
        }

        private void Start()
        {

            overTime = Convert.ToDateTime(Player.Instance.FreeJewelOvertime);
        }

        private void Update()
        {
            if(overTime <= DateTime.Now && adLoaded)
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

        // Call this public method when you want to get an ad ready to show.
        public void LoadAd()
        {
            // IMPORTANT! Only load content AFTER initialization (in this example, initialization is handled in a different script).
            //Debug.Log("Loading Ad: " + _adUnitId);
            if(Advertisement.isInitialized)
                Advertisement.Load(_adUnitId, this);
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

        /*
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
        }*/

        public void ShowAd()
        {
            // Ensure the ad has loaded, then show it.
            if (adLoaded == true)
            {
                button.interactable = false;
                Advertisement.Show(_adUnitId, this);
            } else
            {
                LoadAd();
            }
        }

        /*
        void ImpressionEvent(object sender, ImpressionEventArgs args)
        {
            var impressionData = args.ImpressionData != null ? JsonUtility.ToJson(args.ImpressionData, true) : "null";
        }*/

        public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
        {
            
        }

        public void OnUnityAdsShowStart(string placementId)
        {
            
        }

        public void OnUnityAdsShowClick(string placementId)
        {
            
        }

        public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
        {
            if (placementId.Equals(_adUnitId) && showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
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

                Player.Instance.SetFreeJewelOvertime(overTime.ToString());

                // Execute logic for rewarding the user.
                Player.Instance.GainJewels(1);
            }
        }

        public void OnUnityAdsAdLoaded(string placementId)
        {
            if (placementId.Equals(_adUnitId))
            {
                // Configure the button to call the ShowAd() method when clicked:
                //_showAdButton.onClick.AddListener(ShowAd);

                // Enable the button for users to click:
                adLoaded = true;
                button.interactable = true;
            }
        }

        public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
        {
            tries++;
            if(tries < 4)
            {
                LoadAd();
            }
        }
    }
}