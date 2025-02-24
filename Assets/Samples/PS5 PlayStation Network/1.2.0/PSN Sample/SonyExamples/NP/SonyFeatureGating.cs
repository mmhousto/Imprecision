using System;
using System.Collections.Generic;

#if UNITY_PS5 || UNITY_PS4
using Unity.PSN.PS5;
using Unity.PSN.PS5.Aysnc;
#if UNITY_PS5
using Unity.PSN.PS5.PremiumFeatures;
#endif
#endif

namespace PSNSample
{
#if UNITY_PS5
    public class SonyFeatureGating : IScreen
    {
        MenuLayout m_MenuFeatureGating;

        public SonyFeatureGating()
        {
            Initialize();

            FeatureGating.OnPremiumNotification += OnPremiumNotification;
        }

        public MenuLayout GetMenu()
        {
            return m_MenuFeatureGating;
        }

        public void OnEnter()
        {
        }

        public void OnExit()
        {
        }

        public void Process(MenuStack stack)
        {
            MenuFeatureGating(stack);
        }

        public void Initialize()
        {
            m_MenuFeatureGating = new MenuLayout(this, 450, 20);
        }

        bool premiumEventEnabled = false;

        public void MenuFeatureGating(MenuStack menuStack)
        {
            m_MenuFeatureGating.Update();

            bool enabled = true;

            if (m_MenuFeatureGating.AddItem("Check Premium", "Check if a user is allowed premium features", enabled))
            {
                FeatureGating.CheckPremiumRequest request = new FeatureGating.CheckPremiumRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId
                };

                var requestOp = new AsyncRequest<FeatureGating.CheckPremiumRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Premium Authorized = " + antecedent.Request.Authorized);
                    }
                });

                FeatureGating.Schedule(requestOp);
            }

            if (m_MenuFeatureGating.AddItem("Notify Premium Feature", "Notify a premium feature is being used by the current user", enabled))
            {
                FeatureGating.NotifyPremiumFeatureRequest request = new FeatureGating.NotifyPremiumFeatureRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                    Properties = FeatureGating.MultiplayProperties.InEngineSpectating
                };

                var requestOp = new AsyncRequest<FeatureGating.NotifyPremiumFeatureRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Notify Premium OK");
                    }
                    else
                    {
                        OnScreenLog.AddError("Notify Premium Failed : " + antecedent.Request.Result.sceErrorCode);
                    }
                });

                FeatureGating.Schedule(requestOp);
            }

            if (m_MenuFeatureGating.AddItem("Enable Premium Events", "Enable notifications for premium events", !premiumEventEnabled))
            {
                FeatureGating.StartPremiumEventCallbackRequest request = new FeatureGating.StartPremiumEventCallbackRequest();

                var requestOp = new AsyncRequest<FeatureGating.StartPremiumEventCallbackRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Premium events started");
                        premiumEventEnabled = true;
                    }
                });

                FeatureGating.Schedule(requestOp);
            }

            if (m_MenuFeatureGating.AddItem("Disable Premium Events", "Enable notifications for premium events", premiumEventEnabled))
            {
                FeatureGating.StopPremiumEventCallbackRequest request = new FeatureGating.StopPremiumEventCallbackRequest();

                var requestOp = new AsyncRequest<FeatureGating.StopPremiumEventCallbackRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Premium events stopped");
                        premiumEventEnabled = false;

                    }
                });

                FeatureGating.Schedule(requestOp);
            }

            if (m_MenuFeatureGating.AddBackIndex("Back"))
            {
                menuStack.PopMenu();
            }
        }

        private void OnPremiumNotification(FeatureGating.PremiumEvent premiumEvent)
        {
            OnScreenLog.Add("Premium Notification : ");
            OnScreenLog.Add(System.String.Format("    UserId : 0x{0:X}", premiumEvent.UserId));
            OnScreenLog.Add("    EventType : " + premiumEvent.EventType.ToString());
        }
    }
#endif
}
