using System;
using System.Collections.Generic;
using UnityEngine;


#if UNITY_PS5 || UNITY_PS4
using Unity.PSN.PS5;
using Unity.PSN.PS5.Aysnc;
#if UNITY_PS5
using Unity.PSN.PS5.PremiumFeatures;
#endif
#endif

namespace Com.MorganHouston.Imprecision
{
#if UNITY_PS5
    public static class PSFeatureGating
    {

        public static void Initialize()
        {
            if (premiumEventEnabled)
            {
                CheckPremium();
            }
            else
            {
                FeatureGating.OnPremiumNotification += OnPremiumNotification;
                EnablePremium();
            }
            
        }

        public static bool premiumEventEnabled = false;
        public static bool hasPremium = false;

        public static void EnablePremium()
        {
            FeatureGating.StartPremiumEventCallbackRequest request = new FeatureGating.StartPremiumEventCallbackRequest();

            var requestOp = new AsyncRequest<FeatureGating.StartPremiumEventCallbackRequest>(request).ContinueWith((antecedent) =>
            {
                if (PSNManager.CheckAysncRequestOK(antecedent))
                {
                    //OnScreenLog.Add("Premium events started");
                    premiumEventEnabled = true;
                    CheckPremium();
                }
            });

            FeatureGating.Schedule(requestOp);
        }

        public static void DisablePremium()
        {
            FeatureGating.StopPremiumEventCallbackRequest request = new FeatureGating.StopPremiumEventCallbackRequest();

            var requestOp = new AsyncRequest<FeatureGating.StopPremiumEventCallbackRequest>(request).ContinueWith((antecedent) =>
            {
                if (PSNManager.CheckAysncRequestOK(antecedent))
                {
                    //OnScreenLog.Add("Premium events stopped");
                    premiumEventEnabled = false;

                }
            });

            FeatureGating.Schedule(requestOp);
        }

        public static void CheckPremium()
        {
            FeatureGating.CheckPremiumRequest request = new FeatureGating.CheckPremiumRequest()
            {
                UserId = PSGamePad.activeGamePad.loggedInUser.userId
            };

            var requestOp = new AsyncRequest<FeatureGating.CheckPremiumRequest>(request).ContinueWith((antecedent) =>
            {
                if (PSNManager.CheckAysncRequestOK(antecedent))
                {
                    //Debug.Log("Premium Authorized = " + antecedent.Request.Authorized);
                    hasPremium = antecedent.Request.Authorized;
                }
            });

            FeatureGating.Schedule(requestOp);
        }

        public static void NotifyPremium()
        {
            FeatureGating.NotifyPremiumFeatureRequest request = new FeatureGating.NotifyPremiumFeatureRequest()
            {
                UserId = PSGamePad.activeGamePad.loggedInUser.userId,
                Properties = FeatureGating.MultiplayProperties.CrossPlatformPlay
            };

            var requestOp = new AsyncRequest<FeatureGating.NotifyPremiumFeatureRequest>(request).ContinueWith((antecedent) =>
            {
                if (PSNManager.CheckAysncRequestOK(antecedent))
                {
                    //OnScreenLog.Add("Notify Premium OK");
                }
                else
                {
                    //OnScreenLog.AddError("Notify Premium Failed : " + antecedent.Request.Result.sceErrorCode);
                }
            });

            FeatureGating.Schedule(requestOp);
        }

        public static void NotifyPremiumSpectate()
        {
            FeatureGating.NotifyPremiumFeatureRequest request = new FeatureGating.NotifyPremiumFeatureRequest()
            {
                UserId = PSGamePad.activeGamePad.loggedInUser.userId,
                Properties = FeatureGating.MultiplayProperties.InEngineSpectating
            };

            var requestOp = new AsyncRequest<FeatureGating.NotifyPremiumFeatureRequest>(request).ContinueWith((antecedent) =>
            {
                if (PSNManager.CheckAysncRequestOK(antecedent))
                {
                    //OnScreenLog.Add("Notify Premium OK");
                }
                else
                {
                    //OnScreenLog.AddError("Notify Premium Failed : " + antecedent.Request.Result.sceErrorCode);
                }
            });

            FeatureGating.Schedule(requestOp);
        }

        /*public void MenuFeatureGating(MenuStack menuStack)
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
        }*/

        private static void OnPremiumNotification(FeatureGating.PremiumEvent premiumEvent)
        {
            //Debug.Log("Premium Notification : ");
            //Debug.Log(System.String.Format("    UserId : 0x{0:X}", premiumEvent.UserId));
            //Debug.Log("    EventType : " + premiumEvent.EventType.ToString());
        }
    }
#endif
}
