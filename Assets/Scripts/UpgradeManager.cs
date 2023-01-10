using System;
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

        private void Update()
        {
            if(pointsLbl.text != player.UserPoints.ToString())
                pointsLbl.text = player.UserPoints.ToString();

            if(powerLbl.text != player.Power.ToString())
                powerLbl.text = player.Power.ToString();

            if(dexterityLbl.text != player.Dexterity.ToString())
                dexterityLbl.text = player.Dexterity.ToString();

            if(enduranceLbl.text != player.Endurance.ToString())
                enduranceLbl.text = player.Endurance.ToString();

            if(vitalityLbl.text != player.Vitality.ToString())
                vitalityLbl.text = player.Vitality.ToString();

            if(defenseLbl.text != player.Defense.ToString())
                defenseLbl.text = player.Defense.ToString();

            if(luckLbl.text != player.Luck.ToString())
                luckLbl.text = player.Luck.ToString();

            if(healthPointsLbl.text != player.HealthPoints.ToString())
                healthPointsLbl.text = player.HealthPoints.ToString();

            if(attackPowerLbl.text != player.AttackPower.ToString())
                attackPowerLbl.text = player.AttackPower.ToString();

            if(defensePowerLbl.text != player.DefensePower.ToString())
                defensePowerLbl.text = player.DefensePower.ToString();

            if(attackSpeedLbl.text != player.AttackSpeed.ToString())
                attackSpeedLbl.text = player.AttackSpeed.ToString();

            if(movementSpeedLbl.text != player.MovementSpeed.ToString())
                movementSpeedLbl.text = player.MovementSpeed.ToString();

            if(staminaLbl.text != player.Stamina.ToString())
                staminaLbl.text = player.Stamina.ToString();

            if(critChance != player.CritChance / 10)
                critChance = (float)(Convert.ToDouble(player.CritChance) / 10);

            if(critChanceLbl.text != $"{critChance}%")
                critChanceLbl.text = $"{critChance}%";
        }

        private void OnDisable()
        {
            ResetStats();
        }

        private void OnDestroy()
        {
            ResetStats();
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
            player.SetUserPoints(currentPoints);
            pointsNeededLbl.text = pointsNeeded.ToString();

            player.SetAttribute(0, currentPower);
            player.SetAttribute(1, currentPower);
            player.SetAttribute(2, currentPower);
            player.SetAttribute(3, currentPower);
            player.SetAttribute(4, currentPower);
            player.SetAttribute(5, currentPower);
            player.SetHealthPoints(currentHP);
            player.SetAttackPower(currentAP);
            player.SetDefensePower(currentDP);
            player.SetAttackSpeed(currentAS);
            player.SetMovementSpeed(currentMS);
            player.SetStamina(currentStamina);
            player.SetCritChance(currentCrit);
        }


    }
}
