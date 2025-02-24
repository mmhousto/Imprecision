using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MorganHouston.Imprecision
{
    public class PSNotifyPremium : MonoBehaviour
    {
#if UNITY_PS5 && !UNITY_EDITOR

        bool calledSpectate = false;
        bool calledCP = false;

        // Start is called before the first frame update
        void Start()
        {
            //StartCPNotify();
        }

        private void Update()
        {
            /*if(NetworkSpectatorManager.isAlive == false && calledSpectate == false)
            {
                StopInvoke();
                StartSpectateNotify();
            }
            else if (NetworkSpectatorManager.isAlive == true && calledCP == false)
            {
                StopInvoke();
                StartCPNotify();
            }*/
        }

        void StopInvoke()
        {
            CancelInvoke();
        }

        void StartCPNotify()
        {
            calledSpectate = false;
            calledCP = true;
            InvokeRepeating(nameof(NotifyPremium), 1f, 1f);
        }

        void StartSpectateNotify()
        {
            calledSpectate = true;          
            calledCP = false;
            InvokeRepeating(nameof(NotifyPremiumSpectate), 1f, 1f);
        }

        void NotifyPremiumSpectate()
        {
            PSFeatureGating.NotifyPremiumSpectate();
        }

        void NotifyPremium()
        {
            PSFeatureGating.NotifyPremium();
        }
#endif
    }
}
