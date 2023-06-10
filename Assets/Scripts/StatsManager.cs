using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Com.MorganHouston.Imprecision
{
    public class StatsManager : MonoBehaviour
    {

        public TextMeshProUGUI userID, userName, userPoints, userLevel,
            userXP, jewels, arrowsFired, targetsHit, accuracy,
            bullseyesHit, totalStarsLabel, threeStarLevels, perfectLevels, applesShot;
        private int totalStars, threeStars, perfects, applesShotInt, targetsHitValue, arrowsFiredValue;
        private Player player;

        // Start is called before the first frame update
        void Start()
        {
            UpdateStats();
        }

        // Update is called once per frame
        void Update()
        {
            UpdateStats();
        }

        private void UpdateStats()
        {
            player = Player.Instance;

            if(userID != null && userID.text != player.UserID)
                userID.text = player.UserID;

            if(userName.text != player.UserName)
                userName.text = player.UserName;

            if(userPoints.text != player.UserPoints.ToString())
                userPoints.text = player.UserPoints.ToString();
            
            if(userLevel.text != player.UserLevel.ToString())
                userLevel.text = player.UserLevel.ToString();

            if(userXP.text != player.UserXP.ToString())
                userXP.text = player.UserXP.ToString();

            if(jewels.text != player.Jewels.ToString())
                jewels.text = player.Jewels.ToString();

            arrowsFiredValue = player.ArrowsFired;
            if (arrowsFired.text != arrowsFiredValue.ToString())
                arrowsFired.text = arrowsFiredValue.ToString();

            targetsHitValue = player.TargetsHit;
            if(targetsHit.text != targetsHitValue.ToString())
                targetsHit.text = targetsHitValue.ToString();

            int accuracyValue = arrowsFiredValue == 0 ? 0 : targetsHitValue * 100 / arrowsFiredValue;
            if (accuracy.text != $"{accuracyValue}%")
                accuracy.text = $"{accuracyValue}%";

            if(bullseyesHit.text != player.BullseyesHit.ToString())
                bullseyesHit.text = player.BullseyesHit.ToString();

            totalStars = player.GetTotalStars();
            if (totalStarsLabel.text != totalStars.ToString())
                totalStarsLabel.text = totalStars.ToString();

            threeStars = player.GetThreeStars();
            if(threeStarLevels.text != threeStars.ToString())
                threeStarLevels.text = threeStars.ToString();

            perfects = player.GetPerfects();
            if(perfectLevels.text != perfects.ToString())
                perfectLevels.text = perfects.ToString();

            applesShotInt = player.GetApplesShotOnLevels();
            if(applesShot.text != applesShotInt.ToString())
                applesShot.text = applesShotInt.ToString();

        }


    }
}
