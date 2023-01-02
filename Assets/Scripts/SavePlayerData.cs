using System;

namespace Com.MorganHouston.Imprecision
{

    [Serializable]
    public class SavePlayerData
    {
        public string userID;
        public string userName;

        public int userPoints;
        public int userLevel;
        public int userXP;
        public int jewels;

        public int[] levels = new int[50];
        public int[] appleShotOnLevels = new int[50];
        public int[] bullseyesOnLevels = new int[50];
        public int arrowsFired;
        public int targetsHit;
        public int bullseyesHit;

        public int power;
        public int dexterity;
        public int endurance;
        public int vitality;
        public int defense;
        public int luck;

        public int healthPoints;
        public int attackPower;
        public int defensePower;
        public int attackSpeed;
        public int movementSpeed;
        public int stamina;
        public int critChance;

        public SavePlayerData(Player player)
        {
            userID = player.UserID;
            userName = player.UserName;
            userPoints = player.UserPoints;
            userLevel = player.UserLevel;
            userXP = player.UserXP;
            jewels = player.Jewels;
            levels = player.Levels;
            appleShotOnLevels = player.AppleShotOnLevels;
            arrowsFired = player.ArrowsFired;
            targetsHit = player.TargetsHit;
            bullseyesHit = player.BullseyesHit;

            power = player.Power;
            dexterity = player.Dexterity;
            endurance = player.Endurance;
            vitality = player.Vitality;
            defense = player.Defense;
            luck = player.Luck;

            healthPoints = player.HealthPoints;
            attackPower = player.AttackPower;
            defensePower = player.DefensePower;
            attackSpeed = player.AttackSpeed;
            movementSpeed = player.MovementSpeed;
            stamina = player.Stamina;
            critChance = player.CritChance;

        }

    }

}
