using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using System.Threading.Tasks;
using Unity.Services.Core;
using Facebook.Unity;
using System;
using AppleAuth;
using AppleAuth.Native;
using AppleAuth.Enums;
using AppleAuth.Extensions;
using AppleAuth.Interfaces;
using System.Text;
using GooglePlayGames.BasicApi;
using GooglePlayGames;

namespace Com.MorganHouston.Imprecision
{

    public class CloudSaveLogin : MonoBehaviour
    {

        #region Fields/Variables

        private static CloudSaveLogin instance;

        public static CloudSaveLogin Instance { get { return instance; } }

        // What SSO Option the use is using atm.
        public enum ssoOption { Anonymous, Facebook, Google, Apple }

        // Player Data Object
        private Player player;

        public ssoOption currentSSO = ssoOption.Anonymous;

        private IAppleAuthManager appleAuthManager;

        private bool triedQuickLogin = false;

        public bool devModeActivated = false;

        // User Info.
        public string userName, userID;


        #endregion


        #region MonoBehaviour Methods


        // Start is called before the first frame update
        async void Awake()
        {
            if(instance != null && instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            else
            {
                instance = this;
                DontDestroyOnLoad(Instance.gameObject);
            }

            if (UnityServices.State == ServicesInitializationState.Initialized)
            {

            }
            else
            {
                await UnityServices.InitializeAsync();
            }


            if (!FB.IsInitialized)
            {
                // Initialize the Facebook SDK
                FB.Init(InitCallback, OnHideUnity);
            }
            else
            {
                // Already initialized, signal an app activation App Event
                FB.ActivateApp();
            }

#if UNITY_ANDROID
            // Initializes Google Play Games Login
            InitializePlayGamesLogin();
#endif
            try
            {
                player = GetComponent<Player>();
            }
            catch
            {

            }
        }

        private void Start()
        {
            // If the current platform is supported initialize apple authentication.
            if (AppleAuthManager.IsCurrentPlatformSupported)
            {
                // Creates a default JSON deserializer, to transform JSON Native responses to C# instances
                IPayloadDeserializer deserializer = new PayloadDeserializer();
                // Creates an Apple Authentication manager with the deserializer
                appleAuthManager = new AppleAuthManager(deserializer);
            }
        }

        void Update()
        {
            // Updates the AppleAuthManager instance to execute
            // pending callbacks inside Unity's execution loop
            if (appleAuthManager != null)
            {
                appleAuthManager.Update();
            }

            // Tries to quick login on Apple, if user previously logged in.
            if (triedQuickLogin == false && appleAuthManager != null)
            {
                GetCredentialState();
                triedQuickLogin = true;
            }


        }

        /// <summary>
        /// Saves data on application exit.
        /// </summary>
        private void OnApplicationQuit()
        {
            SaveCloudData();

        }

        /// <summary>
        /// Saves data to cloud on application pause or swipe out.
        /// </summary>
        /// <param name="pause"></param>
        private void OnApplicationPause(bool pause)
        {
#if (UNITY_IOS || UNITY_ANDROID)
        if(pause)
            SaveCloudData();
#endif
        }


        #endregion


        #region Public Sign In/Out Methods


        /// <summary>
        /// Signs user into an anonymous account.
        /// </summary>
        public async void SignInAnonymously()
        {
            currentSSO = ssoOption.Anonymous;
            AuthenticationService.Instance.SwitchProfile("default");
            await SignInAnonymouslyAsync();

        }

        /// <summary>
        /// Signs user into dev account.
        /// </summary>
        public async void SignInDeveloper()
        {
            devModeActivated = true;
            await SignInAnonymouslyAsync();
        }

        /// <summary>
        /// Signs user into facebook account with authentication from Facebook.
        /// </summary>
        public void SignInFacebook()
        {
            currentSSO = ssoOption.Facebook;
            AuthenticationService.Instance.SwitchProfile("facebook");

#if UNITY_ANDROID
        FB.Android.RetrieveLoginStatus(LoginStatusCallback);
#else
            var perms = new List<string>() { "public_profile" };
            FB.LogInWithReadPermissions(perms, AuthCallback);
#endif

        }

        /// <summary>
        /// Signs user into Apple with Auth from Apple.
        /// </summary>
        public async void SignInApple()
        {
            currentSSO = ssoOption.Apple;
            if (!AuthenticationService.Instance.IsSignedIn)
                AuthenticationService.Instance.SwitchProfile("apple");

            var idToken = await GetAppleIdTokenAsync();

            await AuthenticationService.Instance.SignInWithAppleAsync(idToken);

            SetPlayerData(AuthenticationService.Instance.PlayerId, userName);

            Login();

        }

        /// <summary>
        /// Saves player data to cloud if user is signed in.
        /// </summary>
        public async void SaveCloudData()
        {
            if (AuthenticationService.Instance.IsSignedIn)
            {
                SavePlayerData data = new SavePlayerData(player);
                await ForceSaveObjectData(player.UserID, data);
            }
        }

        /// <summary>
        /// Logs out the user, unless logged in with Apple account will display message.
        /// </summary>
        public void Logout()
        {
                SaveLogout();

                LogoutScreenActivate();

        }

        /// <summary>
        /// Developer sign out option to delete all player data and sign out.
        /// </summary>
        public async void DevSignOut()
        {

            await DeleteEverythingSignOut();
        }

        /// <summary>
        /// Logs out of facebook.
        /// </summary>
        public void FacebookLogout()
        {
            FB.LogOut();
        }


        #endregion


        #region Private Login/Logout Methods

        /// <summary>
        /// Loads the Main Menu Scene.
        /// </summary>
        private void Login()
        {
            SceneLoader.LoadThisScene(1);
        }

        /// <summary>
        /// Loads the Sign-In Scene.
        /// </summary>
        private void LogoutScreenActivate()
        {
            SceneLoader.LoadThisScene(0);
        }

        /// <summary>
        /// Saves and logs out of user and resets player data.
        /// </summary>
        private void SaveLogout()
        {
            SaveCloudData();

            if (FB.IsLoggedIn)
            {
                FacebookLogout();
            }

            if (currentSSO == ssoOption.Google)
            {
                GoogleLogout();
            }

            if (AuthenticationService.Instance.IsSignedIn)
            {
                AuthenticationService.Instance.SignOut();
            }

            ResetPlayerData();
        }


        #endregion


        #region Apple Auth

        /// <summary>
        /// Performs continue with Apple login.
        /// </summary>
        public async void QuickLoginApple()
        {
            Debug.Log("Quick Login Apple Called");
            if (appleAuthManager == null) return;

            currentSSO = ssoOption.Apple;
            if (!AuthenticationService.Instance.IsSignedIn)
                AuthenticationService.Instance.SwitchProfile("apple");

            var quickLoginArgs = new AppleAuthQuickLoginArgs();

            this.appleAuthManager.QuickLogin(
                quickLoginArgs,
                credential =>
                {
                // Received a valid credential!
                // Try casting to IAppleIDCredential or IPasswordCredential

                // Previous Apple sign in credential
                var appleIdCredential = credential as IAppleIDCredential;

                // Saved Keychain credential (read about Keychain Items)
                var passwordCredential = credential as IPasswordCredential;

                    if (appleIdCredential != null)
                    {
                        userID = PlayerPrefs.GetString("AppleUserIdKey", appleIdCredential.User);
                        userName = PlayerPrefs.GetString("AppleUserNameKey", appleIdCredential.FullName.GivenName);

                    }

                },
                error =>
                {
                    Debug.Log("Quick Login Apple Failed");
                    return;
                // Quick login failed. The user has never used Sign in With Apple on your app. Go to login screen
            });

            var idToken = PlayerPrefs.GetString("AppleTokenIdKey");

            await AuthenticationService.Instance.SignInWithAppleAsync(idToken);

            SetPlayerData(AuthenticationService.Instance.PlayerId, userName);
            Login();
        }

        /// <summary>
        /// Checks if user has logged in with apple before on device, if so continues to quick login.
        /// </summary>
        public void GetCredentialState()
        {
            userID = PlayerPrefs.GetString("AppleUserIdKey");
            this.appleAuthManager.GetCredentialState(
                        userID,
                        state =>
                        {
                            switch (state)
                            {
                                case CredentialState.Authorized:
                                // User ID is still valid. Login the user.
                                Debug.Log("User ID is valid!");
                                    QuickLoginApple();
                                    break;

                                case CredentialState.Revoked:
                                // User ID was revoked. Go to login screen.
                                Debug.Log("User ID was revoked.");
                                    if (AuthenticationService.Instance.IsSignedIn)
                                    {
                                        AuthenticationService.Instance.SignOut();
                                    }
                                    break;

                                case CredentialState.NotFound:
                                // User ID was not found. Go to login screen.
                                Debug.Log("User ID was not found.");
                                    break;
                            }
                        },
                        error =>
                        {
                        // Something went wrong
                        Debug.Log("Credential Failed");
                            if (AuthenticationService.Instance.IsSignedIn)
                            {
                                AuthenticationService.Instance.SignOut();
                            }
                            return;
                        });
        }


        /// <summary>
        /// Gets the Apple Identity Token to Authenticate the user.
        /// Stores username, userID, Identity Token and email in Player Prefs.
        /// Returns the Identity Token.
        /// </summary>
        /// <returns></returns>
        private Task<string> GetAppleIdTokenAsync()
        {
            var tcs = new TaskCompletionSource<string>();

            if (appleAuthManager == null) return null;

            var loginArgs = new AppleAuthLoginArgs(LoginOptions.IncludeFullName);

            this.appleAuthManager.LoginWithAppleId(
                loginArgs,
                credential =>
                {
                //      Obtained credential, cast it to IAppleIDCredential
                var appleIdCredential = credential as IAppleIDCredential;
                    if (appleIdCredential != null)
                    {
                    // Apple User ID
                    // You should save the user ID somewhere in the device
                    userID = appleIdCredential.User;
                        PlayerPrefs.SetString("AppleUserIdKey", userID);

                    // Email (Received ONLY in the first login)
                    /*email = appleIdCredential.Email;
                        PlayerPrefs.SetString("AppleUserEmailKey", email);*/

                    // Full name (Received ONLY in the first login)
                    userName = appleIdCredential.FullName.GivenName;
                        PlayerPrefs.SetString("AppleUserNameKey", userName);

                    // Identity token
                    var idToken = Encoding.UTF8.GetString(
                            appleIdCredential.IdentityToken,
                            0,
                            appleIdCredential.IdentityToken.Length);

                        tcs.SetResult(idToken);

                        PlayerPrefs.SetString("AppleTokenIdKey", idToken);

                    // Authorization code
                    var AuthCode = Encoding.UTF8.GetString(
                                    appleIdCredential.AuthorizationCode,
                                    0,
                                    appleIdCredential.AuthorizationCode.Length);

                    // And now you have all the information to create/login a user in your system

                }
                    else
                    {
                        tcs.SetException(new Exception("Retrieving Apple Id Token failed."));
                    }
                },
                error =>
                {
                // Something went wrong
                tcs.SetException(new Exception("Retrieving Apple Id Token failed."));
                    var authorizationErrorCode = error.GetAuthorizationErrorCode();
                    return;
                });

            return tcs.Task;

        }


        #endregion


        #region Facebook Auth

        /// <summary>
        /// Initializes Facebook SDK
        /// </summary>
        private void InitCallback()
        {
            if (FB.IsInitialized)
            {
                // Signal an app activation App Event
                FB.ActivateApp();
                // Continue with Facebook SDK
                // ...
            }
            else
            {
                Debug.Log("Failed to Initialize the Facebook SDK");
            }
        }

        private void OnHideUnity(bool isGameShown)
        {
            if (!isGameShown)
            {
                // Pause the game - we will need to hide
                Time.timeScale = 0;
            }
            else
            {
                // Resume the game - we're getting focus again
                Time.timeScale = 1;
            }
        }

        /// <summary>
        /// Callback to get player info on login.
        /// </summary>
        /// <param name="result"></param>
        private async void AuthCallback(ILoginResult result)
        {
            if (FB.IsLoggedIn)
            {
                // AccessToken class will have session details
                var aToken = AccessToken.CurrentAccessToken;
                // Print current access token's User ID
                Debug.Log(aToken.UserId);
                userID = aToken.UserId;

                FB.API("me?fields=id,name", HttpMethod.GET, AssignInfo);



                await SignInWithFacebookAsync(aToken.TokenString);

            }
            else
            {
                Debug.Log("User cancelled login");
            }
        }

        /// <summary>
        /// Assigns player info on login.
        /// </summary>
        /// <param name="result"></param>
        void AssignInfo(IGraphResult result)
        {
            if (result.Error != null)
            {
                Debug.Log("Error: " + result.Error);
            }
            else if (!FB.IsLoggedIn)
                Debug.Log("Login Canceled By Player");
            else
            {
                userID = result.ResultDictionary["id"].ToString();
                userName = result.ResultDictionary["name"].ToString();
            }
        }

        /// <summary>
        /// Signs the player into Unity Services and sets player data.
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        async Task SignInWithFacebookAsync(string accessToken)
        {
            try
            {
                await AuthenticationService.Instance.SignInWithFacebookAsync(accessToken);
                Debug.Log("Sign-In With Facebook is successful.");

                SetPlayerData(AuthenticationService.Instance.PlayerId, userName);

                Login();
            }
            catch (AuthenticationException ex)
            {
                // Compare error code to AuthenticationErrorCodes
                // Notify the player with the proper error message
                Debug.LogException(ex);
            }
            catch (RequestFailedException ex)
            {
                // Compare error code to CommonErrorCodes
                // Notify the player with the proper error message
                Debug.LogException(ex);
            }
        }

        /// <summary>
        /// Logs the player in, if they were logged in before.
        /// </summary>
        /// <param name="result"></param>
        private async void LoginStatusCallback(ILoginStatusResult result)
        {
            if (!string.IsNullOrEmpty(result.Error))
            {
                Debug.Log("Error: " + result.Error);

                var perms = new List<string>() { "public_profile" };
                FB.LogInWithReadPermissions(perms, AuthCallback);
            }
            else if (result.Failed)
            {
                Debug.Log("Failure: Access Token could not be retrieved");

                var perms = new List<string>() { "public_profile" };
                FB.LogInWithReadPermissions(perms, AuthCallback);
            }
            else
            {
                // Successfully logged user in
                // A popup notification will appear that says "Logged in as <User Name>"
                Debug.Log("Success: " + result.AccessToken.UserId);

                await SignInWithFacebookAsync(result.AccessToken.TokenString);

            }
        }


        #endregion


        #region Google Play Auth


        void InitializePlayGamesLogin()
        {
            var config = new PlayGamesClientConfiguration.Builder()
                // Requests an ID token be generated.  
                // This OAuth token can be used to
                // identify the player to other services such as Firebase.
                .RequestIdToken()
                .Build();

            PlayGamesPlatform.InitializeInstance(config);
            PlayGamesPlatform.Activate();
        }

        public void LoginGooglePlayGames()
        {
            currentSSO = ssoOption.Google;
            AuthenticationService.Instance.SwitchProfile("google");
            try
            {
                PlayGamesPlatform.Instance.Authenticate(SignInInteractivity.CanPromptOnce, success => { OnGooglePlayGamesLogin(success); });
            }
            catch (Exception e)
            {
                Debug.Log("ERROR: " + e);
            }
            
        }

        async void OnGooglePlayGamesLogin(SignInStatus status)
        {
            if (status == SignInStatus.Success)
            {
                ((PlayGamesPlatform)Social.Active).SetGravityForPopups(Gravity.BOTTOM);

                // Call Unity Authentication SDK to sign in or link with Google.
                var idToken = ((PlayGamesLocalUser)Social.localUser).GetIdToken();
                Debug.Log("Login with Google Play Games done. IdToken: " + idToken);
                userID = Social.localUser.id;
                userName = Social.localUser.userName;

                await SignInWithGoogleAsync(idToken);
                Debug.Log("Sign-In With Google is successful.");

            }
            else if (status == SignInStatus.UiSignInRequired)
            {
                PlayGamesPlatform.Instance.Authenticate(SignInInteractivity.CanPromptAlways, success => { OnGooglePlayGamesLogin(success); });
            }
            else
            {
                Debug.Log("Unsuccessful login");
            }
        }

        async Task SignInWithGoogleAsync(string idToken)
        {
            try
            {
                Debug.Log("Authenticating with id Token: " + idToken);
                await AuthenticationService.Instance.SignInWithGoogleAsync(idToken);
                Debug.Log("Sign-In With Unity Authentication is successful.");

                SetPlayerData(AuthenticationService.Instance.PlayerId, userName);

                Login();
            }
            catch (AuthenticationException ex)
            {
                // Compare error code to AuthenticationErrorCodes
                // Notify the player with the proper error message
                Debug.LogException(ex);
            }
            catch (RequestFailedException ex)
            {
                // Compare error code to CommonErrorCodes
                // Notify the player with the proper error message
                Debug.LogException(ex);
            }
        }

        void GoogleLogout()
        {
            PlayGamesPlatform.Instance.SignOut();
        }

        #endregion


        #region Private Methods

        /// <summary>
        /// Signs in an anonymous player.
        /// </summary>
        /// <returns></returns>
        async Task SignInAnonymouslyAsync()
        {
            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("Sign in anonymously succeeded!");

                userID = AuthenticationService.Instance.PlayerId;

                SetPlayerData(userID);

                Login();

                // Shows how to get the playerID
                Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");

            }
            catch (AuthenticationException ex)
            {
                // Compare error code to AuthenticationErrorCodes
                // Notify the player with the proper error message
                Debug.LogException(ex);
            }
            catch (RequestFailedException exception)
            {
                // Compare error code to CommonErrorCodes
                // Notify the player with the proper error message
                Debug.LogException(exception);
            }
        }

        /// <summary>
        /// Loads player data from cloud or creates a new player.
        /// </summary>
        /// <param name="id"></param>
        private async void SetPlayerData(string id)
        {
            SavePlayerData incomingSample = await RetrieveSpecificData<SavePlayerData>(id);

            if (incomingSample != null)
            {
                LoadPlayerData(incomingSample);
            }
            else
            {
                LoadPlayerData(id);
            }


        }

        /// <summary>
        /// Loads player data from cloud or creates a new player.
        /// </summary>
        /// <param name="id"></param>
        private async void SetPlayerData(string id, string name)
        {
            SavePlayerData incomingSample = await RetrieveSpecificData<SavePlayerData>(id);

            if (incomingSample != null)
                LoadPlayerData(incomingSample);
            else
            {
                LoadPlayerData(id, name);
            }

            // updates facebook gaming name
            if (FB.IsInitialized)
            {
                player.SetPlayerName(name);
            }
        }


        /// <summary>
        /// Signs in with Session Token.
        /// </summary>
        /// <returns></returns>
        async Task SignInWithSessionTokenAsync()
        {
            try
            {
                await AuthenticationService.Instance.SignInWithSessionTokenAsync();
                Debug.Log("SignIn is successful.");

                SetPlayerData(userID, userName);

                Login();
            }
            catch (AuthenticationException ex)
            {
                // Compare error code to AuthenticationErrorCodes
                // Notify the player with the proper error message
                Debug.LogException(ex);
            }
            catch (RequestFailedException ex)
            {
                // Compare error code to CommonErrorCodes
                // Notify the player with the proper error message
                Debug.LogException(ex);
            }
        }

        /// <summary>
        /// List all the cloud save keys/players.
        /// </summary>
        /// <returns></returns>
        private async Task ListAllKeys()
        {
            try
            {
                var keys = await SaveData.RetrieveAllKeysAsync();

                Debug.Log($"Keys count: {keys.Count}\n" +
                          $"Keys: {String.Join(", ", keys)}");
            }
            catch (CloudSaveValidationException e)
            {
                Debug.LogError(e);
            }
            catch (CloudSaveException e)
            {
                Debug.LogError(e);
            }
        }

        /// <summary>
        /// Saves a Single Item to cloud.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private async Task ForceSaveSingleData(string key, string value)
        {
            try
            {
                Dictionary<string, object> oneElement = new Dictionary<string, object>();

                // It's a text input field, but let's see if you actually entered a number.
                if (Int32.TryParse(value, out int wholeNumber))
                {
                    oneElement.Add(key, wholeNumber);
                }
                else if (Single.TryParse(value, out float fractionalNumber))
                {
                    oneElement.Add(key, fractionalNumber);
                }
                else
                {
                    oneElement.Add(key, value);
                }

                await SaveData.ForceSaveAsync(oneElement);

                Debug.Log($"Successfully saved {key}:{value}");
            }
            catch (CloudSaveValidationException e)
            {
                Debug.LogError(e);
            }
            catch (CloudSaveException e)
            {
                Debug.LogError(e);
            }
        }

        /// <summary>
        /// Save an object to cloud.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private async Task ForceSaveObjectData(string key, SavePlayerData value)
        {
            try
            {
                // Although we are only saving a single value here, you can save multiple keys
                // and values in a single batch.
                Dictionary<string, object> oneElement = new Dictionary<string, object>
                {
                    { key, value }
                };

                await SaveData.ForceSaveAsync(oneElement);

                Debug.Log($"Successfully saved {key}:{value}");
            }
            catch (CloudSaveValidationException e)
            {
                Debug.LogError(e);
            }
            catch (CloudSaveException e)
            {
                Debug.LogError(e);
            }
        }

        /// <summary>
        /// Get data from the cloud.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        private async Task<T> RetrieveSpecificData<T>(string key)
        {
            try
            {
                var results = await SaveData.LoadAsync(new HashSet<string> { key });

                if (results.TryGetValue(key, out string value))
                {
                    return JsonUtility.FromJson<T>(value);
                }
                else
                {
                    Debug.Log($"There is no such key as {key}!");
                }
            }
            catch (CloudSaveValidationException e)
            {
                Debug.LogError(e);
            }
            catch (CloudSaveException e)
            {
                Debug.LogError(e);
            }

            return default;
        }

        /// <summary>
        /// Deletes everything and signs out.
        /// </summary>
        /// <returns></returns>
        private async Task DeleteEverythingSignOut()
        {
            try
            {
                // If you wish to load only a subset of keys rather than everything, you
                // can call a method LoadAsync and pass a HashSet of keys into it.
                var results = await SaveData.LoadAllAsync();

                Debug.Log($"Elements loaded!");

                foreach (var element in results)
                {
                    Debug.Log($"Key: {element.Key}, Value: {element.Value}");
                    await ForceDeleteSpecificData(element.Key);
                }

                AuthenticationService.Instance.SignOut();
            }
            catch (CloudSaveValidationException e)
            {
                Debug.LogError(e);
            }
            catch (CloudSaveException e)
            {
                Debug.LogError(e);
            }
        }

        /// <summary>
        /// Deletes a specific key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private async Task ForceDeleteSpecificData(string key)
        {
            try
            {
                await SaveData.ForceDeleteAsync(key);

                Debug.Log($"Successfully deleted {key}");
            }
            catch (CloudSaveValidationException e)
            {
                Debug.LogError(e);
            }
            catch (CloudSaveException e)
            {
                Debug.LogError(e);
            }
        }

        /// <summary>
        /// Loads data from cloud.
        /// </summary>
        /// <param name="incomingSample"></param>
        private void LoadPlayerData(SavePlayerData incomingSample)
        {
            player.SetData(incomingSample);
        }

        /// <summary>
        /// Creates new anonymous player
        /// </summary>
        /// <param name="id"></param>
        private void LoadPlayerData(string id)
        {
            player.SetData(id);
        }

        /// <summary>
        /// Creates new player with login details.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        private void LoadPlayerData(string id, string name)
        {
            player.SetData(id, name);
        }

        /// <summary>
        /// Resets player data on logout.
        /// </summary>
        private void ResetPlayerData()
        {
            player.SetData();
        }

        #endregion


    }
}


