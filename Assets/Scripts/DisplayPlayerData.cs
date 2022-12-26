using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Com.MorganHouston.Imprecision
{

    public class DisplayPlayerData : MonoBehaviour
    {
        public TextMeshProUGUI userNameTxt, pointsTxt, levelTxt, jewelsTxt;
        public Slider xpSlider;

        private Player player;

        private int xp, points, level, jewels;
        private string userName;
        private float maxXP;

        // Start is called before the first frame update
        void Start()
        {
            player = Player.Instance;
            xpSlider.minValue = 0;
            maxXP = 420 * level;
            xpSlider.maxValue = maxXP;
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
                maxXP = 420 * level;
            }

            if(xpSlider.maxValue != maxXP)
                xpSlider.maxValue = maxXP;

            if (userName != player.UserName)
                userName = player.UserName;

            if(jewels != player.Jewels)
                jewels = player.Jewels;

            if(userNameTxt.text != userName)
                userNameTxt.text = userName;

            if(pointsTxt.text != $"Points: {points}")
                pointsTxt.text = $"Points: {points}";

            if(levelTxt.text != $"Level: {level}")
                levelTxt.text = $"Level: {level}";

            if(jewelsTxt.text != $"{jewels}")
                jewelsTxt.text = $"{jewels}";

            if(xpSlider.value != xp)
                xpSlider.value = xp;
        }
    }
}
