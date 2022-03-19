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

        public SavePlayerData(Player player)
        {
            userID = player.userID;
            userName = player.userName;
            userPoints = player.userPoints;
            userLevel = player.userLevel;
            userXP = player.userXP;
        }

    }

}
