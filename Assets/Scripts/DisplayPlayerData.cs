using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Com.MorganHouston.Imprecision
{

    public class DisplayPlayerData : MonoBehaviour
    {
        public TMP_InputField userNameField;
        public TextMeshProUGUI pointsTxt, levelTxt, jewelsTxt;
        public Slider xpSlider;

        private Player player;

        private int xp, points, level, jewels;
        private string userName;

        // Start is called before the first frame update
        void Start()
        {
            player = Player.Instance;
            xpSlider.minValue = 0;
            xpSlider.maxValue = 150 * level;
        }

        // Update is called once per frame
        void Update()
        {
            if (xp != player.UserXP)
                xp = player.UserXP;

            if (points != player.UserPoints)
                points = player.UserPoints;

            if (level != player.UserLevel)
            {
                level = player.UserLevel;
                xpSlider.maxValue = 150 * level;
            }

            if (userName != player.UserName)
                userName = player.UserName;

            if(jewels != player.Jewels)
                jewels = player.Jewels;

            userNameField.text = userName;
            pointsTxt.text = $"Points: {points}";
            levelTxt.text = $"Level: {level}";
            jewelsTxt.text = $"{jewels}";
            xpSlider.value = xp;
        }
    }
}
