#if !UNITY_PS5 && !UNITY_PS4
using UnityEngine;

public class PSAuth : MonoBehaviour
{}

#endif

#if UNITY_PS5 || UNITY_PS4
using Com.MorganHouston.Imprecision;
using System.Collections;
using Unity.PSN.PS5.Auth;
using Unity.PSN.PS5.Aysnc;
using UnityEngine;
using UnityEngine.PS5;
#endif


#if UNITY_PS5 || UNITY_PS4
public class PSAuth : MonoBehaviour
{
    public string userID, iDToken, authCode;
    public bool initialized;
    private bool calledSignIn;
    private bool notSignedIn = false;
    private int tries = 0;
    private void Update()
    {
#if UNITY_PS5 && !UNITY_EDITOR
        if(initialized && calledSignIn == false)
        {
            calledSignIn = true;
            CloudSaveLogin.Instance.SignInPS(userID, iDToken, authCode);
        }else if (calledSignIn == false && notSignedIn == true)
        {
            calledSignIn = true;
            CloudSaveLogin.Instance.SignInPS(userID, iDToken, authCode);
        }
#endif
    }

    public void Initialize()
    {
        if (PSGamePad.activeGamePad.loggedInUser.userName.Contains("Guest"))
        {
            CloudSaveLogin.Instance.isSigningIn = false;
            CloudSaveLogin.Instance.SignInAnonymously();
            return;
        }
        GetAuthCode();

    }

    public void SignIn()
    {
        if (PSGamePad.activeGamePad.loggedInUser.userName.Contains("Guest"))
        {
            CloudSaveLogin.Instance.isSigningIn = false;
            CloudSaveLogin.Instance.SignInAnonymously();
            return;
        }
#if UNITY_PS5 && !UNITY_EDITOR
        CloudSaveLogin.Instance.SignInPS(userID, iDToken, authCode);
#endif
    }

    private void CheckPremium()
    {
        if (PSFeatureGating.premiumEventEnabled) PSFeatureGating.CheckPremium();
    }

    private void GetAuthCode()
    {
        try
        { 

            Authentication.GetAuthorizationCodeRequest request = new Authentication.GetAuthorizationCodeRequest()
            {
                UserId = PSGamePad.activeGamePad.loggedInUser.userId,
#if UNITY_PS5
                ClientId = "686986a6-3b34-4a42-89d1-b4ba193bc80f",
#elif UNITY_PS4
                    ClientId = "c5806b90-16f4-4086-9b43-665b69654b05",
#endif
                Scope = "psn:s2s"
            };

            var requestOp = new AsyncRequest<Authentication.GetAuthorizationCodeRequest>(request).ContinueWith((antecedent) =>
            {
                if (PSNManager.CheckAysncRequestOK(antecedent))
                {
                    if(antecedent.HasSequenceFailed)
                    {
                        notSignedIn = true;
                    }
                    else
                    {
                        authCode = antecedent.Request.AuthCode;
                        GetIDToken();
                    }

                }
                else
                {
                    userID = antecedent.Request.UserId.ToString();
                    CloudSaveLogin.Instance.userID = userID;
                    CloudSaveLogin.Instance.userName = PSUser.GetActiveUserName;
                    notSignedIn = true;
                }
            });

            Authentication.Schedule(requestOp);
        }
        catch
        {
            tries++;
            Debug.Log("Failed to Auth, Tries: " + tries);
            if(tries == 5)
            {
                CloudSaveLogin.Instance.isSigningIn = false;
                CloudSaveLogin.Instance.SignInAnonymously();
            }
            else
                GetAuthCode();
        }

        
    }

    private void GetIDToken()
    {
        Authentication.GetIdTokenRequest request = new Authentication.GetIdTokenRequest()
        {
            UserId = PSGamePad.activeGamePad.loggedInUser.userId,
#if UNITY_PS5
            ClientId = "686986a6-3b34-4a42-89d1-b4ba193bc80f",
            ClientSecret = "U3ILASyaI0Y3s0l5",
#elif UNITY_PS4
                    ClientId = "c5806b90-16f4-4086-9b43-665b69654b05",
                    ClientSecret = "NhpNTk6BygPyw7hw",
#endif

            Scope = "openid id_token:psn.basic_claims"
        };

        var requestOp = new AsyncRequest<Authentication.GetIdTokenRequest>(request).ContinueWith((antecedent) =>
        {
            if (PSNManager.CheckAysncRequestOK(antecedent))
            {
                if (antecedent.HasSequenceFailed)
                {
                    notSignedIn = true;
                }
                else
                {
                    iDToken = antecedent.Request.IdToken;
                    userID = antecedent.Request.UserId.ToString();
                    CloudSaveLogin.Instance.userID = userID;
                    CloudSaveLogin.Instance.userName = PSUser.GetActiveUserName;
                    initialized = true;
                    //PSOnlineSafety.GetCRStatus();
                    //PSFeatureGating.Initialize();
                }

            }
            else
            {
                userID = antecedent.Request.UserId.ToString();
                CloudSaveLogin.Instance.userID = userID;
                CloudSaveLogin.Instance.userName = PSUser.GetActiveUserName;
                notSignedIn = true;
            }
        });

        Authentication.Schedule(requestOp);

    }

    public void ResetInit()
    {
        notSignedIn = false;
        initialized = false;
        calledSignIn = false;
    }

}
#endif

