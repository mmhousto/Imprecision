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

        public string UserID { get; private set; }
        public string UserName { get; private set; }

        public int UserPoints { get; private set; }
        public int UserLevel { get; private set; }
        public int UserXP { get; private set; }
        public int Jewels { get; private set; }
        public int ArrowsFired { get; private set; }

        [SerializeField]
        private int[] levels;

        public int[] Levels { get { return levels; } private set { levels = value; } }

        [SerializeField]
        private int[] applesShotOnLevels;

        public int[] AppleShotOnLevels { get { return applesShotOnLevels; } private set { applesShotOnLevels = value; } }
        

        private int maxXP;

        private CloudSaveLogin cloudSaveLogin;

        // Resets Player data.
        public void SetData()
        {
            UserID = "";
            UserName = "";
            UserPoints = 0;
            UserLevel = 1;
            UserXP = 0;
            Jewels = 0;
            ArrowsFired = 0;
            Levels = new int[50];
            AppleShotOnLevels = new int[50];
        }

        // Loads player from cloud save/ local save
        public void SetData(SavePlayerData data)
        {
            UserID = data.userID;
            UserName = data.userName;
            UserPoints = data.userPoints;
            UserLevel = data.userLevel;
            UserXP = data.userXP;
            Jewels = data.jewels;
            ArrowsFired = data.arrowsFired;

            if (data.levels != null)
                Levels = data.levels;
            else
                Levels = new int[50];

            if (data.appleShotOnLevels != null)
                AppleShotOnLevels = data.appleShotOnLevels;
            else
                AppleShotOnLevels = new int[50];
        }

        // Creates an anonymous player.
        public void SetData(string id)
        {
            UserID = id;
            UserName = $"Guest_{id}";
            UserPoints = 0;
            UserLevel = 1;
            UserXP = 0;
            Jewels = 0;
            ArrowsFired = 0;
            Levels = new int[50];
            AppleShotOnLevels = new int[50];
        }

        // Creates new player w/ SSO login.
        public void SetData(string id, string name)
        {
            UserID = id;
            UserName = name;
            UserPoints = 0;
            UserLevel = 1;
            UserXP = 0;
            Jewels = 0;
            ArrowsFired = 0;
            Levels = new int[50];
            AppleShotOnLevels = new int[50];
        }


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
            cloudSaveLogin = GetComponent<CloudSaveLogin>();
        }

        public void GainXP(int xpToAdd)
        {
            UserXP += xpToAdd;
            maxXP = UserLevel * 150;
            if (UserXP >= maxXP)
            {
                UserLevel++;
                int rem = UserXP % maxXP;
                UserXP = 0 + rem;
            }
            cloudSaveLogin.SaveCloudData();
        }

        public void GainPoints(int pointsToAdd)
        {
            UserPoints += pointsToAdd;
            cloudSaveLogin.SaveCloudData();
        }

        public void SetPlayerName(string name)
        {
            UserName = name;
        }

        public void GainJewels(int jewelsToAdd)
        {
            Jewels += jewelsToAdd;
            cloudSaveLogin.SaveCloudData();
        }

    }
}
