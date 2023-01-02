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

        private int pointsNeeded;
        private Player player;

        // Start is called before the first frame update
        void Start()
        {
            player = Player.Instance;
            pointsNeeded = (player.UserLevel * 420) * 10;
            pointsLbl.text = player.UserPoints.ToString();
            pointsNeededLbl.text = pointsNeeded.ToString();
            powerLbl.text = player.Power.ToString();
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

        public void IncreaseAttribute(int attributeToIncrease)
        {
            switch (attributeToIncrease)
            {
                // POWER
                case 0:

                    break;
                    
                // DEXTERITY
                case 1:

                    break;

                // ENDURANCE
                case 2:
                    break;


                // VITALITY
                case 3:

                    break;

                // DEFENSE
                case 4:

                    break;

                // LUCK
                case 5:

                    break;
            }
        }


    }
}
