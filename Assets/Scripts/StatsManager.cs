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
            if(userID != null && userID.text != Player.Instance.UserID)
                userID.text = Player.Instance.UserID;

            if(userName.text != Player.Instance.UserName)
                userName.text = Player.Instance.UserName;

            if(userPoints.text != Player.Instance.UserPoints.ToString())
                userPoints.text = Player.Instance.UserPoints.ToString();
            
            if(userLevel.text != Player.Instance.UserLevel.ToString())
                userLevel.text = Player.Instance.UserLevel.ToString();

            if(userXP.text != Player.Instance.UserXP.ToString())
                userXP.text = Player.Instance.UserXP.ToString();

            if(jewels.text != Player.Instance.Jewels.ToString())
                jewels.text = Player.Instance.Jewels.ToString();

            arrowsFiredValue = Player.Instance.ArrowsFired;
            if (arrowsFired.text != arrowsFiredValue.ToString())
                arrowsFired.text = arrowsFiredValue.ToString();

            targetsHitValue = Player.Instance.TargetsHit;
            if(targetsHit.text != targetsHitValue.ToString())
                targetsHit.text = targetsHitValue.ToString();

            int accuracyValue = arrowsFiredValue == 0 ? 0 : targetsHitValue * 100 / arrowsFiredValue;
            if (accuracy.text != $"{accuracyValue}%")
                accuracy.text = $"{accuracyValue}%";

            if(bullseyesHit.text != Player.Instance.BullseyesHit.ToString())
                bullseyesHit.text = Player.Instance.BullseyesHit.ToString();

            totalStars = GetTotalStars();
            if (totalStarsLabel.text != totalStars.ToString())
                totalStarsLabel.text = totalStars.ToString();

            threeStars = GetThreeStars();
            if(threeStarLevels.text != threeStars.ToString())
                threeStarLevels.text = threeStars.ToString();

            perfects = GetPerfects();
            if(perfectLevels.text != perfects.ToString())
                perfectLevels.text = perfects.ToString();

            applesShotInt = GetApplesShotOnLevels();
            if(applesShot.text != applesShotInt.ToString())
                applesShot.text = applesShotInt.ToString();

        }

        private int GetApplesShotOnLevels()
        {
            int applesShotOnLevelsCounted = 0;
            foreach (int apple in Player.Instance.AppleShotOnLevels)
            {
               applesShotOnLevelsCounted += apple;
            }
            return applesShotOnLevelsCounted;
        }

        private int GetPerfects()
        {
            int perfectsCounted = 0;
            foreach (int perfect in Player.Instance.BullseyesOnLevels)
            {
                perfectsCounted += perfect;
            }
            return perfectsCounted;
        }

        private int GetTotalStars()
        {
            int starsCounted = 0;
            foreach (int stars in Player.Instance.Levels)
            {
                starsCounted += stars;
            }
            return starsCounted;
        }

        private int GetThreeStars()
        {
            int threeStarsCounted = 0;
            foreach (int stars in Player.Instance.Levels)
            {
                if (stars == 3)
                {
                    threeStarsCounted++;
                }
            }
            return threeStarsCounted;
        }



    }
}
