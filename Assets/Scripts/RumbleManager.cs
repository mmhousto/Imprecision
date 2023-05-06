using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Com.MorganHouston.Imprecision
{
    public class RumbleManager : MonoBehaviour
    {
        public static RumbleManager instance;
        private Gamepad pad;
        private Coroutine stopRumble;
        private bool rumbleStarted = false;

        private void Awake()
        {
            if (instance == null)
                instance = this;
        }
        
        public void RumblePulse(float lofreq, float hifreq, float duration)
        {
            pad = Gamepad.current;

            if(pad != null)
            {
                pad.SetMotorSpeeds(lofreq, hifreq);
                
            }

            if(rumbleStarted == false)
            {
                StartCoroutine(StopRumble(duration));
                rumbleStarted = true;
            }
            
        }

        public void StopRumbleNow()
        {
            if (pad != null)
                pad.SetMotorSpeeds(0f, 0f);
            rumbleStarted = false;
        }
        
        IEnumerator StopRumble(float duration)
        {
            float elapsedTime = 0f;
            while(elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                yield return null;

            }

            if(pad != null)
                pad.SetMotorSpeeds(0f, 0f);
            rumbleStarted = false;
        }
    }
}
