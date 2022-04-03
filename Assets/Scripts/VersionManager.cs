using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace com.MorganHouston.Imprecision
{
    public class VersionManager : MonoBehaviour
    {

        public TextMeshProUGUI versionLabel;

        // Start is called before the first frame update
        void Start()
        {
            versionLabel.text = $"VERSION: {Application.version}";
        }
    }
}
