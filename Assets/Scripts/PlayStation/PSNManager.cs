#if !UNITY_PS5 && !UNITY_PS4
using UnityEngine;

    public class PSNManager : MonoBehaviour
    { }

#else

using UnityEngine;
using System;
using Unity.PSN.PS5.WebApi;
using System.Collections.Generic;
using PSNSample;

#if UNITY_PS5 || UNITY_PS4
using Unity.PSN.PS5;
using Unity.PSN.PS5.Initialization;
using Unity.PSN.PS5.Users;
using Unity.PSN.PS5.PremiumFeatures;
using Unity.PSN.PS5.Aysnc;
#endif

using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using System.Runtime.InteropServices;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.DualShock;
using System.Collections;

// IMPORTANT: State layout must match with GamepadInputStatePS5 in native.
[StructLayout(LayoutKind.Explicit, Size = 4)]
internal struct GamepadStatePS5Lite : IInputStateTypeInfo
{
    public FourCC format => new FourCC('P', '4', 'G', 'P');

    [InputControl(name = "buttonNorth", bit = 12)]
    [InputControl(name = "buttonEast", bit = 13)]
    [InputControl(name = "buttonSouth", bit = 14)]
    [InputControl(name = "buttonWest", bit = 15)]
    [InputControl(name = "dpad", layout = "Dpad", sizeInBits = 4, bit = 4)]
    [InputControl(name = "dpad/up", bit = 4)]
    [InputControl(name = "dpad/right", bit = 5)]
    [InputControl(name = "dpad/down", bit = 6)]
    [InputControl(name = "dpad/left", bit = 7)]
    [FieldOffset(0)] public uint buttons;

    [InputControl(layout = "Stick")][FieldOffset(4)] public Vector2 leftStick;
    [InputControl(layout = "Stick")][FieldOffset(12)] public Vector2 rightStick;
    [InputControl][FieldOffset(20)] public float leftTrigger;
    [InputControl][FieldOffset(24)] public float rightTrigger;
}

[InputControlLayout(stateType = typeof(GamepadStatePS5Lite), displayName = "PS5 DualSense (on PS5)")]
//[Scripting.Preserve]
class DualSenseGamepadLite : DualShockGamepad { }

namespace Com.MorganHouston.Imprecision
{
    public class PSNManager : MonoBehaviour
    {
        PSAuth psAuth;

#if UNITY_PS5
        SonyNpUDS m_UDS;
        PSEntitlements m_Entitlements;
#endif
        SonyLeaderboards m_Leaderboards;

        SonyTitleCloudStorage m_TCS;

        SonyGameIntentNotifications m_GameIntent;

        SonyOnlineSafety m_OnlineSafety;

        //SonyAuth m_SonyAuth;

        SonySessions m_Sessions;

        SonyWebApiEvents m_WebEvents;

        SonySessionSignalling m_SessionSignalling;

        SonyBandwidth m_Bandwidth;

        private void Awake()
        {
            psAuth = GetComponent<PSAuth>();
            //PSGamePad[] gamePads = GetComponents<PSGamePad>();

            //PSUser.Initialize(gamePads);
            //PSUserProfiles.Initialize();
        }

        // Start is called before the first frame update
        void Start()
        {
            InputSystem.RegisterLayout<DualSenseGamepadLite>("PS5DualSenseGamepad",
            matches: new UnityEngine.InputSystem.Layouts.InputDeviceMatcher()
                .WithInterface("PS5")
                .WithDeviceClass("PS5DualShockGamepad"));

            psAuth = GetComponent<PSAuth>();

            /*
#if UNITY_PS5
            m_Trophies = new SonyNpTrophies();
            m_UDS = new SonyNpUDS();
            m_Entitlements = new SonyEntitlements();
#endif
            m_Leaderboards = new SonyLeaderboards();

            m_TCS = new SonyTitleCloudStorage();

            m_GameIntent = new SonyGameIntentNotifications();

            m_OnlineSafety = new SonyOnlineSafety();*/

            //m_SonyAuth = new SonyAuth();

            /*m_Sessions = new SonySessions();

            m_WebEvents = new SonyWebApiEvents();

            m_SessionSignalling = new SonySessionSignalling();

            m_Bandwidth = new SonyBandwidth();*/

            //Initialize();

            //m_Sessions.SetupSessionNotifications();
        }

        public InitResult initResult;

        public void Initialize()
        {
            if(initResult.Initialized == true)
            {
                psAuth.SignIn();
                return;
            }
            try
            {
                initResult = Main.Initialize();

                //RequestCallback.OnRequestCompletion += OnCompleteion;

                if (initResult.Initialized == true)
                {
                    PSAuthInit();
                    //OnScreenLog.Add("PSN Initialized ");
                    //m_SonyAuth = new SonyAuth();
                    //CloudSaveLogin.Instance.SignInPS(m_SonyAuth.userID, m_SonyAuth.iDToken, m_SonyAuth.authCode);
                }
                else
                {
                    //OnScreenLog.Add("PSN not initialized ");

                }
            }
            catch (PSNException e)
            {
                //OnScreenLog.AddError("Exception During Initialization : " + e.ExtendedMessage);
            }
#if UNITY_EDITOR
            catch (DllNotFoundException e)
            {
                //OnScreenLog.AddError("Missing DLL Expection : " + e.Message);
                //OnScreenLog.AddError("The sample APP will not run in the editor.");
            }
#endif

            
        }

        // Update is called once per frame
        void Update()
        {
            try
            {
                Main.Update();
            }
            catch (Exception e)
            {
                //OnScreenLog.AddError("Main.Update Exception : " + e.Message);
                //OnScreenLog.AddError(e.StackTrace);
            }

        }

        public static bool CheckRequestOK<R>(R request) where R : Request
        {
            if (request == null)
            {
                UnityEngine.Debug.LogError("Request is null");
                return false;
            }

            if (request.Result.apiResult == APIResultTypes.Success)
            {
                return true;
            }

            PSNManager.OutputApiResult(request.Result);

            return false;
        }

        public static bool CheckAysncRequestOK<R>(AsyncRequest<R> asyncRequest) where R : Request
        {
            if (asyncRequest == null)
            {
                UnityEngine.Debug.LogError("AsyncRequest is null");
                return false;
            }

            return CheckRequestOK<R>(asyncRequest.Request);
        }

        public static void OutputApiResult(APIResult result)
        {
            if (result.apiResult == APIResultTypes.Success)
            {
                return;
            }

            string output = result.ErrorMessage();

            //OnScreenLog.AddError($"\n{(sceCode)(long)(UInt32)result.sceErrorCode}\n");

            if (result.apiResult == APIResultTypes.Error)
            {
                //OnScreenLog.AddError(output);
            }
            else
            {
                //OnScreenLog.AddWarning(output);
            }
        }

        IEnumerator AuthPlayer()
        {
            yield return new WaitForSeconds(10f);

            PSAuthInit();
        }

        private void PSAuthInit()
        {
            psAuth.Initialize();
            //PSUserProfiles.Initialize();
            
        }

    }
}
#endif