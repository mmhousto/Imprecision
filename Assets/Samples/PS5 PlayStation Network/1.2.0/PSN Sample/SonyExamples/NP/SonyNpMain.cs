#define USE_ASYNC_HANDLING
#define ENABLE_TUS_LOGGING

using UnityEngine;
using System;
using Unity.PSN.PS5.WebApi;
using System.Collections.Generic;

#if UNITY_PS5 || UNITY_PS4
using Unity.PSN.PS5;
using Unity.PSN.PS5.Initialization;
using Unity.PSN.PS5.Users;
using Unity.PSN.PS5.Aysnc;
#endif

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using System.Runtime.InteropServices;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.DualShock;

// IMPORTANT: State layout must match with GamepadInputStatePS5 in native.
[StructLayout(LayoutKind.Explicit, Size = 4)]
internal struct GamepadStatePS5Lite : IInputStateTypeInfo
{
    public FourCC format  => new FourCC('P', '4', 'G', 'P');

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

    [InputControl(layout = "Stick")] [FieldOffset(4)] public Vector2 leftStick;
    [InputControl(layout = "Stick")] [FieldOffset(12)] public Vector2 rightStick;
    [InputControl] [FieldOffset(20)] public float leftTrigger;
    [InputControl] [FieldOffset(24)] public float rightTrigger;
}

[InputControlLayout(stateType = typeof(GamepadStatePS5Lite), displayName = "PS5 DualSense (on PS5)")]
//[Scripting.Preserve]
class DualSenseGamepadLite : DualShockGamepad {}
#endif

namespace PSNSample
{

    public class SonyNpMain : MonoBehaviour, IScreen
    {
        public MeshRenderer iconRender;

#if UNITY_PS5 || UNITY_PS4
        MenuStack m_MenuStack = null;
        MenuLayout m_MenuMain;
        bool m_NpReady = true;     // Is the NP plugin initialized and ready for use.

#if UNITY_PS5
        SonyNpTrophies m_Trophies;
        SonyNpUDS m_UDS;
        SonyFeatureGating m_FeatureGating;
        SonyGameUpdate m_GameUpdate;
        SonyEntitlements m_Entitlements;
        SonyCommerce m_Commerce;
#endif
        SonyLeaderboards m_Leaderboards;

        SonyTitleCloudStorage m_TCS;

        SonyMessages m_Messages;

        SonyUserProfiles m_UserProfiles;

        SonyGameIntentNotifications m_GameIntent;

        SonyOnlineSafety m_OnlineSafety;

        SonyAuth m_SonyAuth;

        SonySessions m_Sessions;

        SonyWebApiEvents m_WebEvents;

        SonySessionSignalling m_SessionSignalling;

        SonyBandwidth m_Bandwidth;

        void Start()
        {
            //s_main = this;

#if ENABLE_INPUT_SYSTEM
        InputSystem.RegisterLayout<DualSenseGamepadLite>("PS5DualSenseGamepad",
            matches: new UnityEngine.InputSystem.Layouts.InputDeviceMatcher()
                .WithInterface("PS5")
                .WithDeviceClass("PS5DualShockGamepad"));
#endif

            m_MenuMain = new MenuLayout(this, 450, 20);

            m_MenuStack = new MenuStack();
            m_MenuStack.SetMenu(m_MenuMain);

#if UNITY_PS5
            m_Trophies = new SonyNpTrophies();
            m_UDS = new SonyNpUDS();
            m_FeatureGating = new SonyFeatureGating();
            m_GameUpdate = new SonyGameUpdate();
            m_Entitlements = new SonyEntitlements();
            m_Commerce = new SonyCommerce();
#endif
            m_Leaderboards = new SonyLeaderboards();

            m_TCS = new SonyTitleCloudStorage();

            m_Messages = new SonyMessages();

            m_UserProfiles = new SonyUserProfiles();

            m_GameIntent = new SonyGameIntentNotifications();

            m_OnlineSafety = new SonyOnlineSafety();

            m_SonyAuth = new SonyAuth();

            m_Sessions = new SonySessions();

            m_WebEvents = new SonyWebApiEvents();

            m_SessionSignalling = new SonySessionSignalling();

            m_Bandwidth = new SonyBandwidth();

            // Initialize the PSN system.
            OnScreenLog.Add("Initializing PSN");

            Initialize();

            m_Sessions.SetupSessionNotifications();
        }

        public InitResult initResult;

        void Initialize()
        {
            try
            {
                initResult = Main.Initialize();

                // RequestCallback.OnRequestCompletion += OnCompleteion;

                if (initResult.Initialized == true)
                {
                    OnScreenLog.Add("PSN Initialized ");
                    OnScreenLog.Add("Plugin SDK Version : " + initResult.SceSDKVersion.ToString());
                }
                else
                {
                    OnScreenLog.Add("PSN not initialized ");
                }
            }
            catch (PSNException e)
            {
                OnScreenLog.AddError("Exception During Initialization : " + e.ExtendedMessage);
            }
#if UNITY_EDITOR
            catch (DllNotFoundException e)
            {
                OnScreenLog.AddError("Missing DLL Expection : " + e.Message);
                OnScreenLog.AddError("The sample APP will not run in the editor.");
            }
#endif

            string[] args = System.Environment.GetCommandLineArgs();

            if (args.Length > 0)
            {
                OnScreenLog.Add("Args:");

                for (int i = 0; i < args.Length; i++)
                {
                    OnScreenLog.Add("  " + args[i]);
                }
            }
            else
            {
                OnScreenLog.Add("No Args");
            }


            OnScreenLog.AddNewLine();

            GamePad[] gamePads = GetComponents<GamePad>();

            User.Initialize(gamePads);
        }

        void Update()
        {
            User.CheckRegistration();

            try
            {
                Main.Update();
            }
            catch(Exception e)
            {
                OnScreenLog.AddError("Main.Update Exception : " + e.Message);
                OnScreenLog.AddError(e.StackTrace);
            }

#if UNITY_PS5
            m_Trophies.Update(iconRender);
            m_Commerce.Update();
#endif
            m_Messages.Update();

            //Unity.PSN.PS5.Sessions.TestJSONParsing.TestJSONNotifciations();
            // Unity.PSN.PS5.Matchmaking.TestJSONParsing.TestJSONNotifciations();
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

            SonyNpMain.OutputApiResult(request.Result);

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

            OnScreenLog.AddError($"\n{(sceCode)(long)(UInt32)result.sceErrorCode}\n");

            if(result.apiResult == APIResultTypes.Error)
            {
                OnScreenLog.AddError(output);
            }
            else
            {
                OnScreenLog.AddWarning(output);
            }
        }

        void MenuMain()
        {
            m_MenuMain.Update();

            if (m_NpReady)
            {
                bool isRegistered = User.IsActiveUserAdded;

                if (m_MenuMain.AddItem("Sessions", isRegistered == true))
                {
                    m_MenuStack.PushMenu(m_Sessions.GetMenu());
                }

                if (m_MenuMain.AddItem("Signalling", isRegistered == true))
                {
                    m_MenuStack.PushMenu(m_SessionSignalling.GetMenu());
                }

                if (m_MenuMain.AddItem("Leaderboards", isRegistered == true))
                {
                    m_MenuStack.PushMenu(m_Leaderboards.GetMenu());
                }
#if UNITY_PS5
                if (m_MenuMain.AddItem("Commerce", isRegistered == true))
                {
                    m_MenuStack.PushMenu(m_Commerce.GetMenu());

                    OnScreenLog.AddWarning("Commerce:");
                    OnScreenLog.AddWarning("For commerce to work in the sample you must has a PSN account created for the SIEE region.");
                    OnScreenLog.AddWarning("When creating an Quick Signup account select a country, e.g. United Kingdom, that exists in the SIEE region.");
                    OnScreenLog.AddWarning("Currently entitlements have only been uploaded for the SIEE store region.");
                    OnScreenLog.AddWarning("Accounts in other regions won't be able to see the entitlements.");
                    OnScreenLog.AddWarning("Use Commerce to purchase consumables before using them in the Entitlements menu.");
                }

                if (m_MenuMain.AddItem("Entitlements", isRegistered == true))
                {
                    m_MenuStack.PushMenu(m_Entitlements.GetMenu());

                    OnScreenLog.AddWarning("Entitlements:");
                    OnScreenLog.AddWarning("For entitlements to work in the sample you must has a PSN account created for the SIEE region.");
                    OnScreenLog.AddWarning("When creating an Quick Signup account select a country, e.g. United Kingdom, that exists in the SIEE region.");
                    OnScreenLog.AddWarning("Currently entitlement have only been uploaded for the SIEE store region.");
                    OnScreenLog.AddWarning("Accounts in other regions won't be able to see the entitlements.");
                    OnScreenLog.AddWarning("Before being able to list and consume entitlements please use the Commerce menu option to purchase them.");
                }
#endif

                if (m_MenuMain.AddItem("Title Cloud Storage", "Change user based variables in the title cloud storage system.", isRegistered == true))
                {
                    m_MenuStack.PushMenu(m_TCS.GetMenu());
                }

                if (m_MenuMain.AddItem("Messages", isRegistered == true))
                {
                    m_MenuStack.PushMenu(m_Messages.GetMenu());
                }

                if (m_MenuMain.AddItem("User Profiles", "Get User profile and friends", isRegistered == true))
                {
                    m_MenuStack.PushMenu(m_UserProfiles.GetMenu());
                }

                if (m_MenuMain.AddItem("Web Events", "Get info on the currently active WebApi filters and events", isRegistered == true))
                {
                    m_MenuStack.PushMenu(m_WebEvents.GetMenu());
                }
#if UNITY_PS5
                if (m_MenuMain.AddItem("Universal Data System", isRegistered == true))
                {
                    m_MenuStack.PushMenu(m_UDS.GetMenu());
                }

                if (m_MenuMain.AddItem("Trophies", isRegistered == true))
                {
                    m_MenuStack.PushMenu(m_Trophies.GetMenu());
                }

                if (m_MenuMain.AddItem("Feature Gating", isRegistered == true))
                {
                    m_MenuStack.PushMenu(m_FeatureGating.GetMenu());
                }

                if (m_MenuMain.AddItem("Game Update", isRegistered == true))
                {
                    m_MenuStack.PushMenu(m_GameUpdate.GetMenu());
                }
#endif
                if (m_MenuMain.AddItem("Online Safety", isRegistered == true))
                {
                    m_MenuStack.PushMenu(m_OnlineSafety.GetMenu());
                }

                if (m_MenuMain.AddItem("Auth", isRegistered == true))
                {
                    m_MenuStack.PushMenu(m_SonyAuth.GetMenu());
                }

                if (m_MenuMain.AddItem("Bandwidth", isRegistered == true))
                {
                    m_MenuStack.PushMenu(m_Bandwidth.GetMenu());
                }

                if (m_MenuMain.AddItem("Shutdown", isRegistered == true))
                {
                    Unity.PSN.PS5.Main.ShutDown();
                    m_NpReady = false;
                }
            }
            else
            {
                if (m_MenuMain.AddItem("Initialize PSN"))
                {
                    initResult = Main.Initialize();
                    if (initResult.Initialized == true)
                    {
                        OnScreenLog.Add("PSN re-initialized");
                    }
                    else
                    {
                        OnScreenLog.Add("PSN not  re-initialized ");
                    }
                    m_NpReady = true;
                }
                if (m_MenuMain.AddItem("Exit Application"))
                {
                    Application.Quit();
                }
            }
        }

        public void OnEnter()
        {

        }

        public void OnExit()
        {
        }

        public void Process(MenuStack stack)
        {
            MenuMain();
        }

        void OnGUI()
        {
            MenuLayout activeMenu = m_MenuStack.GetMenu();
            activeMenu.GetOwner().Process(m_MenuStack);

            DisplayFilterList();
            DisplayPushEventsList();
            DisplayPendingRequestsList();

            string userOutput = User.Output();
            GUI.TextArea(new Rect(Screen.width * 0.01f, Screen.height * 0.01f, Screen.width * 0.23f, Screen.height * 0.07f), userOutput);

            if (GamePad.activeGamePad != null && GamePad.activeGamePad.IsSquarePressed)
            {
                // Clear the OnScreenLog.
                OnScreenLog.Clear();
            }

            GUI.TextArea(new Rect(Screen.width * 0.25f, Screen.height * 0.97f, Screen.width * 0.70f, Screen.height * 0.02f), "Press 'Square' to clear the screen log. Use Right stick to scroll the screen log. Press 'R3' to reset the scroll.");

            if (GamePad.activeGamePad != null)
            {
                Vector2 rightStick = GamePad.activeGamePad.GetThumbstickRight;

                if (rightStick.y > 0.1f)
                {
                    OnScreenLog.ScrollDown(rightStick.y * rightStick.y);
                }
                else if (rightStick.y < -0.1f)
                {
                    OnScreenLog.ScrollUp(rightStick.y * rightStick.y);
                }

                if (GamePad.activeGamePad.IsR3Pressed == true)
                {
                    OnScreenLog.ScrollReset();
                }
            }
        }

        public string GetDebugFilterOutput()
        {
            string output = "";

            output += String.Format("{0,-30}\n", "WebApiFilters");
            output += String.Format("{0,-10} {1,-10} {2,-20}\n", "Id", "Ref Count", "First Filter");

            List<WebApiFilters> filters = new List<WebApiFilters>();
            WebApiNotifications.GetActiveFilters(filters);

            for(int i = 0; i < filters.Count; i++)
            {
                string firstFilterText = filters[i].Filters != null && filters[i].Filters.Count > 0 ? filters[i].Filters[0].DataType : "";

                output += String.Format("{0,-10} {1,-10} {2,-20}\n", filters[i].PushFilterId, filters[i].RefCount, firstFilterText);
            }

            return output;
        }

        public string GetDebugPushEventsOutput()
        {
            string output = "";

            output += String.Format("{0,-30}\n", "WebApiPushEvent");
            output += String.Format("{0,-10} {1,-16} {2,-10} {3,-8}\n", "Id", "User Id", "Filter Id", "Ordered");

            List<WebApiPushEvent> pushEvents = new List<WebApiPushEvent>();
            WebApiNotifications.GetActivePushEvents(pushEvents);

            for (int i = 0; i < pushEvents.Count; i++)
            {
                int filterId = pushEvents[i].Filters != null ? pushEvents[i].Filters.PushFilterId : -1;

                output += String.Format("{0,-10} {1,-10} {2,-10} {3,-8}\n", pushEvents[i].PushCallbackId, "0x"+pushEvents[i].UserId.ToString("X8"), filterId, pushEvents[i].OrderGuaranteed);
            }

            return output;
        }

        public void DisplayFilterList()
        {
            string output = GetDebugFilterOutput();

            GUI.TextArea(new Rect(Screen.width * 0.01f, Screen.height * 0.82f, Screen.width * 0.28f, Screen.height * 0.14f), output);
        }

        public void DisplayPushEventsList()
        {
            string output = GetDebugPushEventsOutput();

            GUI.TextArea(new Rect(Screen.width * 0.30f, Screen.height * 0.82f, Screen.width * 0.28f, Screen.height * 0.14f), output);
        }

        public void DisplayPendingRequestsList()
        {
            if (GamePad.activeGamePad == null)
            {
                return;
            }

            //    string pendingOutput = RequestThread.GetPendingRequestDebugOutput();

            //     GUI.TextArea(new Rect(Screen.width * 0.01f, Screen.height * 0.8f, Screen.width * 0.23f, Screen.height * 0.17f), pendingOutput);

            //  pendingOutput += String.Format("{0,-12} {1,-30} {2,-6}\n", "Request Id", "Response Type");

            //foreach (var pendingRequest in pendingRequests)
            //{
            //    string responseTypeText;

            //    if (pendingRequest.Request != null)
            //    {
            //        responseTypeText = pendingRequest.Request.GetType().ToString();
            //    }
            //    else
            //    {
            //        responseTypeText = "None";
            //    }

            //    pendingOutput += String.Format("{0,-10} {1,-30} {2,-5}\n", pendingRequest.NpRequestId, responseTypeText, pendingRequest.AbortPending);
            //}

            //GUI.TextArea(new Rect(Screen.width * 0.01f, Screen.height * 0.8f, Screen.width * 0.23f, Screen.height * 0.17f), pendingOutput);
        }
#endif
    }
}
