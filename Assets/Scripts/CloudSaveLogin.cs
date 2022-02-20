using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using System.Threading.Tasks;
using Unity.Services.Core;
using Facebook.Unity;
using System;

public class CloudSaveLogin : MonoBehaviour
{
    public GameObject mainMenuScreen;

    public Player player;

    // Start is called before the first frame update
    async void Awake()
    {
        await UnityServices.InitializeAsync();

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
    }

    public async void SignInAnonymously()
    {
        await SignInAnonymouslyAsync();
    }

    public void SignInFacebook()
    {
        var perms = new List<string>() { "public_profile", "email" };
        FB.LogInWithReadPermissions(perms, AuthCallback);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    async Task SignInAnonymouslyAsync()
    {
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Sign in anonymously succeeded!");

            SetPlayerData(AuthenticationService.Instance.PlayerId);

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

    private async void SetPlayerData(string id)
    {
        Player incomingSample = await RetrieveSpecificData<Player>(id);

        player.SetData(incomingSample.UserID, incomingSample.Points, incomingSample.PlayerName, incomingSample.PlayerEmail, incomingSample.PlayerLevel, incomingSample.PlayerExperience);
    }

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

    private void AuthCallback(ILoginResult result)
    {
        if (FB.IsLoggedIn)
        {
            // AccessToken class will have session details
            var aToken = Facebook.Unity.AccessToken.CurrentAccessToken;
            // Print current access token's User ID
            Debug.Log(aToken.UserId);

            SetPlayerData(aToken.UserId);

            // Print current access token's granted permissions
            foreach (string perm in aToken.Permissions)
            {
                Debug.Log(perm);
            }

            Login();
        }
        else
        {
            Debug.Log("User cancelled login");
        }
    }

    private void Login()
    {
        this.gameObject.SetActive(false);
        mainMenuScreen.SetActive(true);
    }

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

    private async Task ForceSaveObjectData(string key, Player value)
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

    private async Task RetrieveEverything()
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
    }

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
}

