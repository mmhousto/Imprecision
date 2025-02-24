using System;
using System.Collections.Generic;

#if UNITY_PS5 || UNITY_PS4
using Unity.PSN.PS5;
using Unity.PSN.PS5.Aysnc;

#if UNITY_PS5
using Unity.PSN.PS5.Commerce;
using Unity.PSN.PS5.Dialogs;
using UnityEngine;
#endif

#endif

namespace PSNSample
{
#if UNITY_PS5
    public class SonyCommerce : IScreen
    {
        MenuLayout m_MenuCommerce;

        public SonyCommerce()
        {
            Initialize();
        }

        public MenuLayout GetMenu()
        {
            return m_MenuCommerce;
        }

        public void OnEnter()
        {
        }

        public void OnExit()
        {
        }

        public void Update()
        {
            if (forceCloseRequest != null)
            {
                closeTimer -= Time.deltaTime;
                if (closeTimer < 0.0f)
                {
                    forceCloseRequest.CloseDialog();
                    OnScreenLog.AddWarning("Dialog Set to Force Close...");
                    forceCloseRequest = null;
                }
            }
        }

        public void Process(MenuStack stack)
        {
            MenuEntitlements(stack);
        }

        public void Initialize()
        {
            m_MenuCommerce = new MenuLayout(this, 450, 20);
        }

        CommerceDialogSystem.OpenJoinPremiumDialogRequest forceCloseRequest = null;
        float closeTimer = 0.0f;

        public void MenuEntitlements(MenuStack menuStack)
        {
            m_MenuCommerce.Update();

            bool userEnabled = false;

            if (GamePad.activeGamePad != null) //&& GamePad.activeGamePad.loggedInUser.onlineStatus == PlatformInput.OnlineStatus.SignedIn)
            {
                userEnabled = true;
            }

            if (m_MenuCommerce.AddItem("Show Store Icon", "Display store icon.", userEnabled))
            {
                CommerceSystem.PSStoreIconRequest request = new CommerceSystem.PSStoreIconRequest()
                {
                    Mode = CommerceSystem.PSStoreIconRequest.DisplayModes.Show,
                    IconPosition = CommerceSystem.PSStoreIconRequest.IconPositions.Left
                };

                var requestOp = new AsyncRequest<CommerceSystem.PSStoreIconRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                    }
                });

                OnScreenLog.Add("Showing Icon ...");

                CommerceSystem.Schedule(requestOp);
            }

            if (m_MenuCommerce.AddItem("Hide Store Icon", "Hide the store icon.", userEnabled))
            {
                CommerceSystem.PSStoreIconRequest request = new CommerceSystem.PSStoreIconRequest()
                {
                    Mode = CommerceSystem.PSStoreIconRequest.DisplayModes.Hide,
                };

                var requestOp = new AsyncRequest<CommerceSystem.PSStoreIconRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                    }
                });

                OnScreenLog.Add("Hiding Icon ...");

                CommerceSystem.Schedule(requestOp);
            }

            if (m_MenuCommerce.AddItem("Open Join Premium Dialog", "Open the join premium dialog for the current user", userEnabled))
            {
                CommerceDialogSystem.OpenJoinPremiumDialogRequest request = new CommerceDialogSystem.OpenJoinPremiumDialogRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                };

                var requestOp = new AsyncRequest<CommerceDialogSystem.OpenJoinPremiumDialogRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Dialog Status : " + antecedent.Request.Status);
                        OnScreenLog.Add("Authorized : " + antecedent.Request.Authorized);

                        OnScreenLog.Add("Dialog Closed...");
                    }
                });

                OnScreenLog.Add("Opening Dialog ...");

                DialogSystem.Schedule(requestOp);
            }

            if (m_MenuCommerce.AddItem("Open Join Dialog and Close", "Open the join premium dialog for the current user and force close the dialog after 5 seconds", userEnabled && forceCloseRequest == null))
            {
                CommerceDialogSystem.OpenJoinPremiumDialogRequest request = new CommerceDialogSystem.OpenJoinPremiumDialogRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                };

                var requestOp = new AsyncRequest<CommerceDialogSystem.OpenJoinPremiumDialogRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Dialog Status : " + antecedent.Request.Status);

                        OnScreenLog.Add("Dialog Closed...");
                    }
                });

                OnScreenLog.Add("Opening Dialog ...");

                DialogSystem.Schedule(requestOp);

                forceCloseRequest = request;
                closeTimer = 5.0f;

            }

            if (m_MenuCommerce.AddItem("Open Commerce Category", "Open the commerce dialog for Categorys", userEnabled && forceCloseRequest == null))
            {
                List<string> categories = new List<string>();
                categories.Add("TESTENTITLEMENT");

                CommerceDialogSystem.OpenBrowseCategoryDialogRequest request = new CommerceDialogSystem.OpenBrowseCategoryDialogRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                    Targets = categories.ToArray()
                };

                var requestOp = new AsyncRequest<CommerceDialogSystem.OpenBrowseCategoryDialogRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Dialog Status : " + antecedent.Request.Status);

                        OnScreenLog.Add("Dialog Closed...");
                    }
                });

                OnScreenLog.Add("Opening Dialog ...");

                DialogSystem.Schedule(requestOp);
            }

            if (m_MenuCommerce.AddItem("Open Commerce Products", "Open the commerce dialog for Products", userEnabled && forceCloseRequest == null))
            {
                List<string> targets = new List<string>();
                // Target number taken from Product ID of Entitlement. EP6340-PPSA01684_00-4068583663090088
                targets.Add("4068583663090088");

                CommerceDialogSystem.OpenBrowseProductDialogRequest request = new CommerceDialogSystem.OpenBrowseProductDialogRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                    Targets = targets.ToArray()
                };

                var requestOp = new AsyncRequest<CommerceDialogSystem.OpenBrowseProductDialogRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Dialog Status : " + antecedent.Request.Status);

                        OnScreenLog.Add("Dialog Closed...");
                    }
                });

                OnScreenLog.Add("Opening Dialog ...");

                DialogSystem.Schedule(requestOp);
            }

            if (m_MenuCommerce.AddItem("Open Commerce Product Code", "Open the commerce dialog for with an empty Product Code", userEnabled && forceCloseRequest == null))
            {
                List<string> targets = new List<string>();

                CommerceDialogSystem.OpenRedeemPromotionCodeDialogRequest request = new CommerceDialogSystem.OpenRedeemPromotionCodeDialogRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                };

                var requestOp = new AsyncRequest<CommerceDialogSystem.OpenRedeemPromotionCodeDialogRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Dialog Status : " + antecedent.Request.Status);

                        OnScreenLog.Add("Dialog Closed...");
                    }
                });

                OnScreenLog.Add("Opening Dialog ...");

                DialogSystem.Schedule(requestOp);
            }

            if (m_MenuCommerce.AddItem("Open Commerce Product Code (2)", "Open the commerce dialog with a pre-filled Product Code", userEnabled && forceCloseRequest == null))
            {
                List<string> targets = new List<string>();
                targets.Add("1234ABCD5678");

                CommerceDialogSystem.OpenRedeemPromotionCodeDialogRequest request = new CommerceDialogSystem.OpenRedeemPromotionCodeDialogRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                    Targets = targets.ToArray()
                };

                var requestOp = new AsyncRequest<CommerceDialogSystem.OpenRedeemPromotionCodeDialogRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Dialog Status : " + antecedent.Request.Status);

                        OnScreenLog.Add("Dialog Closed...");
                    }
                });

                OnScreenLog.Add("Opening Dialog ...");

                DialogSystem.Schedule(requestOp);
            }

            if (m_MenuCommerce.AddItem("Open Commerce Checkout", "Open the commerce dialog for Checkout", userEnabled && forceCloseRequest == null))
            {
                List<string> targets = new List<string>();
                // Target numbers taken from Product ID of Entitlement. EP6340-PPSA01684_00-4068583663090088 and EP6340-PPSA01684_00-EXTRALIVESENTITL
                targets.Add("4068583663090088");
                targets.Add("EXTRALIVESENTITL");

                CommerceDialogSystem.OpenCheckoutDialogRequest request = new CommerceDialogSystem.OpenCheckoutDialogRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                    Targets = targets.ToArray(),
                };

                var requestOp = new AsyncRequest<CommerceDialogSystem.OpenCheckoutDialogRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Dialog Status : " + antecedent.Request.Status);

                        OnScreenLog.Add("Dialog Closed...");
                    }
                });

                OnScreenLog.Add("Opening Dialog ...");

                DialogSystem.Schedule(requestOp);
            }

            if (m_MenuCommerce.AddItem("Open Commerce Download", "Open the commerce dialog for Download", userEnabled && forceCloseRequest == null))
            {
                CommerceDialogSystem.OpenDownloadDialogRequest request = new CommerceDialogSystem.OpenDownloadDialogRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                };

                var requestOp = new AsyncRequest<CommerceDialogSystem.OpenDownloadDialogRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Dialog Status : " + antecedent.Request.Status);

                        OnScreenLog.Add("Dialog Closed...");
                    }
                });

                OnScreenLog.Add("Opening Dialog ...");

                DialogSystem.Schedule(requestOp);
            }

            if (m_MenuCommerce.AddBackIndex("Back"))
            {
                menuStack.PopMenu();
            }
        }
    }
#endif
}
