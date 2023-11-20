using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.MorganHouston.Imprecision
{
    public class ShowToggleValue : MonoBehaviour
    {

        private bool toggleValue;
        private Toggle toggle;
        public int defaultValue;
        public string toggleName;

        // Start is called before the first frame update
        void Start()
        {
            toggleValue = Convert.ToBoolean(PlayerPrefs.GetInt(toggleName, defaultValue));
            toggle = GetComponent<Toggle>();
            toggle.isOn = toggleValue;

            if (toggleName == "TutorialPlayed" && toggleValue == true)
                toggle.interactable = true;
            else if (toggleName == "TutorialPlayed" && toggleValue == false)
                toggle.interactable = false;
        }

        // Update is called once per frame
        void Update()
        {
            if(toggleName == "TutorialPlayed" && toggle.isOn == false)
            {
                toggle.interactable = false;
            }
        }
    }
}
