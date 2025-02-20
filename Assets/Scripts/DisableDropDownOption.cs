using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.MorganHouston.Imprecision
{
    public class DisableDropDownOption : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
/*#if UNITY_WSA
            Toggle toggle = GetComponent<Toggle>();
            if (toggle != null && toggle.name == "Item 3: ULTRA")
            {
                toggle.interactable = false;
            }
#endif*/
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
