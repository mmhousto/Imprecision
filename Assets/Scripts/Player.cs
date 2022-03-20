using System.Collections;
using System.Collections.Generic;
using CloudSaveSample;
using UnityEngine;
using Unity.Services.CloudSave;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using System;

namespace Com.MorganHouston.Imprecision
{

    public class Player : MonoBehaviour
    {
        private static Player instance;

        public static Player Instance { get { return instance; } }

        public string userID;
        public string userName;

        public int userPoints;
        public int userLevel;
        public int userXP;

        private int maxXP;

        public CloudSaveLogin cloudSaveLogin;

        private void Awake()
        {
            if(instance != null && instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                instance = this;
                DontDestroyOnLoad(Instance.gameObject);
            }
        }

        public void GainXP(int xpToAdd)
        {
            userXP += xpToAdd;
            maxXP = userLevel * 150;
            if (userXP >= maxXP)
            {
                userLevel++;
                int rem = userXP % maxXP;
                userXP = 0 + rem;
            }
            cloudSaveLogin.SaveCloudData();
        }

        public void GainPoints(int pointsToAdd)
        {
            userPoints += pointsToAdd;
            cloudSaveLogin.SaveCloudData();
        }

    }
}
