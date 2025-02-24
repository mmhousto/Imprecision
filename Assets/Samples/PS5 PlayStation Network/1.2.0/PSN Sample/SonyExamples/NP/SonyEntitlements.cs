using System;
using System.Collections.Generic;

#if UNITY_PS5 || UNITY_PS4
using Unity.PSN.PS5;
using Unity.PSN.PS5.Aysnc;

#if UNITY_PS5
using Unity.PSN.PS5.Entitlement;
using Unity.PSN.PS5.PremiumFeatures;
#endif

#endif

namespace PSNSample
{
#if UNITY_PS5
    public class SonyEntitlements : IScreen
    {
        MenuLayout m_MenuEntitlements;

        public SonyEntitlements()
        {
            Initialize();
        }

        public MenuLayout GetMenu()
        {
            return m_MenuEntitlements;
        }

        public void OnEnter()
        {

        }

        public void OnExit()
        {
        }

        public void Process(MenuStack stack)
        {
            MenuEntitlements(stack);
        }

        public void Initialize()
        {
            m_MenuEntitlements = new MenuLayout(this, 450, 20);
        }

        Entitlements.AdditionalContentEntitlementInfo[] currentAddEntitlements = null;
        Entitlements.UnifiedEntitlementInfo[] currentUnifiedEntitlements = null;
        Entitlements.ServiceEntitlementInfo[] currentServiceEntitlements = null;

        public void MenuEntitlements(MenuStack menuStack)
        {
            m_MenuEntitlements.Update();

            bool enabled = true;

            if (m_MenuEntitlements.AddItem("Get Sku", "Get the SKU type", enabled))
            {
                Entitlements.GetSkuFlagRequest request = new Entitlements.GetSkuFlagRequest();

                var requestOp = new AsyncRequest<Entitlements.GetSkuFlagRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("  SkuType : " + antecedent.Request.SkuType);
                    }
                });

                OnScreenLog.Add("Getting sku...");

                Entitlements.Schedule(requestOp);
            }

            if (m_MenuEntitlements.AddItem("Get Additional Content List", "Get additional content entitlements", enabled))
            {
                Entitlements.GetAdditionalContentEntitlementListRequest request = new Entitlements.GetAdditionalContentEntitlementListRequest()
                {
                    ServiceLabel = 0
                };

                var requestOp = new AsyncRequest<Entitlements.GetAdditionalContentEntitlementListRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OutputAdditionalContentList(antecedent.Request.Entitlements);
                    }
                });

                OnScreenLog.Add("Getting additional content entitlements...");

                Entitlements.Schedule(requestOp);
            }

            if (m_MenuEntitlements.AddItem("Get Additional Content", "Get additional content entitlement", enabled && currentAddEntitlements != null && currentAddEntitlements.Length > 0))
            {
                Entitlements.GetAdditionalContentEntitlementInfoRequest request = new Entitlements.GetAdditionalContentEntitlementInfoRequest()
                {
                    ServiceLabel = 0,
                    EntitlementLabel = currentAddEntitlements[0].EntitlementLabel
                };

                var requestOp = new AsyncRequest<Entitlements.GetAdditionalContentEntitlementInfoRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OutputAdditionalContent(antecedent.Request.Entitlement);
                    }
                });

                OnScreenLog.Add("Getting additional content entitlement label : " + request.EntitlementLabel);

                Entitlements.Schedule(requestOp);
            }

            if (m_MenuEntitlements.AddItem("Get Entitlement key", "Get the entitlement key of additional content", enabled && currentAddEntitlements != null && currentAddEntitlements.Length > 0))
            {
                Entitlements.GetEntitlementKeyRequest request = new Entitlements.GetEntitlementKeyRequest()
                {
                    ServiceLabel = 0,
                    EntitlementLabel = currentAddEntitlements[0].EntitlementLabel
                };

                var requestOp = new AsyncRequest<Entitlements.GetEntitlementKeyRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("  EntitlementKey : " + antecedent.Request.EntitlementKey);
                    }
                });

                OnScreenLog.Add("Getting Entitlement Key for entitlement label : " + request.EntitlementLabel);

                Entitlements.Schedule(requestOp);
            }

            if (m_MenuEntitlements.AddItem("Get Unified Entitlement List", "Get unified entitlements", enabled))
            {
                Entitlements.GetUnifiedEntitlementInfoListRequest request = new Entitlements.GetUnifiedEntitlementInfoListRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                    ServiceLabel = 0,
                    Sort = Entitlements.SortTypes.ActiveData,
                    SortDirection = Entitlements.SortOrders.Ascending,
                    PackageType = Entitlements.EntitlementAccessPackageType.PSCONS
                };

                var requestOp = new AsyncRequest<Entitlements.GetUnifiedEntitlementInfoListRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OutputUnifiedEntitlementsList(antecedent.Request.Entitlements);

                        currentUnifiedEntitlements = antecedent.Request.Entitlements;
                    }
                });

                OnScreenLog.Add("Getting unified entitlements...");

                Entitlements.Schedule(requestOp);
            }

            bool unifiedEnabled = enabled && currentUnifiedEntitlements != null && currentUnifiedEntitlements.Length > 0;

            if (m_MenuEntitlements.AddItem("Get Unified Entitlement", "Get a unified entitlement", unifiedEnabled))
            {
                Entitlements.GetUnifiedEntitlementInfoRequest request = new Entitlements.GetUnifiedEntitlementInfoRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                    ServiceLabel = 0,
                    EntitlementLabel = currentUnifiedEntitlements[0].EntitlementLabel
                };

                var requestOp = new AsyncRequest<Entitlements.GetUnifiedEntitlementInfoRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OutputUnifiedEntitlement(antecedent.Request.Entitlement);
                    }
                });

                OnScreenLog.Add("Getting unified entitlement...");

                Entitlements.Schedule(requestOp);
            }

            if (m_MenuEntitlements.AddItem("Consume Unified Entitlement", "Consume a unified entitlement", unifiedEnabled))
            {
                Entitlements.ConsumeUnifiedEntitlementRequest request = new Entitlements.ConsumeUnifiedEntitlementRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                    ServiceLabel = 0,
                    UseCount = 1,
                    EntitlementLabel = currentUnifiedEntitlements[0].EntitlementLabel
                };

                var requestOp = new AsyncRequest<Entitlements.ConsumeUnifiedEntitlementRequest>(request).ContinueWith((consumeAntecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(consumeAntecedent))
                    {
                        OnScreenLog.Add("Consume Complete : ");
                        OnScreenLog.Add("      EntitlementLabel : " + consumeAntecedent.Request.EntitlementLabel);
                        OnScreenLog.Add("      UseCount : " + consumeAntecedent.Request.UseCount);
                        OnScreenLog.Add("      UseLimit : " + consumeAntecedent.Request.UseLimit);
                    }
                });

                OnScreenLog.Add("Consuming unified entitlement...");

                Entitlements.Schedule(requestOp);
            }

            if (m_MenuEntitlements.AddItem("Get Service Entitlement List", "Get service entitlements", enabled))
            {
                Entitlements.GetServiceEntitlementInfoListRequest request = new Entitlements.GetServiceEntitlementInfoListRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                    ServiceLabel = 0,
                };

                var requestOp = new AsyncRequest<Entitlements.GetServiceEntitlementInfoListRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OutputServiceEntitlementsList(antecedent.Request.Entitlements);

                        currentServiceEntitlements = antecedent.Request.Entitlements;
                    }
                });

                OnScreenLog.Add("Getting unified entitlements...");

                Entitlements.Schedule(requestOp);
            }

            bool serviceEnabled = enabled && currentServiceEntitlements != null && currentServiceEntitlements.Length > 0;

            if (m_MenuEntitlements.AddItem("Get Service Entitlement", "Get a service entitlement", serviceEnabled))
            {
                Entitlements.GetServiceEntitlementInfoRequest request = new Entitlements.GetServiceEntitlementInfoRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                    ServiceLabel = 0,
                    EntitlementLabel = currentServiceEntitlements[0].EntitlementLabel
                };

                var requestOp = new AsyncRequest<Entitlements.GetServiceEntitlementInfoRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OutputServiceEntitlement(antecedent.Request.Entitlement);
                    }
                });

                OnScreenLog.Add("Getting unified entitlement...");

                Entitlements.Schedule(requestOp);
            }

            if (m_MenuEntitlements.AddBackIndex("Back"))
            {
                menuStack.PopMenu();
            }
        }

        private void OutputUnifiedEntitlement(Entitlements.UnifiedEntitlementInfo entitlement)
        {
            OnScreenLog.Add("Unified Entitlements : ");

            if (entitlement == null)
            {
                OnScreenLog.Add("    Null entitlement");
            }
            else
            {
                OnScreenLog.Add("      EntitlementLabel : " + entitlement.EntitlementLabel);
                OnScreenLog.Add("      EntitlementType : " + entitlement.EntitlementType);
                OnScreenLog.Add("      PackageType : " + entitlement.PackageType);
                OnScreenLog.Add("      IsActive : " + entitlement.IsActive);
                OnScreenLog.Add("      UseCount : " + entitlement.UseCount);
                OnScreenLog.Add("      UseLimit : " + entitlement.UseLimit);
                OnScreenLog.Add("      ActiveDate : " + entitlement.ActiveDate);
                OnScreenLog.Add("      InactiveDate : " + entitlement.InactiveDate);
                OnScreenLog.AddNewLine();
            }
        }

        private void OutputUnifiedEntitlementsList(Entitlements.UnifiedEntitlementInfo[] entitlements)
        {
            OnScreenLog.Add("Unified Entitlements : ");

            if (entitlements == null)
            {
                OnScreenLog.Add("    No entitlements");
            }
            else
            {
                for (int i = 0; i < entitlements.Length; i++)
                {
                    OnScreenLog.Add("      EntitlementLabel : " + entitlements[i].EntitlementLabel);
                    OnScreenLog.Add("      EntitlementType : " + entitlements[i].EntitlementType);
                    OnScreenLog.Add("      PackageType : " + entitlements[i].PackageType);
                    OnScreenLog.Add("      IsActive : " + entitlements[i].IsActive);
                    OnScreenLog.Add("      UseCount : " + entitlements[i].UseCount);
                    OnScreenLog.Add("      UseLimit : " + entitlements[i].UseLimit);
                    OnScreenLog.Add("      ActiveDate : " + entitlements[i].ActiveDate);
                    OnScreenLog.Add("      InactiveDate : " + entitlements[i].InactiveDate);
                    OnScreenLog.AddNewLine();
                }
            }
        }

        private void OutputServiceEntitlement(Entitlements.ServiceEntitlementInfo entitlement)
        {
            OnScreenLog.Add("Service Entitlements : ");

            if (entitlement == null)
            {
                OnScreenLog.Add("    Null entitlement");
            }
            else
            {
                OnScreenLog.Add("      EntitlementLabel : " + entitlement.EntitlementLabel);
                OnScreenLog.Add("      EntitlementType : " + entitlement.EntitlementType);
                OnScreenLog.Add("      IsActive : " + entitlement.IsActive);
                OnScreenLog.Add("      UseCount : " + entitlement.UseCount);
                OnScreenLog.Add("      UseLimit : " + entitlement.UseLimit);
                OnScreenLog.Add("      IsConsumable : " + entitlement.IsConsumable);
                OnScreenLog.Add("      ActiveDate : " + entitlement.ActiveDate);
                OnScreenLog.Add("      InactiveDate : " + entitlement.InactiveDate);
                OnScreenLog.AddNewLine();
            }
        }

        private void OutputServiceEntitlementsList(Entitlements.ServiceEntitlementInfo[] entitlements)
        {
            OnScreenLog.Add("Service Entitlements : ");

            if (entitlements == null)
            {
                OnScreenLog.Add("    No entitlements");
            }
            else
            {
                for (int i = 0; i < entitlements.Length; i++)
                {
                    OnScreenLog.Add("      EntitlementLabel : " + entitlements[i].EntitlementLabel);
                    OnScreenLog.Add("      EntitlementType : " + entitlements[i].EntitlementType);
                    OnScreenLog.Add("      IsActive : " + entitlements[i].IsActive);
                    OnScreenLog.Add("      UseCount : " + entitlements[i].UseCount);
                    OnScreenLog.Add("      UseLimit : " + entitlements[i].UseLimit);
                    OnScreenLog.Add("      IsConsumable : " + entitlements[i].IsConsumable);
                    OnScreenLog.Add("      ActiveDate : " + entitlements[i].ActiveDate);
                    OnScreenLog.Add("      InactiveDate : " + entitlements[i].InactiveDate);
                    OnScreenLog.AddNewLine();
                }
            }
        }

        private void OutputAdditionalContentList(Entitlements.AdditionalContentEntitlementInfo[] entitlements)
        {
            OnScreenLog.Add("Aditional Content : ");

            if (entitlements == null)
            {
                OnScreenLog.Add("    No entitlements");
            }
            else
            {
                for (int i = 0; i < entitlements.Length; i++)
                {
                    OnScreenLog.Add("      EntitlementLabel : " + entitlements[i].EntitlementLabel);
                    OnScreenLog.Add("      PackageType : " + entitlements[i].PackageType);
                    OnScreenLog.Add("      DownloadStatus : " + entitlements[i].DownloadStatus);

                    OnScreenLog.AddNewLine();
                }
            }
        }

        private void OutputAdditionalContent(Entitlements.AdditionalContentEntitlementInfo entitlement)
        {
            OnScreenLog.Add("Aditional Content : ");

            if (entitlement == null)
            {
                OnScreenLog.Add("    No entitlement");
            }
            else
            {
                OnScreenLog.Add("      EntitlementLabel : " + entitlement.EntitlementLabel);
                OnScreenLog.Add("      PackageType : " + entitlement.PackageType);
                OnScreenLog.Add("      DownloadStatus : " + entitlement.DownloadStatus);
            }
        }
    }
#endif
}
