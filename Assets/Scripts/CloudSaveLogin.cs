using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using System.Threading.Tasks;
using Unity.Services.Core;

public class CloudSaveLogin : MonoBehaviour
{
    public GameObject mainMenuScreen;

    // Start is called before the first frame update
    async void Awake()
    {
        await UnityServices.InitializeAsync();
    }

    public async void SignInAnonymously()
    {
        await SignInAnonymouslyAsync();
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

            this.gameObject.SetActive(false);
            mainMenuScreen.SetActive(true);

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
}
