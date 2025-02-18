using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Com.MorganHouston.Imprecision
{
    public class ShowDropDownValue : MonoBehaviour
    {
        private int dropDownValue;
        private TMP_Dropdown dropDown;
        public int defaultValue;
        public string prefName;

        // Start is called before the first frame update
        void Start()
        {
            dropDownValue = PlayerPrefs.GetInt("Quality", 2);
            dropDown = GetComponent<TMP_Dropdown>();
            dropDown.value = dropDownValue;


        }
    }
}
