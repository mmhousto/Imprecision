using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if UNITY_PS5
class User
{
    public static int GetActiveUserId
    {
        get
        {
            if ( GamePad.activeGamePad == null )
            {
                OnScreenLog.AddError("User.GetActiveUserId : Active Gamepad is null. Must wait until the gamepad system has had time to initialize correctly.");
            }
            return GamePad.activeGamePad.loggedInUser.userId;
        }
    }

    public static User GetActiveUser
    {
        get
        {
            return FindUser(GamePad.activeGamePad.loggedInUser.userId);
        }
    }

    public static User[] users = new User[4];

    public bool trophyPackRegistered = false;

    public static User FindUser(int userId)
    {
        for (int i = 0; i < users.Length; i++)
        {
            if (users[i] != null && users[i].gamePad != null)
            {
                if (users[i].gamePad.loggedInUser.userId == userId)
                {
                    return users[i];
                }
            }
        }

        return null;
    }

    public static void Initialize(GamePad[] gamePads)
    {
        for(int i = 0; i < gamePads.Length; i++)
        {
            int playerId = gamePads[i].playerId;

            users[playerId] = new User();
            users[playerId].gamePad = gamePads[i];

           // OnScreenLog.Add("Player " + playerId + " has a controller");
        }
    }

    public static string Output()
    {
        string userOutput = "";
        for (int i = 0; i < users.Length; i++)
        {
            if (users[i] != null && users[i].gamePad != null)
            {
                if (users[i].gamePad.IsConnected == true)
                {
                    if (users[i].gamePad == GamePad.activeGamePad)
                    {
                        userOutput += "-->";
                    }
                    else
                    {
                        userOutput += "    ";
                    }

                    userOutput += " (0x" + users[i].gamePad.loggedInUser.userId.ToString("X8") + ")    " + users[i].gamePad.loggedInUser.userName + "\n";
                }
            }
        }
        return userOutput;
    }

    public GamePad gamePad;
}
#endif
