using System;
using System.Collections.Generic;
using System.Linq;

#if UNITY_PS5 || UNITY_PS4
using Unity.PSN.PS5;
using Unity.PSN.PS5.Aysnc;
using Unity.PSN.PS5.Users;
using Unity.PSN.PS5.WebApi;
#endif

namespace PSNSample
{
#if UNITY_PS5 || UNITY_PS4
    public class SonyUserProfiles : IScreen
    {
        MenuLayout m_Menu;

        public SonyUserProfiles()
        {
            Initialize();

#if UNITY_PS5
            UserSystem.OnSignedInNotification += OnSignedInNotification;
            UserSystem.OnReachabilityNotification += OnReachabilityNotification;
#endif
        }

#if UNITY_PS5
        private void OnReachabilityNotification(UserSystem.ReachabilityEvent reachabilityEvent)
        {
            OnScreenLog.Add("Reachability Notification : ");
            OnScreenLog.Add(System.String.Format("    UserId : 0x{0:X}", reachabilityEvent.UserId));
            OnScreenLog.Add("    State : " + reachabilityEvent.State.ToString());
        }

        private void OnSignedInNotification(UserSystem.SignedInEvent signedInEvent)
        {
            OnScreenLog.Add("SignIn Notification : ");
            OnScreenLog.Add(System.String.Format("    UserId : 0x{0:X}", signedInEvent.UserId));
            OnScreenLog.Add("    State : " + signedInEvent.State.ToString());
        }
#endif

        public MenuLayout GetMenu()
        {
            return m_Menu;
        }

        public void OnEnter()
        {
        }

        public void OnExit()
        {
        }

        public void Process(MenuStack stack)
        {
            Menu(stack);
        }

        public void Initialize()
        {
            m_Menu = new MenuLayout(this, 450, 20);
        }

#if UNITY_PS5
        FrameCounterEvent reusableCounterEvent = new FrameCounterEvent();
#endif

        Dictionary<Int32, WebApiPushEvent> userCallbackIds = new Dictionary<int, WebApiPushEvent>();

        public void NotificationEventHandler(WebApiNotifications.CallbackParams eventData)
        {
            OnScreenLog.Add("Notification Event Handler");

            OnScreenLog.Add("   CtxId : " + eventData.CtxId);
            OnScreenLog.Add("   CallbackId : " + eventData.CallbackId);

            if (eventData.DataType != null)
            {
                OnScreenLog.Add("   DataType : " + eventData.DataType);
            }

            OnScreenLog.Add("   NpServiceName : " + eventData.NpServiceName);
            OnScreenLog.Add("   NpServiceLabel : " + eventData.NpServiceLabel);

            if (eventData.To != null)
            {
                OnScreenLog.Add("   To : " + eventData.To.AccountId.ToString("X16") + " : " + eventData.To.Platform);
            }

            if (eventData.ToOnlineId != null)
            {
                OnScreenLog.Add("   ToOnlineId : " + eventData.ToOnlineId);
            }

            if (eventData.From != null)
            {
                OnScreenLog.Add("   From : " + eventData.From.AccountId.ToString("X16") + " : " + eventData.From.Platform);
            }

            if (eventData.FromOnlineId != null)
            {
                OnScreenLog.Add("   FromOnlineId : " + eventData.FromOnlineId);
            }

            if (eventData.Data != null)
            {
                OnScreenLog.Add("   Data : " + eventData.Data);
            }
            else
            {
                OnScreenLog.Add("   Data : null");
            }

            if (eventData.ExtData != null)
            {
                OnScreenLog.Add("   ExtData : " + eventData.ExtData.Length);

                for (int i = 0; i < eventData.ExtData.Length; i++)
                {
                    OnScreenLog.Add("     Key : " + eventData.ExtData[i].Key);
                    OnScreenLog.Add("     Data : " + eventData.ExtData[i].Data);
                }
            }
            else
            {
                OnScreenLog.Add("   ExtData : null");
            }
        }

        WebApiFilters globalUserFilters;

#if UNITY_PS5
        bool signinEventEnabled = false;
        bool reachabilityEventEnabled = false;
#endif

        public void Menu(MenuStack menuStack)
        {
            m_Menu.Update();

            bool enabled = true;

            bool userHasPushEvent = userCallbackIds.ContainsKey(GamePad.activeGamePad.loggedInUser.userId);

            if (m_Menu.AddItem("Create Push Event", "Create a user push event with friends filters", enabled && userHasPushEvent == false))
            {
                WebApiPushEvent pushEvent = new WebApiPushEvent();

                if (globalUserFilters == null)
                {
                    globalUserFilters = new WebApiFilters();

                    WebApiFilter friendsFilter = globalUserFilters.AddFilterParam("np:service:friendlist:friend");
                    friendsFilter.ExtendedKeys = new List<string>() { "additionalTrigger" };

                    globalUserFilters.AddFilterParams(new string[] { "np:service:presence2:onlineStatus", "np:service:blocklist" });
                }

                pushEvent.Filters = globalUserFilters;
                pushEvent.UserId = GamePad.activeGamePad.loggedInUser.userId;
                pushEvent.OrderGuaranteed = false;

                WebApiNotifications.RegisterPushEventRequest request = new WebApiNotifications.RegisterPushEventRequest()
                {
                    PushEvent = pushEvent,
                    Callback = NotificationEventHandler
                };

                var requestOp = new AsyncRequest<WebApiNotifications.RegisterPushEventRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Event is registered");
                        // OnScreenLog.Add("Callback Id = " + antecedent.Request.PushEvent.PushCallbackId);

                        userCallbackIds.Add(antecedent.Request.PushEvent.UserId, antecedent.Request.PushEvent);

                        SonyWebApiEvents.OutputPushEventFull(antecedent.Request.PushEvent);

                        OnScreenLog.AddNewLine();
                        SonyWebApiEvents.OutputActiveEventsAndFilters();
                        OnScreenLog.AddNewLine();
                    }
                    else
                    {
                        OnScreenLog.AddError("Create PushEvent Request error");
                    }
                });

                WebApiNotifications.Schedule(requestOp);

                OnScreenLog.Add("Creating PushEvent...");
            }

            if (m_Menu.AddItem("Delete Push Event", "Delete the users push event with friends filters", enabled && userHasPushEvent == true))
            {
                WebApiPushEvent pushEvent;

                int userId = GamePad.activeGamePad.loggedInUser.userId;

                if (userCallbackIds.TryGetValue(userId, out pushEvent) == true)
                {
                    userCallbackIds.Remove(userId);

                    WebApiNotifications.UnregisterPushEventRequest request = new WebApiNotifications.UnregisterPushEventRequest()
                    {
                        PushEvent = pushEvent,
                    };

                    var requestOp = new AsyncRequest<WebApiNotifications.UnregisterPushEventRequest>(request).ContinueWith((antecedent) =>
                    {
                        if (SonyNpMain.CheckAysncRequestOK(antecedent))
                        {
                            OnScreenLog.Add("Event has been deleted");
                            SonyWebApiEvents.OutputPushEventFull(antecedent.Request.PushEvent);

                            OnScreenLog.AddNewLine();
                            SonyWebApiEvents.OutputActiveEventsAndFilters();
                            OnScreenLog.AddNewLine();
                        }
                        else
                        {
                            OnScreenLog.AddError("Delete PushEvent Request error");
                        }
                    });

                    WebApiNotifications.Schedule(requestOp);

                    OnScreenLog.Add("Deleteing PushEvent...");
                }
            }

            if (m_Menu.AddItem("Get Friends", "Get the list of friends for the current user", enabled))
            {
                UInt32 limit = 95;

                UserSystem.GetFriendsRequest request = new UserSystem.GetFriendsRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                    Offset = 0,
                    Limit = limit,
                    Filter = UserSystem.GetFriendsRequest.Filters.NotSet,
                    SortOrder = UserSystem.GetFriendsRequest.Order.OnlineId,
                };

                var requestOp = new AsyncRequest<UserSystem.GetFriendsRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Got Friends : ");
                        OnScreenLog.Add("   Next Offset : " + antecedent.Request.NextOffset);
                        OnScreenLog.Add("   Previous Offset : " + antecedent.Request.PreviousOffset);

                        OnScreenLog.Add("   Account Ids : ");

                        var accountIds = antecedent.Request.RetrievedAccountIds;

                        for (int i = 0; i < accountIds.Count; i++)
                        {
                            OnScreenLog.Add("       " + accountIds[i]);
                        }
                    }
                    else
                    {
                        OnScreenLog.AddError("Get Friends error");
                    }
                });

                UserSystem.Schedule(requestOp);

                OnScreenLog.Add("Getting Friends...");
            }

            if (m_Menu.AddItem("Get My Profile", "Get the current users profile", enabled))
            {
                UserSystem.GetProfilesRequest request = new UserSystem.GetProfilesRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                    AccountIds = null, //globalAccountIds,
                    RetrievedProfiles = new List<UserSystem.UserProfile>()
                };

                var requestOp = new AsyncRequest<UserSystem.GetProfilesRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Got Profiles : ");

                        OutputProfiles(antecedent.Request.RetrievedProfiles);
                    }
                    else
                    {
                        OnScreenLog.AddError("Get Profiles error");
                    }
                });

                UserSystem.Schedule(requestOp);

                OnScreenLog.Add("Getting Your Profile...");
            }

            if (m_Menu.AddItem("Get Friends Profiles", "Get the profiles of the current users friends", enabled))
            {
                UInt32 limit = 95;

                UserSystem.GetFriendsRequest friendRequest = new UserSystem.GetFriendsRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                    Offset = 0,
                    Limit = limit,
                    Filter = UserSystem.GetFriendsRequest.Filters.NotSet,
                    SortOrder = UserSystem.GetFriendsRequest.Order.OnlineId,
                };

                UserSystem.GetProfilesRequest profileRequest = new UserSystem.GetProfilesRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                    AccountIds = friendRequest.RetrievedAccountIds,
                    RetrievedProfiles = new List<UserSystem.UserProfile>()
                };

                var requestOp = new AsyncRequest<UserSystem.GetFriendsRequest>(friendRequest).ContinueWith(new AsyncRequest<UserSystem.GetProfilesRequest>(profileRequest)).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Got Friends Profiles : ");

                        OutputProfiles(antecedent.Request.RetrievedProfiles);
                    }
                    else
                    {
                        OnScreenLog.AddError("Get Profiles error");
                    }
                });

                UserSystem.Schedule(requestOp);

                OnScreenLog.Add("Getting Friends Profiles...");
            }

            if (m_Menu.AddItem("Get Friends Presence", "Get the presenses of the current users friends", enabled))
            {
                UInt32 limit = 20;

                UserSystem.GetFriendsRequest friendRequest = new UserSystem.GetFriendsRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                    Offset = 0,
                    Limit = limit,
                    Filter = UserSystem.GetFriendsRequest.Filters.NotSet,
                    SortOrder = UserSystem.GetFriendsRequest.Order.OnlineId,
                };

                UserSystem.GetBasicPresencesRequest presenceRequest = new UserSystem.GetBasicPresencesRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                    AccountIds = friendRequest.RetrievedAccountIds,
                    RetrievedPresences = new List<UserSystem.BasicPresence>()
                };

                var requestOp = new AsyncRequest<UserSystem.GetFriendsRequest>(friendRequest).ContinueWith(new AsyncRequest<UserSystem.GetBasicPresencesRequest>(presenceRequest)).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Got Friends Presences : ");

                        OutputPresences(antecedent.Request.RetrievedPresences);
                    }
                    else
                    {
                        OnScreenLog.AddError("Get Profiles error");
                    }
                });

                UserSystem.Schedule(requestOp);

                OnScreenLog.Add("Getting Friends Profiles...");
            }

            if (m_Menu.AddItem("Get Blocking Users", "Get the list of users blocked by the current user", enabled))
            {
                UInt32 limit = 95;

                UserSystem.GetBlockingUsersRequest request = new UserSystem.GetBlockingUsersRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                    Offset = 0,
                    Limit = limit,
                    RetrievedAccountIds = new System.Collections.Generic.List<UInt64>((int)limit)
                };

                var requestOp = new AsyncRequest<UserSystem.GetBlockingUsersRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Got Blocked Users : ");
                        OnScreenLog.Add("   Account Ids : ");

                        var accountIds = antecedent.Request.RetrievedAccountIds;

                        for (int i = 0; i < accountIds.Count; i++)
                        {
                            OnScreenLog.Add("       " + accountIds[i]);
                        }

                        OnScreenLog.Add("   NextOffset : " + antecedent.Request.NextOffset);
                        OnScreenLog.Add("   PreviousOffset : " + antecedent.Request.PreviousOffset);
                        OnScreenLog.Add("   TotalItemCount : " + antecedent.Request.TotalItemCount);
                    }
                    else
                    {
                        OnScreenLog.AddError("Get Blocked Users error");
                    }
                });

                UserSystem.Schedule(requestOp);

                OnScreenLog.Add("Getting Blocked users...");
            }

#if UNITY_PS5
            if (m_Menu.AddItem("Enable SignIn Notifications", "Enable notifications for Signin/out events", !signinEventEnabled))
            {
                UserSystem.StartSignedStateCallbackRequest request = new UserSystem.StartSignedStateCallbackRequest();

                var requestOp = new AsyncRequest<UserSystem.StartSignedStateCallbackRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Signin events started");
                        signinEventEnabled = true;
                    }
                });

                UserSystem.Schedule(requestOp);
            }

            if (m_Menu.AddItem("Disable SignIn Notifications", "Enable notifications for premium events", signinEventEnabled))
            {
                UserSystem.StopSignedStateCallbackRequest request = new UserSystem.StopSignedStateCallbackRequest();

                var requestOp = new AsyncRequest<UserSystem.StopSignedStateCallbackRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Signin events stopped");
                        signinEventEnabled = false;

                    }
                });

                UserSystem.Schedule(requestOp);
            }

            if (m_Menu.AddItem("Enable Reachability Notifications", "Enable notifications for PSN reachability events", !reachabilityEventEnabled))
            {
                UserSystem.StartReachabilityStateCallbackRequest request = new UserSystem.StartReachabilityStateCallbackRequest();

                var requestOp = new AsyncRequest<UserSystem.StartReachabilityStateCallbackRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Signin events started");
                        reachabilityEventEnabled = true;
                    }
                });

                UserSystem.Schedule(requestOp);
            }

            if (m_Menu.AddItem("Disable Reachability Notifications", "Enable notifications for PSN reachability events", reachabilityEventEnabled))
            {
                UserSystem.StopReachabilityStateCallbackRequest request = new UserSystem.StopReachabilityStateCallbackRequest();

                var requestOp = new AsyncRequest<UserSystem.StopReachabilityStateCallbackRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Signin events stopped");
                        reachabilityEventEnabled = false;
                    }
                });

                UserSystem.Schedule(requestOp);
            }
#endif

            if (m_Menu.AddBackIndex("Back"))
            {
                menuStack.PopMenu();
            }
        }

        void OutputProfiles(List<UserSystem.UserProfile> userProfiles)
        {
            OnScreenLog.Add("   Num Profiles : " + userProfiles.Count);

            for (int i = 0; i < userProfiles.Count; i++)
            {
                OutputProfile(userProfiles[i], 8);
            }
        }

        void OutputProfile(UserSystem.UserProfile userProfile, int indent)
        {
            OnScreenLog.Add("    Profile:");
            OnScreenLog.Add("        Online Id: " + userProfile.OnlineId);
            if (userProfile.PersonalDetails != null)
            {
                var personalDetails = userProfile.PersonalDetails;

                OnScreenLog.Add("        Personal Details : ");
                OnScreenLog.Add("            First Name : " + personalDetails.FirstName);
                OnScreenLog.Add("            Middle Name : " + personalDetails.MiddleName);
                OnScreenLog.Add("            Last Name : " + personalDetails.LastName);
                OnScreenLog.Add("            Display Name : " + personalDetails.DisplayName);

                if (personalDetails.ProfilePictures != null && personalDetails.ProfilePictures.Count > 0)
                {
                    OnScreenLog.Add("            Pictures : ");

                    for (int p = 0; p < personalDetails.ProfilePictures.Count; p++)
                    {
                        var picture = personalDetails.ProfilePictures[p];

                        OnScreenLog.Add("                Size : " + picture.Size);
                        OnScreenLog.Add("                Url : " + picture.Url);
                    }
                }
            }
            OnScreenLog.Add("        About Me : " + userProfile.AboutMe);

            if (userProfile.Avatars != null && userProfile.Avatars.Count > 0)
            {
                var avatars = userProfile.Avatars;

                OnScreenLog.Add("        Avatars : ");

                for (int a = 0; a < avatars.Count; a++)
                {
                    var avatar = avatars[a];

                    OnScreenLog.Add("            Size : " + avatar.Size);
                    OnScreenLog.Add("            Url : " + avatar.Url);
                }
            }

            if (userProfile.Languages != null && userProfile.Languages.Count > 0)
            {
                var languages = userProfile.Languages;

                OnScreenLog.Add("        Languages : ");

                for (int l = 0; l < languages.Count; l++)
                {
                    OnScreenLog.Add("            " + languages[l]);
                }
            }

            OnScreenLog.Add("        Verified : " + userProfile.VerifiedState);
        }

        void OutputPresences(List<UserSystem.BasicPresence> presences)
        {
            OnScreenLog.Add("   Num Presences : " + presences.Count);

            for (int i = 0; i < presences.Count; i++)
            {
                OutputPresences(presences[i], 8);
            }
        }

        void OutputPresences(UserSystem.BasicPresence presence, int indent)
        {
            OnScreenLog.Add("    Profile:");
            OnScreenLog.Add("        AccountId : " + presence.AccountId.ToString("X16"));
            OnScreenLog.Add("        OnlineStatus : " + presence.OnlineStatus);
            OnScreenLog.Add("        InContext : " + presence.InContext);
        }
    }
#endif
    }
