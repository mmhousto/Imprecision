
#if UNITY_PS5 || UNITY_PS4
using Unity.PSN.PS5.Auth;
using Unity.PSN.PS5.Aysnc;
using Unity.PSN.PS5.Checks;
#endif


namespace PSNSample
{
#if UNITY_PS5 || UNITY_PS4
    public class SonyAuth : IScreen
    {
        MenuLayout m_MenuAuth;

        public SonyAuth()
        {
            Initialize();
        }

        public MenuLayout GetMenu()
        {
            return m_MenuAuth;
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
            m_MenuAuth = new MenuLayout(this, 450, 20);
        }

        public void MenuOnlineSafety(MenuStack menuStack)
        {
            m_MenuAuth.Update();

            bool enabled = true;

            if (m_MenuAuth.AddItem("Get Auth Code", "Get an authentication code", enabled))
            {
                Authentication.GetAuthorizationCodeRequest request = new Authentication.GetAuthorizationCodeRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
#if UNITY_PS5
                    ClientId = "686986a6-3b34-4a42-89d1-b4ba193bc80f",
#elif UNITY_PS4
                    ClientId = "c5806b90-16f4-4086-9b43-665b69654b05",
#endif
                    Scope = "psn:s2s"
                };

                var requestOp = new AsyncRequest<Authentication.GetAuthorizationCodeRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("GetAuthorizationCodeRequest:");
                        OnScreenLog.Add("  ClientId = " + antecedent.Request.ClientId);
                        OnScreenLog.Add("  Scope = " + antecedent.Request.Scope);
                        OnScreenLog.Add("  AuthCode = " + antecedent.Request.AuthCode);
                        OnScreenLog.Add("  IssuerId = " + antecedent.Request.IssuerId);
                    }
                });

                Authentication.Schedule(requestOp);
            }

            if (m_MenuAuth.AddItem("Get Id Token", "Get Id Token", enabled))
            {
                Authentication.GetIdTokenRequest request = new Authentication.GetIdTokenRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
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
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("GetIdTokenRequest:");
                        OnScreenLog.Add("  ClientId = " + antecedent.Request.ClientId);
                        OnScreenLog.Add("  ClientSecret = " + antecedent.Request.ClientSecret);
                        OnScreenLog.Add("  Scope = " + antecedent.Request.Scope);
                        OnScreenLog.Add("  IdToken = " + antecedent.Request.IdToken);
                    }
                });

                Authentication.Schedule(requestOp);
            }

            if (m_MenuAuth.AddBackIndex("Back"))
            {
                menuStack.PopMenu();
            }
        }

    }
#endif
                }
