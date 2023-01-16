using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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

        public TextMeshProUGUI upgradeLbl;

        public Button minusPow, minusDex, minusEnd, minusVit, minusDef, minusLuck, upgrade;

        public float critChance;

        private int currentPoints, currentPower, currentDex, currentEnd, currentVit, 
            currentDef, currentLuck, currentHP, currentAP, currentDP, 
            currentAS, currentMS, currentStamina, currentCrit;
        private int pointsNeeded;
        private Player player;
        private List<Button> buttons = new List<Button>();

        /// <summary>
        /// Gets current stats and sets lables
        /// </summary>
        void Start()
        {
            player = Player.Instance;
            SetCurrentStats();

            buttons.Add(minusPow);
            buttons.Add(minusDex);
            buttons.Add(minusEnd);
            buttons.Add(minusVit);
            buttons.Add(minusDef);
            buttons.Add(minusLuck);
        }

        /// <summary>
        /// Updates Text Lables
        /// </summary>
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

            if(pointsNeeded > 0)
                upgrade.interactable = true;
            else
                upgrade.interactable = false;
        }

        /// <summary>
        /// Sets Stats back to current stats on disable
        /// </summary>
        private void OnDisable()
        {
            ResetStats();
        }

        /// <summary>
        /// Sets Stats back to current stats on destroy
        /// </summary>
        private void OnDestroy()
        {
            ResetStats();
        }

        /// <summary>
        /// Sets Stats back to current stats on pause
        /// </summary>
        private void OnApplicationPause(bool pause)
        {
            ResetStats();
        }

        /// <summary>
        /// Sets Stats back to current stats on quit
        /// </summary>
        private void OnApplicationQuit()
        {
            ResetStats();
        }

        private void SetCurrentStats()
        {
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

        /// <summary>
        /// Increases attributes
        /// </summary>
        /// <param name="attributeToIncrease">The attribute to increase</param>
        public void IncreaseAttribute(int attributeToIncrease)
        {
            int tmp = pointsNeeded; 

            switch (attributeToIncrease)
            {
                // POWER
                case 0:
                    tmp += (player.Power) * 420 * 10;
                    
                    break;

                // DEXTERITY
                case 1:
                    tmp += (player.Dexterity) * 420 * 10;
                    break;

                // ENDURANCE
                case 2:
                    tmp += (player.Endurance) * 420 * 10;
                    break;

                // VITALITY
                case 3:
                    tmp += (player.Vitality) * 420 * 10;
                    break;

                // DEFENSE
                case 4:
                    tmp += (player.Defense) * 420 * 10;
                    break;

                // LUCK
                case 5:
                    tmp += (player.Luck) * 420 * 10;
                    break;
            }

            if (tmp > player.UserPoints) return;

            player.IncreaseAttribute(attributeToIncrease);

            switch (attributeToIncrease)
            {
                // POWER
                case 0:
                    pointsNeeded += ((player.Power-1) * 420) * 10;
                    if (!minusPow.IsInteractable())
                        minusPow.interactable = true;
                    break;
                    
                // DEXTERITY
                case 1:
                    pointsNeeded += ((player.Dexterity-1) * 420) * 10;
                    if (!minusDex.IsInteractable())
                        minusDex.interactable = true;
                    break;

                // ENDURANCE
                case 2:
                    pointsNeeded += ((player.Endurance-1) * 420) * 10;
                    if (!minusEnd.IsInteractable())
                        minusEnd.interactable = true;
                    break;


                // VITALITY
                case 3:
                    pointsNeeded += ((player.Vitality-1) * 420) * 10;
                    if (!minusVit.IsInteractable())
                        minusVit.interactable = true;
                    break;

                // DEFENSE
                case 4:
                    pointsNeeded += ((player.Defense-1) * 420) * 10;
                    if (!minusDef.IsInteractable())
                        minusDef.interactable = true;
                    break;

                // LUCK
                case 5:
                    pointsNeeded += ((player.Luck - 1) * 420) * 10;
                    if (!minusLuck.IsInteractable())
                        minusLuck.interactable = true;
                    break;
            }
            pointsNeededLbl.text = pointsNeeded.ToString();
        }

        /// <summary>
        /// Decreases attributes
        /// </summary>
        /// <param name="attributeToIncrease">The attribute to decrease</param>
        public void DecreaseAttribute(int attributeToIncrease)
        {
            switch (attributeToIncrease)
            {
                // POWER
                case 0:
                    if (player.Power <= currentPower)
                        return;
                    break;

                // DEXTERITY
                case 1:
                    if (player.Dexterity <= currentDex)
                        return;
                    break;

                // ENDURANCE
                case 2:
                    if (player.Endurance <= currentEnd)
                        return;
                    break;

                // VITALITY
                case 3:
                    if (player.Vitality <= currentVit)
                        return;
                    break;

                // DEFENSE
                case 4:
                    if (player.Defense <= currentDef)
                        return;
                    break;

                // LUCK
                case 5:
                    if (player.Luck <= currentLuck)
                        return;
                    break;
            }

            player.DecreaseAttribute(attributeToIncrease);

            switch (attributeToIncrease)
            {
                // POWER
                case 0:
                    pointsNeeded -= ((player.Power) * 420) * 10;
                    break;

                // DEXTERITY
                case 1:
                    pointsNeeded -= ((player.Dexterity) * 420) * 10;
                    break;

                // ENDURANCE
                case 2:
                    pointsNeeded -= ((player.Endurance) * 420) * 10;
                    break;


                // VITALITY
                case 3:
                    pointsNeeded -= ((player.Vitality) * 420) * 10;
                    break;

                // DEFENSE
                case 4:
                    pointsNeeded -= ((player.Defense) * 420) * 10;
                    break;

                // LUCK
                case 5:
                    pointsNeeded -= ((player.Luck) * 420) * 10;
                    break;
            }
            pointsNeededLbl.text = pointsNeeded.ToString();
        }

        public void SetUpgradeLabel()
        {
            upgradeLbl.text = $"Are you sure you want to spend {pointsNeeded} points?";
        }

        public void Upgrade()
        {
            player.SetUserPoints(player.UserPoints - pointsNeeded);

            foreach(Button button in buttons)
            {
                button.interactable = false;
            }

            SetCurrentStats();

            CloudSaveLogin.Instance.SaveCloudData();
        }

        /// <summary>
        /// Sets stats back to current stats
        /// </summary>
        public void ResetStats()
        {
            pointsNeeded = 0;
            player.SetUserPoints(currentPoints);
            pointsNeededLbl.text = pointsNeeded.ToString();

            player.SetAttribute(0, currentPower);
            player.SetAttribute(1, currentDex);
            player.SetAttribute(2, currentEnd);
            player.SetAttribute(3, currentVit);
            player.SetAttribute(4, currentDef);
            player.SetAttribute(5, currentLuck);
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
