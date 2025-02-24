
#if UNITY_PS5 || UNITY_PS4
using Unity.PSN.PS5.Aysnc;
using Unity.PSN.PS5.Checks;
#endif


namespace PSNSample
{
#if UNITY_PS5 || UNITY_PS4
    public class SonyOnlineSafety : IScreen
    {
        MenuLayout m_MenuOnlineSafety;

        public SonyOnlineSafety()
        {
            Initialize();
        }

        public MenuLayout GetMenu()
        {
            return m_MenuOnlineSafety;
        }

        public void OnEnter()
        {
        }

        public void OnExit()
        {
        }

        public void Process(MenuStack stack)
        {
            MenuOnlineSafety(stack);
        }

        public void Initialize()
        {
            m_MenuOnlineSafety = new MenuLayout(this, 450, 20);
        }

        public void MenuOnlineSafety(MenuStack menuStack)
        {
            m_MenuOnlineSafety.Update();

            bool enabled = true;

            if (m_MenuOnlineSafety.AddItem("CR Status", "Get Communication Restriction Status for current user", enabled))
            {
                OnlineSafety.GetCommunicationRestrictionStatusRequest request = new OnlineSafety.GetCommunicationRestrictionStatusRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId
                };

                var requestOp = new AsyncRequest<OnlineSafety.GetCommunicationRestrictionStatusRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("CR Status = " + antecedent.Request.Status);
                    }
                });

                OnlineSafety.Schedule(requestOp);
            }

            if (m_MenuOnlineSafety.AddItem("Filter Profanity", "Replace profanity in text with *", enabled))
            {
                OnlineSafety.FilterProfanityRequest request = new OnlineSafety.FilterProfanityRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                    Locale = "en-US",
                    TextToFilter = "This is a test of a shit string"
                };

                OnScreenLog.Add("Filtering : " + request.TextToFilter);

                var requestOp = new AsyncRequest<OnlineSafety.FilterProfanityRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Filtered Text = " + antecedent.Request.FilteredText);
                    }
                });

                OnlineSafety.Schedule(requestOp);
            }

            if (m_MenuOnlineSafety.AddItem("Test Profanity", "Mark profanity in text with []", enabled))
            {
                OnlineSafety.FilterProfanityRequest request = new OnlineSafety.FilterProfanityRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                    Locale = "en-US",
                    TextToFilter = "This is a test of a shit string",
                    FilterType = OnlineSafety.ProfanityFilterType.MarkProfanity
                };

                OnScreenLog.Add("Filtering : " + request.TextToFilter);

                var requestOp = new AsyncRequest<OnlineSafety.FilterProfanityRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Filtered Text = " + antecedent.Request.FilteredText);
                    }
                });

                OnlineSafety.Schedule(requestOp);
            }

            if (m_MenuOnlineSafety.AddBackIndex("Back"))
            {
                menuStack.PopMenu();
            }
        }

    }
#endif
}
