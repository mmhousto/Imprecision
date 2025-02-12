using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Com.MorganHouston.Imprecision
{
    public class SubtitleManager : MonoBehaviour
    {
        public string[] subtitles;
        private TextMeshProUGUI subtitleText;
        private int subtitleIndex;

        // Start is called before the first frame update
        void Start()
        {
            subtitleText = GetComponent<TextMeshProUGUI>();
            subtitleIndex = 0;
        }

        public void ChangeText()
        {
            subtitleIndex++;
            subtitleText.text = subtitles[subtitleIndex];
        }

        public void SetSubtitleIndex(int i)
        {
            subtitleIndex = i;
            subtitleText.text = subtitles[subtitleIndex];
        }
    }
}
