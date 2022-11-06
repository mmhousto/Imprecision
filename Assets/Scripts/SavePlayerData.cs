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
        public float accuracy;
        public int bullseyesHit;

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
            accuracy = player.Accuracy;
            bullseyesHit = player.BullseyesHit;

        }

    }

}
