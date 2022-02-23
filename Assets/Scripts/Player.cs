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

    private int maxXP;

    public void GainXP(int xpToAdd)
    {
        userXP += xpToAdd;
        maxXP = userLevel * 150;
        if(userXP >= maxXP)
        {
            userLevel++;
            int rem = userXP % maxXP;
            userXP = 0 + rem;
        }
    }

    public void GainPoints(int pointsToAdd)
    {
        userPoints += pointsToAdd;
    }
    
}
