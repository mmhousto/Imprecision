using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Com.MorganHouston.Imprecision
{
    public class SkipTextManager : MonoBehaviour
    {

        private TextMeshProUGUI skipLabel;

        // Start is called before the first frame update
        void Start()
        {
            skipLabel = GetComponent<TextMeshProUGUI>();
            SetLabel();
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        private void SetLabel()
        {
#if UNITY_STANDALONE
            skipLabel.text = "Press 'Enter' to Skip";
#elif UNITY_WSA
            skipLabel.text = "Press 'B' to Skip";
#endif
        }
    }
}
