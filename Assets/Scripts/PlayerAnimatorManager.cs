using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MorganHouston.Imprecision
{
    public class PlayerAnimatorManager : MonoBehaviour
    {
        private string verticalKey = "vert";
        private string horizontalKey = "horz";
        private string isRunningKey = "isRunning";
        private string drawKey = "Draw";
        private string inAirKey = "inAir";
        private string shotStrengthKey = "ShotStrength";
        private string shotKey = "Shot";
        private string speedKey = "speed";

        private Animator anim;

        private void Start()
        {
            anim = GetComponent<Animator>();
        }

        public void SetInAir(bool newState)
        {
            anim.SetBool(inAirKey, newState);
        }

        public void SetShot()
        {
            anim.SetTrigger(shotKey);
        }

        public void SetSpeed(float newSpeed)
        {
            anim.SetFloat(speedKey, newSpeed);
        }

        public void SetVert(float newVert)
        {
            anim.SetFloat(verticalKey, newVert);
        }

        public void SetHorz(float newHorz)
        {
            anim.SetFloat(horizontalKey, newHorz);
        }

        public void SetShotStrength(float newPower)
        {
            anim.SetFloat(shotStrengthKey, newPower);
        }

        public void SetDraw(bool newDrawState)
        {
            anim.SetBool(drawKey, newDrawState);
        }


    }
}
