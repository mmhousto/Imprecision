#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLESTEAMWORKS
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_IOS
using UnityEngine.SocialPlatforms.GameCenter;
#endif
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using System.Threading.Tasks;
using Unity.Services.Core;
//using Facebook.Unity;
using System;
using AppleAuth;
using AppleAuth.Native;
using AppleAuth.Enums;
using AppleAuth.Extensions;
using AppleAuth.Interfaces;
using System.Text;
#if !DISABLESTEAMWORKS
using Steamworks;
#endif
#if UNITY_ANDROID
using GooglePlayGames.BasicApi;
using GooglePlayGames;
#endif

namespace Com.MorganHouston.Imprecision
{

    public class CloudSaveLogin : MonoBehaviour
    {

        #region Fields/Variables

        private static CloudSaveLogin instance;

        public static CloudSaveLogin Instance { get { return instance; } }

        // What SSO Option the use is using atm.
        public enum ssoOption { Anonymous, Facebook, Google, Apple, Steam }

        // Player Data Object
        private Player player;

        public ssoOption currentSSO = ssoOption.Anonymous;

        private IAppleAuthManager appleAuthManager;

        private bool triedQuickLogin = false;

        public bool isSteam;
        public bool devModeActivated = false;
        public bool loggedIn;
        private bool isSigningIn = false;

        // User Info.
        public string userName, userID;

#if !DISABLESTEAMWORKS
        public GameObject steamStats;

        Callback<GetAuthSessionTicketResponse_t> m_AuthTicketResponseCallback;
        HAuthTicket m_AuthTicket;
        string m_SessionTicket;
#endif


#endregion


#region MonoBehaviour Methods


        // Start is called before the first frame update
        async void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            else
            {
                instance = this;
                DontDestroyOnLoad(Instance.gameObject);
            }

            loggedIn = false;

            if (UnityServices.State == ServicesInitializationState.Initialized)
            {

            }
            else
            {
                await UnityServices.InitializeAsync();
            }


#if UNITY_WSA
#else
            if (isSteam && SteamManager.Initialized)
            {
                SignInWithSteam();
                isSigningIn = true;
            }

            /*if (!FB.IsInitialized)
            {
                // Initialize the Facebook SDK
                FB.Init(InitCallback, OnHideUnity);
            }
            else
            {
                // Already initialized, signal an app activation App Event
                FB.ActivateApp();
            }*/

#endif


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
#if (UNITY_IOS || UNITY_STANDALONE_OSX)
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
                isSigningIn = true;
                triedQuickLogin = true;
            }
#endif

        }

        /*// <summary>
        /// Saves data on application exit.
        /// </summary>
        private void OnApplicationQuit()
        {
            SaveCloudData();

        }*/

        /*// <summary>
        /// Saves data to cloud on application pause or swipe out.
        /// </summary>
        /// <param name="pause"></param>
        private void OnApplicationPause(bool pause)
        {
#if (UNITY_IOS || UNITY_ANDROID)
        if(pause)
            SaveCloudData();
#endif
        }*/


#endregion


#region Public Sign In/Out Methods


        /// <summary>
        /// Signs user into an anonymous account.
        /// </summary>
        public async void SignInAnonymously()
        {
            if (isSigningIn || AuthenticationService.Instance.IsSignedIn) return;
            isSigningIn = true;

            currentSSO = ssoOption.Anonymous;
            AuthenticationService.Instance.SwitchProfile("default");
            await SignInAnonymouslyAsync();

        }

        /// <summary>
        /// Signs user into dev account.
        /// </summary>
        public async void SignInDeveloper()
        {
            if (isSigningIn || AuthenticationService.Instance.IsSignedIn) return;
            isSigningIn = true;

            devModeActivated = true;
            await SignInAnonymouslyAsync();
        }

        /// <summary>
        /// Signs user into facebook account with authentication from Facebook.
        /// </summary>
        /*public void SignInFacebook()
        {
            if (isSigningIn || AuthenticationService.Instance.IsSignedIn) return;
            isSigningIn = true;

            currentSSO = ssoOption.Facebook;
            AuthenticationService.Instance.SwitchProfile("facebook");

#if UNITY_ANDROID
        FB.Android.RetrieveLoginStatus(LoginStatusCallback);
#else
            var perms = new List<string>() { "public_profile" };
            FB.LogInWithReadPermissions(perms, AuthCallback);
#endif
            isSigningIn = false;

        }*/

        /// <summary>
        /// Signs user into Apple with Auth from Apple.
        /// </summary>
        public async void SignInApple()
        {
            if (isSigningIn || AuthenticationService.Instance.IsSignedIn) return;
            isSigningIn = true;

            currentSSO = ssoOption.Apple;
            if (!AuthenticationService.Instance.IsSignedIn)
                AuthenticationService.Instance.SwitchProfile("apple");

            var idToken = await GetAppleIdTokenAsync();

            if(idToken != null)
            {
                await AuthenticationService.Instance.SignInWithAppleAsync(idToken);

                GameCenterLogin();

                SetPlayerData(AuthenticationService.Instance.PlayerId, userName);


                Login();
            }
            isSigningIn = false;

        }

        
        public void SignInWithSteam()
        {
#if !DISABLESTEAMWORKS
            if (isSigningIn || AuthenticationService.Instance.IsSignedIn) return;
            isSigningIn = true;

            currentSSO = ssoOption.Steam;
            AuthenticationService.Instance.SwitchProfile("steam");

            // It's not necessary to add event handlers if they are 
            // already hooked up.
            // Callback.Create return value must be assigned to a 
            // member variable to prevent the GC from cleaning it up.
            // Create the callback to receive events when the session ticket
            // is ready to use in the web API.
            // See GetAuthSessionTicket document for details.
            m_AuthTicketResponseCallback = Callback<GetAuthSessionTicketResponse_t>.Create(OnAuthCallback);

            var buffer = new byte[1024];

            CSteamID cSteamID = SteamUser.GetSteamID();

            userID = cSteamID.m_SteamID.ToString();
            userName = SteamFriends.GetPersonaName();

            // Create a SteamNetworkingIdentity object
            SteamNetworkingIdentity identity = new SteamNetworkingIdentity();

            // Set the Steam ID in the identity object
            identity.SetSteamID(cSteamID);

            m_AuthTicket = SteamUser.GetAuthSessionTicket(buffer, buffer.Length, out var ticketSize, ref identity);

            //Array.Resize(ref buffer, (int)ticketSize);

            // The ticket is not ready yet, wait for OnAuthCallback.
            m_SessionTicket = BitConverter.ToString(buffer).Replace("-", string.Empty);
#endif
        }

#if !DISABLESTEAMWORKS
        void OnAuthCallback(GetAuthSessionTicketResponse_t callback)
        {
            // Call Unity Authentication SDK to sign in or link with Steam.
            //Debug.Log("Steam Login success. Session Ticket: " + m_SessionTicket);
            CallSignInSteam(m_SessionTicket);
        }

        private async void CallSignInSteam(string sessionTicket)
        {
            await SignInWithSteamAsync(m_SessionTicket);
        }

#endif

        private void GameCenterLogin()
        {
            Social.localUser.Authenticate(success =>
            {
                if (success)
                {
                    //Debug.Log("Authentication successful");
                    userName = Social.localUser.userName;/* +
                        "\nUser ID: " + Social.localUser.id +*/
                    string userInfo = "\nIsUnderage: " + Social.localUser.underage;
                }
                else { }
                    //Debug.Log("Authentication failed");
            });
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
        /*public void FacebookLogout()
        {
            FB.LogOut();
        }*/


#endregion


#region Private Login/Logout Methods

        /// <summary>
        /// Loads the Main Menu Panel.
        /// </summary>
        private void Login()
        {
            loggedIn = true;
            LoginManager.Instance.Login();
        }

        /// <summary>
        /// Loads the Sign-In/MainMenu Scene.
        /// </summary>
        private void LogoutScreenActivate()
        {
            loggedIn = false;
            //SceneLoader.LoadThisScene(1);
            UnLoadLevel.Instance.LoadUnLoad(1);
        }

        /// <summary>
        /// Saves and logs out of user and resets player data.
        /// </summary>
        private void SaveLogout()
        {
            SaveCloudData();

            /*if (FB.IsLoggedIn)
            {
                FacebookLogout();
            }*/

            if (currentSSO == ssoOption.Google)
            {
                //GoogleLogout();
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
                    //Debug.Log("Quick Login Apple Failed");
                    isSigningIn = false;
                    return;
                    // Quick login failed. The user has never used Sign in With Apple on your app. Go to login screen
                });

            var idToken = PlayerPrefs.GetString("AppleTokenIdKey");

            await AuthenticationService.Instance.SignInWithAppleAsync(idToken);

            GameCenterLogin();

            SetPlayerData(AuthenticationService.Instance.PlayerId, userName);

            Login();

            isSigningIn = false;
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
                                    //Debug.Log("User ID is valid!");
                                    QuickLoginApple();
                                    break;

                                case CredentialState.Revoked:
                                    // User ID was revoked. Go to login screen.
                                    //Debug.Log("User ID was revoked.");
                                    if (AuthenticationService.Instance.IsSignedIn)
                                    {
                                        AuthenticationService.Instance.SignOut();
                                    }
                                    isSigningIn = false;
                                    break;

                                case CredentialState.NotFound:
                                    // User ID was not found. Go to login screen.
                                    //Debug.Log("User ID was not found.");
                                    isSigningIn = false;
                                    break;
                            }
                        },
                        error =>
                        {
                            // Something went wrong
                            //Debug.Log("Credential Failed");
                            if (AuthenticationService.Instance.IsSignedIn)
                            {
                                AuthenticationService.Instance.SignOut();
                            }
                            isSigningIn = false;
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
                        if (appleIdCredential.User != null)
                        {
                            userID = appleIdCredential.User;
                            PlayerPrefs.SetString("AppleUserIdKey", userID);
                        }
                        else
                        {
                            userID = PlayerPrefs.GetString("AppleUserIdKey", "123456");
                        }


                        // Email (Received ONLY in the first login)
                        /*email = appleIdCredential.Email;
                            PlayerPrefs.SetString("AppleUserEmailKey", email);*/

                        // Full name (Received ONLY in the first login)
                        if (appleIdCredential.FullName != null)
                        {
                            userName = appleIdCredential.FullName.GivenName;
                            PlayerPrefs.SetString("AppleUserNameKey", userName);
                        }
                        else
                        {
                            userName = PlayerPrefs.GetString("AppleUserNameKey", "Player 1");
                        }


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

/*
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
                //Debug.Log("Failed to Initialize the Facebook SDK");
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
                //Debug.Log(aToken.UserId);
                userID = aToken.UserId;

                FB.API("me?fields=id,name", HttpMethod.GET, AssignInfo);



                await SignInWithFacebookAsync(aToken.TokenString);

            }
            else
            {
                //Debug.Log("User cancelled login");
                isSigningIn = false;
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
                //Debug.Log("Error: " + result.Error);
                isSigningIn = false;
            }
            else if (!FB.IsLoggedIn) { }
                //Debug.Log("Login Canceled By Player");
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
                //Debug.Log("Sign-In With Facebook is successful.");

                SetPlayerData(AuthenticationService.Instance.PlayerId, userName);

                Login();
            }
            catch (AuthenticationException ex)
            {
                // Compare error code to AuthenticationErrorCodes
                // Notify the player with the proper error message
                //Debug.LogException(ex);
            }
            catch (RequestFailedException ex)
            {
                // Compare error code to CommonErrorCodes
                // Notify the player with the proper error message
                //Debug.LogException(ex);
            }
            isSigningIn = false;
        }

        /// <summary>
        /// Logs the player in, if they were logged in before.
        /// </summary>
        /// <param name="result"></param>
        private async void LoginStatusCallback(ILoginStatusResult result)
        {
            isSigningIn = false;
            if (!string.IsNullOrEmpty(result.Error))
            {
                //Debug.Log("Error: " + result.Error);
                var perms = new List<string>() { "public_profile" };
                FB.LogInWithReadPermissions(perms, AuthCallback);
            }
            else if (result.Failed)
            {
                //Debug.Log("Failure: Access Token could not be retrieved");
                var perms = new List<string>() { "public_profile" };
                FB.LogInWithReadPermissions(perms, AuthCallback);
            }
            else
            {
                // Successfully logged user in
                // A popup notification will appear that says "Logged in as <User Name>"
                //Debug.Log("Success: " + result.AccessToken.UserId);

                await SignInWithFacebookAsync(result.AccessToken.TokenString);

            }
        }


#endregion
*/

#region Google Play Auth

#if UNITY_ANDROID
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
            if (isSigningIn || AuthenticationService.Instance.IsSignedIn) return;
            isSigningIn = true;

            currentSSO = ssoOption.Google;
            AuthenticationService.Instance.SwitchProfile("google");
            try
            {
                PlayGamesPlatform.Instance.Authenticate(SignInInteractivity.CanPromptOnce, success => { OnGooglePlayGamesLogin(success); });
            }
            catch (Exception e)
            {
                //Debug.Log("ERROR: " + e);
                isSigningIn = false;
            }
            
        }

        async void OnGooglePlayGamesLogin(SignInStatus status)
        {
            if (status == SignInStatus.Success)
            {
                ((PlayGamesPlatform)Social.Active).SetGravityForPopups(Gravity.BOTTOM);

                // Call Unity Authentication SDK to sign in or link with Google.
                var idToken = ((PlayGamesLocalUser)Social.localUser).GetIdToken();
                //Debug.Log("Login with Google Play Games done. IdToken: " + idToken);
                userID = Social.localUser.id;
                userName = Social.localUser.userName;

                await SignInWithGoogleAsync(idToken);
                //Debug.Log("Sign-In With Google is successful.");

            }
            else if (status == SignInStatus.UiSignInRequired)
            {
                PlayGamesPlatform.Instance.Authenticate(SignInInteractivity.CanPromptAlways, success => { OnGooglePlayGamesLogin(success); });
            }
            else
            {
                //Debug.Log("Unsuccessful login");
            }
            isSigningIn = false;
        }

        async Task SignInWithGoogleAsync(string idToken)
        {
            try
            {
                //Debug.Log("Authenticating with id Token: " + idToken);
                await AuthenticationService.Instance.SignInWithGoogleAsync(idToken);
                //Debug.Log("Sign-In With Unity Authentication is successful.");

                SetPlayerData(AuthenticationService.Instance.PlayerId, userName);

                Login();
            }
            catch (AuthenticationException ex)
            {
                // Compare error code to AuthenticationErrorCodes
                // Notify the player with the proper error message
                //Debug.LogException(ex);
            }
            catch (RequestFailedException ex)
            {
                // Compare error code to CommonErrorCodes
                // Notify the player with the proper error message
                //Debug.LogException(ex);
            }
            isSigningIn = false;
        }

        void GoogleLogout()
        {
            PlayGamesPlatform.Instance.SignOut();
        }
#endif

#endregion


#region Steam Auth

        async Task SignInWithSteamAsync(string ticket)
        {
            try
            {
                await AuthenticationService.Instance.SignInWithSteamAsync(ticket);

                SetPlayerData(userID, userName);

                Login();
#if !DISABLESTEAMWORKS
                steamStats.SetActive(true);
#endif

            }
            catch (AuthenticationException ex)
            {
                // Compare error code to AuthenticationErrorCodes
                // Notify the player with the proper error message
                //Debug.Log(ex);
            }
            catch (RequestFailedException ex)
            {
                // Compare error code to CommonErrorCodes
                // Notify the player with the proper error message
                //Debug.Log(ex);
            }
            isSigningIn = false;
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
                //Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");

            }
            catch (AuthenticationException ex)
            {
                // Compare error code to AuthenticationErrorCodes
                // Notify the player with the proper error message
                //Debug.Log(ex);
            }
            catch (RequestFailedException exception)
            {
                // Compare error code to CommonErrorCodes
                // Notify the player with the proper error message
                //Debug.Log(exception);
            }

            isSigningIn = false;
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

            player.SetPlayerName(name);
        }


        /// <summary>
        /// Signs in with Session Token.
        /// </summary>
        /// <returns></returns>
        async Task SignInWithSessionTokenAsync()
        {
            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();  //SignInWithSessionTokenAsync();
                Debug.Log("SignIn is successful.");

                SetPlayerData(userID, userName);

                Login();
            }
            catch (AuthenticationException ex)
            {
                // Compare error code to AuthenticationErrorCodes
                // Notify the player with the proper error message
                //Debug.Log(ex);
            }
            catch (RequestFailedException ex)
            {
                // Compare error code to CommonErrorCodes
                // Notify the player with the proper error message
                //Debug.Log(ex);
            }

            isSigningIn = false;
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
                //Debug.LogError(e);
            }
            catch (CloudSaveException e)
            {
                //Debug.LogError(e);
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

                //Debug.Log($"Successfully saved {key}:{value}");
            }
            catch (CloudSaveValidationException e)
            {
                //Debug.LogError(e);
            }
            catch (CloudSaveException e)
            {
                //Debug.LogError(e);
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

                //Debug.Log($"Successfully saved {key}:{value}");
            }
            catch (CloudSaveValidationException e)
            {
                //Debug.LogError(e);
            }
            catch (CloudSaveException e)
            {
                //Debug.LogError(e);
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
                    //Debug.Log($"There is no such key as {key}!");
                }
            }
            catch (CloudSaveValidationException e)
            {
                //Debug.LogError(e);
            }
            catch (CloudSaveException e)
            {
                //Debug.LogError(e);
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

                //Debug.Log($"Elements loaded!");

                foreach (var element in results)
                {
                    //Debug.Log($"Key: {element.Key}, Value: {element.Value}");
                    await ForceDeleteSpecificData(element.Key);
                }

                AuthenticationService.Instance.SignOut();
            }
            catch (CloudSaveValidationException e)
            {
                //Debug.LogError(e);
            }
            catch (CloudSaveException e)
            {
                //Debug.LogError(e);
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

                //Debug.Log($"Successfully deleted {key}");
            }
            catch (CloudSaveValidationException e)
            {
                //Debug.LogError(e);
            }
            catch (CloudSaveException e)
            {
                //Debug.LogError(e);
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


