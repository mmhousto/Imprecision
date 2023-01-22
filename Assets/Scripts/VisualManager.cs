using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MorganHouston.Imprecision
{
    public class VisualManager : MonoBehaviour
    {

        public Light sun;

        // Start is called before the first frame update
        void Start()
        {
            SetShadowsOnOff();
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public void SetShadowsOnOff()
        {
            if (PreferenceManager.Instance.Shadows == true)
            {
                sun.shadows = LightShadows.Soft;
            }
            else
            {
                sun.shadows = LightShadows.None;
            }
        }
    }
}
