using System.Collections;
using System.Collections.Generic;
using CloudSaveSample;
using UnityEngine;
using Unity.Services.CloudSave;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using System;

public class Player : MonoBehaviour
{
    public string userID;
    public string userName;
    public string userEmail;

    public int userPoints;
    public int userLevel;
    public int userXP;

    /*public string UserID { get { return userID; } set { userID = value; } }

    public int Points { get { return userPoints; } set { userPoints = value; } }

    public string PlayerName { get { return userName; } set { userName = value; } }

    public string PlayerEmail { get { return userEmail; } set { userEmail = value; } }

    public int PlayerLevel { get { return userLevel; } set { userLevel = value; } }

    public int PlayerExperience { get { return userXP; } set { userXP = value; } }*/

    /*public void SetData(SavePlayerData savePlayer)
    {
        UserID = savePlayer.userID;
        Points = savePlayer.userPoints;
        PlayerName = savePlayer.userName;
        PlayerEmail = savePlayer.userEmail;
        PlayerLevel = savePlayer.userLevel;
        PlayerExperience = savePlayer.userXP;
    }

    public void SetData(string id)
    {
        UserID = id;
        Points = 0;
        PlayerName = "Guest_" + id;
        PlayerEmail = null;
        PlayerLevel = 1;
        PlayerExperience = 0;
    }

    public void SetData(string id, string name, string email)
    {
        UserID = id;
        Points = 0;
        PlayerName = name;
        PlayerEmail = email;
        PlayerLevel = 1;
        PlayerExperience = 0;
    }*/

    private async void OnApplicationQuit()
    {
        SavePlayerData data = new SavePlayerData(this);
        await ForceSaveObjectData(userID, data);
    }

    private async Task ForceSaveObjectData(string key, SavePlayerData value)
    {
        try
        {
            // Although we are only saving a single value here, you can save multiple keys
            // and values in a single batch.
            Dictionary<string, object> oneElement = new Dictionary<string, object>
                {
                    { key, value }
                };

            await SaveData.ForceSaveAsync(oneElement);

            Debug.Log($"Successfully saved {key}:{value}");
        }
        catch (CloudSaveValidationException e)
        {
            Debug.LogError(e);
        }
        catch (CloudSaveException e)
        {
            Debug.LogError(e);
        }
    }
}
