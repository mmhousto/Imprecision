
#if UNITY_PS5 || UNITY_PS4
using Unity.PSN.PS5.Bandwidth;
using Unity.PSN.PS5.Aysnc;
#endif


namespace PSNSample
{
#if UNITY_PS5 || UNITY_PS4
    public class SonyBandwidth : IScreen
    {
        MenuLayout m_MenuBandwidth;

        public SonyBandwidth()
        {
            Initialize();
        }

        public MenuLayout GetMenu()
        {
            return m_MenuBandwidth;
        }

        public void OnEnter()
        {
        }

        public void OnExit()
        {
        }

        public void Process(MenuStack stack)
        {
            MenuBandwidth(stack);
        }

        public void Initialize()
        {
            m_MenuBandwidth = new MenuLayout(this, 450, 20);
        }

        public void MenuBandwidth(MenuStack menuStack)
        {
            m_MenuBandwidth.Update();

            bool enabled = true;

            if (m_MenuBandwidth.AddItem("Test Upload Bandwidth", "Test bandwidth to PSN servers", enabled))
            {
                BandwidthTest.MeasureBandwidthRequest request = new BandwidthTest.MeasureBandwidthRequest()
                {
                    Mode = BandwidthTest.Modes.CalcUploadBps,
                    TimeoutMs = 2000
                };

                var requestOp = new AsyncRequest<BandwidthTest.MeasureBandwidthRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Upload BPS = " + antecedent.Request.Bps + "(bps) : " + (antecedent.Request.Bps / (8*1024)) + "(kbps)");
                    }
                });

                BandwidthTest.Schedule(requestOp);
            }

            if (m_MenuBandwidth.AddItem("Test Download Bandwidth", "Test bandwidth to PSN servers", enabled))
            {
                BandwidthTest.MeasureBandwidthRequest request = new BandwidthTest.MeasureBandwidthRequest()
                {
                    Mode = BandwidthTest.Modes.CalcDownloadBps,
                    TimeoutMs = 2000
                };

                var requestOp = new AsyncRequest<BandwidthTest.MeasureBandwidthRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Download BPS = " + antecedent.Request.Bps + "(bps) : " + (antecedent.Request.Bps / (8 * 1024)) + "(kbps)");
                    }
                });

                BandwidthTest.Schedule(requestOp);
            }

            if (m_MenuBandwidth.AddBackIndex("Back"))
            {
                menuStack.PopMenu();
            }
        }

    }
#endif
}
