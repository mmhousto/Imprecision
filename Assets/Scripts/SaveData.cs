using System;

[Serializable]
public class SavePlayerData 
{
    public string userID;
    public string userName;
    public string userEmail;

    public int userPoints;
    public int userLevel;
    public int userXP;

    public SavePlayerData(Player player)
    {
        userID = player.userID;
        userName = player.userName;
        userEmail = player.userEmail;
        userPoints = player.userPoints;
        userLevel = player.userLevel;
        userXP = player.userXP;
    }

}
