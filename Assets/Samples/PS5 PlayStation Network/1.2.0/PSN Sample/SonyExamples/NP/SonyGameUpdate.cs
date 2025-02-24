using System;
using System.Collections.Generic;

#if UNITY_PS5 || UNITY_PS4
using Unity.PSN.PS5;
using Unity.PSN.PS5.Aysnc;
#endif

namespace PSNSample
{
#if UNITY_PS5
    public class SonyGameUpdate : IScreen
    {
        MenuLayout m_MenuGameUpdate;

        public SonyGameUpdate()
        {
            Initialize();
        }

        public MenuLayout GetMenu()
        {
            return m_MenuGameUpdate;
        }

        public void OnEnter()
        {
        }

        public void OnExit()
        {
        }

        public void Process(MenuStack stack)
        {
            MenuGameUpdate(stack);
        }

        public void Initialize()
        {
            m_MenuGameUpdate = new MenuLayout(this, 450, 20);
        }

        public void MenuGameUpdate(MenuStack menuStack)
        {
            m_MenuGameUpdate.Update();

            bool enabled = true;

            if (m_MenuGameUpdate.AddItem("Update Check", "Checks for updates for programs", enabled))
            {
                GameUpdate.GameUpdateRequest request = new GameUpdate.GameUpdateRequest()
                {
                };

                var requestOp = new AsyncRequest<GameUpdate.GameUpdateRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("GameUpdate found = " + antecedent.Request.Found);
                        OnScreenLog.Add("Additional content found = " + antecedent.Request.AddcontFound);
                        OnScreenLog.Add("update version = " + antecedent.Request.ContentVersion);
                    }
                });

                GameUpdate.Schedule(requestOp);
            }

            if (m_MenuGameUpdate.AddItem("Additional Content Check", "Obtains the latest version information for the specified additional content", enabled))
            {
                GameUpdate.AddcontLatestVersionRequest request = new GameUpdate.AddcontLatestVersionRequest()
                {
                    ServiceLabel = 0,
                    EntitlementLabel = "entlab-01"
                };

                var requestOp = new AsyncRequest<GameUpdate.AddcontLatestVersionRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Additional Content Update found = " + antecedent.Request.Found);
                        OnScreenLog.Add("update version = " + antecedent.Request.ContentVersion);
                    }
                });

                GameUpdate.Schedule(requestOp);
            }

            if (m_MenuGameUpdate.AddBackIndex("Back"))
            {
                menuStack.PopMenu();
            }
        }

    }
#endif
}
