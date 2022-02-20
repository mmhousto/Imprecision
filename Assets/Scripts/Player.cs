using System.Collections;
using System.Collections.Generic;
using CloudSaveSample;
using UnityEngine;
using Unity.Services.CloudSave;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using System;

[Serializable]
public class Player : MonoBehaviour
{

    public string UserID { get; set; }

    public int Points { get; set; }

    public string PlayerName { get; set; }

    public string PlayerEmail { get; set; }

    public int PlayerLevel { get; set; }

    public int PlayerExperience { get; set; }

    public void SetData(string id, int points, string name, string email, int level, int xp)
    {
        UserID = id;
        Points = points;
        PlayerName = name;
        PlayerEmail = email;
        PlayerLevel = level;
        PlayerExperience = xp;
    }

    private async void OnApplicationQuit()
    {
        await ForceSaveObjectData(UserID, this);
    }

    private async Task ForceSaveObjectData(string key, Player value)
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
