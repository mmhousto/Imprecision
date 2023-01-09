using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Com.MorganHouston.Imprecision
{
    public class UpgradeManager : MonoBehaviour
    {
        public TextMeshProUGUI pointsLbl;
        public TextMeshProUGUI pointsNeededLbl;
        public TextMeshProUGUI powerLbl;
        public TextMeshProUGUI dexterityLbl;
        public TextMeshProUGUI enduranceLbl;
        public TextMeshProUGUI vitalityLbl;
        public TextMeshProUGUI defenseLbl;
        public TextMeshProUGUI luckLbl;

        public TextMeshProUGUI healthPointsLbl;
        public TextMeshProUGUI attackPowerLbl;
        public TextMeshProUGUI defensePowerLbl;
        public TextMeshProUGUI attackSpeedLbl;
        public TextMeshProUGUI movementSpeedLbl;
        public TextMeshProUGUI staminaLbl;
        public TextMeshProUGUI critChanceLbl;

        public float critChance;

        private int currentPoints, currentPower, currentDex, currentEnd, currentVit, 
            currentDef, currentLuck, currentHP, currentAP, currentDP, 
            currentAS, currentMS, currentStamina, currentCrit;
        private int pointsNeeded;
        private Player player;

        // Start is called before the first frame update
        void Start()
        {
            player = Player.Instance;
            currentPoints = player.UserPoints;
            currentPower = player.Power;
            currentDex = player.Dexterity;
            currentEnd = player.Endurance;
            currentVit = player.Vitality;
            currentDef = player.Defense;
            currentLuck = player.Luck;
            currentHP = player.HealthPoints;
            currentAP = player.AttackPower;
            currentDP = player.DefensePower;
            currentAS = player.AttackSpeed;
            currentMS = player.MovementSpeed;
            currentStamina = player.Stamina;
            currentCrit = player.CritChance;

            pointsNeeded = 0;
            pointsLbl.text = currentPoints.ToString();
            pointsNeededLbl.text = pointsNeeded.ToString();
            powerLbl.text = currentPower.ToString();
            dexterityLbl.text = currentDex.ToString();
            enduranceLbl.text = currentEnd.ToString();
            vitalityLbl.text = currentVit.ToString();
            defenseLbl.text = currentDef.ToString();
            luckLbl.text = currentLuck.ToString();
            healthPointsLbl.text = currentHP.ToString();
            attackPowerLbl.text = currentAP.ToString();
            defensePowerLbl.text = currentDP.ToString();
            attackSpeedLbl.text = currentAS.ToString();
            movementSpeedLbl.text = currentMS.ToString();
            staminaLbl.text = currentStamina.ToString();
            critChance = currentCrit / 10;
            critChanceLbl.text = $"{critChance}%";
        }

        public void IncreaseAttribute(int attributeToIncrease)
        {
            Player.Instance.IncreaseAttribute(attributeToIncrease);

            switch (attributeToIncrease)
            {
                // POWER
                case 0:
                    pointsNeeded += (Player.Instance.Power-1 * 420) * 10;
                    break;
                    
                // DEXTERITY
                case 1:
                    pointsNeeded += (Player.Instance.Dexterity-1 * 420) * 10;
                    break;

                // ENDURANCE
                case 2:
                    pointsNeeded += (Player.Instance.Endurance-1 * 420) * 10;
                    break;


                // VITALITY
                case 3:
                    pointsNeeded += (Player.Instance.Vitality-1 * 420) * 10;
                    break;

                // DEFENSE
                case 4:
                    pointsNeeded += (Player.Instance.Defense-1 * 420) * 10;
                    break;

                // LUCK
                case 5:
                    pointsNeeded += (Player.Instance.Luck-1 * 420) * 10;
                    break;
            }
        }

        public void ResetStats()
        {
            pointsNeeded = 0;
            player.UserPoints;
            pointsNeededLbl.text = pointsNeeded.ToString();

            player.Power.ToString();
            dexterityLbl.text = player.Dexterity.ToString();
            enduranceLbl.text = player.Endurance.ToString();
            vitalityLbl.text = player.Vitality.ToString();
            defenseLbl.text = player.Defense.ToString();
            luckLbl.text = player.Luck.ToString();
            healthPointsLbl.text = player.HealthPoints.ToString();
            attackPowerLbl.text = player.AttackPower.ToString();
            defensePowerLbl.text = player.DefensePower.ToString();
            attackSpeedLbl.text = player.AttackSpeed.ToString();
            movementSpeedLbl.text = player.MovementSpeed.ToString();
            staminaLbl.text = player.Stamina.ToString();
            critChance = player.CritChance / 10;
            critChanceLbl.text = $"{critChance}%";
        }


    }
}
