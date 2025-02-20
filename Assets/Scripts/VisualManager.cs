using System;
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
            SetQuality(PreferenceManager.Instance.QualityLevel);
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public void SetQuality(int qualityLevel)
        {
            QualitySettings.SetQualityLevel(qualityLevel);
            PreferenceManager.Instance.SetQuality(qualityLevel);
        }

        public void SetShadowsOnOff()
        {
            if (Convert.ToBoolean(PlayerPrefs.GetInt("Shadows", 1)) == true)
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
