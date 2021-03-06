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

        [SerializeField]
        private int userLevel;
        public int UserLevel { get { return userLevel; } private set { userLevel = value; } }
        [SerializeField]
        private int userXP;
        public int UserXP { get { return userXP; } private set { userXP = value; } }
        public int Jewels { get; private set; }
        public int ArrowsFired { get; private set; }

        [SerializeField]
        private int[] levels;

        public int[] Levels { get { return levels; } private set { levels = value; } }

        [SerializeField]
        private int[] applesShotOnLevels;

        public int[] AppleShotOnLevels { get { return applesShotOnLevels; } private set { applesShotOnLevels = value; } }
        

        protected int maxXP;

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

        private void Start()
        {
            maxXP = UserLevel * 420;
        }

        private void Update()
        {
            if(UserID != null)
            {
                UpdateMaxXp();

                UpdateLevel();
            }
            
        }

        private void UpdateMaxXp()
        {
            if (maxXP != userLevel * 420)
                maxXP = userLevel * 420;
        }

        private void UpdateLevel()
        {
            if (UserXP >= maxXP)
            {
                UserLevel++;
                int rem = UserXP % maxXP;
                UserXP = 0 + rem;
            }
        }

        public void GainXP(int xpToAdd)
        {
            UserXP += xpToAdd;
            
        }

        public void GainPoints(int pointsToAdd)
        {
            UserPoints += pointsToAdd;
            GainXP(pointsToAdd / 10);
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

        public void SetStarForLevel(int level, int stars)
        {
            if(stars > Levels[level])
                Levels[level] = stars;
        }

        public void FiredArrow()
        {
            ArrowsFired++;
        }

    }
}
