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
        [SerializeField]
        private int arrowsFired;
        public int ArrowsFired { get { return arrowsFired; } private set { arrowsFired = value; } }
        public int TargetsHit { get; private set; }
        public int BullseyesHit { get; set; }


        [SerializeField]
        private int[] levels;

        public int[] Levels { get { return levels; } private set { levels = value; } }

        [SerializeField]
        private int[] applesShotOnLevels;

        public int[] AppleShotOnLevels { get { return applesShotOnLevels; } private set { applesShotOnLevels = value; } }

        [SerializeField]
        private int[] bullseyesOnLevels;

        public int[] BullseyesOnLevels { get { return bullseyesOnLevels; } private set { bullseyesOnLevels = value; } }

        [SerializeField]
        private int power;
        public int Power { get { return power; } private set { power = value; } }

        [SerializeField]
        private int dexterity;
        public int Dexterity { get { return dexterity; } private set { dexterity = value; } }

        [SerializeField]
        private int endurance;
        public int Endurance { get { return endurance; } private set { endurance = value; } }

        [SerializeField]
        private int vitality;
        public int Vitality { get { return vitality; } private set { vitality = value; } }

        [SerializeField]
        private int defense;
        public int Defense { get { return defense; } private set { defense = value; } }

        [SerializeField]
        private int luck;
        public int Luck { get { return luck; } private set { luck = value; } }

        public int HealthPoints { get; private set; }
        public int AttackPower { get; private set; }
        public int DefensePower { get; private set; }
        public int AttackSpeed { get; private set; }
        public int MovementSpeed { get; private set; }
        public int Stamina { get; private set; }
        public int CritChance { get; private set; }

        protected int maxXP;

        private CloudSaveLogin cloudSaveLogin;

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
            TargetsHit = 0;
            BullseyesHit = 0;
            Levels = new int[50];
            AppleShotOnLevels = new int[50];
            BullseyesOnLevels = new int[50];

            Power = 1;
            Dexterity = 1;
            Endurance = 1;
            Vitality = 1;
            Defense = 1;
            Luck = 1;

            HealthPoints = 100;
            AttackPower = 10;
            DefensePower = 10;
            AttackSpeed = 10;
            MovementSpeed = 5;
            Stamina = 75;
            CritChance = 5;
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
            TargetsHit = data.targetsHit;
            BullseyesHit = data.bullseyesHit;

            if (data.levels != null)
                Levels = data.levels;
            else
                Levels = new int[50];

            if (data.bullseyesOnLevels != null)
                BullseyesOnLevels = data.bullseyesOnLevels;
            else
                BullseyesOnLevels = new int[50];

            if (data.appleShotOnLevels != null)
                AppleShotOnLevels = data.appleShotOnLevels;
            else
                AppleShotOnLevels = new int[50];

            Power = data.power;
            Dexterity = data.dexterity;
            Endurance = data.endurance;
            Vitality = data.vitality;
            Defense = data.defense;
            Luck = data.luck;

            HealthPoints = data.healthPoints;
            AttackPower = data.attackPower;
            DefensePower = data.defensePower;
            AttackSpeed = data.attackSpeed;
            MovementSpeed = data.movementSpeed;
            Stamina = data.stamina;
            CritChance = data.critChance;
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
            TargetsHit = 0;
            BullseyesHit = 0;
            Levels = new int[50];
            AppleShotOnLevels = new int[50];
            BullseyesOnLevels = new int[50];

            Power = 1;
            Dexterity = 1;
            Endurance = 1;
            Vitality = 1;
            Defense = 1;
            Luck = 1;

            HealthPoints = 100;
            AttackPower = 10;
            DefensePower = 10;
            AttackSpeed = 10;
            MovementSpeed = 5;
            Stamina = 75;
            CritChance = 5;
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
            TargetsHit = 0;
            BullseyesHit = 0;
            Levels = new int[50];
            AppleShotOnLevels = new int[50];
            BullseyesOnLevels = new int[50];

            Power = 1;
            Dexterity = 1;
            Endurance = 1;
            Vitality = 1;
            Defense = 1;
            Luck = 1;

            HealthPoints = 100;
            AttackPower = 10;
            DefensePower = 10;
            AttackSpeed = 10;
            MovementSpeed = 5;
            Stamina = 75;
            CritChance = 5;
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

#if (UNITY_IOS || UNITY_ANDROID)
            LeaderboardManager.UnlockJewel();
#endif
        }

        public void SetStarForLevel(int level, int stars)
        {
            if(stars > Levels[level])
                Levels[level] = stars;

#if (UNITY_IOS || UNITY_ANDROID)
            LeaderboardManager.CheckArcherAchievements();
#endif
        }

        public void SetBullseyeForLevel(int level, int perfect)
        {
            if (perfect > BullseyesOnLevels[level])
                BullseyesOnLevels[level] = perfect;
        }

        public void FiredArrow()
        {
            ArrowsFired++;
        }

        public void HitTarget()
        {
            TargetsHit++;
        }

        public void HitBullseye()
        {
#if (UNITY_IOS || UNITY_ANDROID)
            if (BullseyesHit <= 0)
            {
                LeaderboardManager.UnlockBullseye();
            }
#endif

            BullseyesHit++;
            
        }

        public void SetUserPoints(int newValue)
        {
            UserPoints = newValue;
        }

        public void SetAttribute(int attribute, int newValue)
        {
            
            switch (attribute)
            {
                // POWER
                case 0:
                    Power = newValue;
                    break;

                // DEXTERITY
                case 1:
                    Dexterity = newValue;
                    break;

                // ENDURANCE
                case 2:
                    Endurance = newValue;
                    break;

                // VITALITY
                case 3:
                    Vitality = newValue;

                    break;

                // DEFENSE
                case 4:
                    Defense = newValue;
                    break;

                // LUCK
                case 5:
                    Luck = newValue;
                    break;
            }
        }

        public void SetHealthPoints(int newValue)
        {
            HealthPoints = newValue;
        }

        public void SetAttackPower(int newValue)
        {
            AttackPower = newValue;
        }

        public void SetAttackSpeed(int newValue)
        {
            AttackSpeed = newValue;
        }

        public void SetDefensePower(int newValue)
        {
            DefensePower = newValue;
        }

        public void SetMovementSpeed(int newValue)
        {
            MovementSpeed = newValue;
        }

        public void SetStamina(int newValue)
        {
            AttackPower = newValue;
        }

        public void SetCritChance(int newValue)
        {
            CritChance = newValue;
        }

        public void IncreaseAttribute(int attributeToIncrease)
        {
            switch (attributeToIncrease)
            {
                // POWER
                case 0:
                    Power++;
                    AttackPower += (Power % 3 == 0) ? 6 : 5;
                    DefensePower += (Power % 3 == 0) ? 3 : 2;
                    break;

                // DEXTERITY
                case 1:
                    Dexterity++;
                    AttackPower += 2;
                    MovementSpeed += (Dexterity % 3 == 0) ? 2 : 1;
                    AttackSpeed += (Dexterity % 3 == 0) ? 5 : 4;
                    break;

                // ENDURANCE
                case 2:
                    Endurance++;
                    MovementSpeed += (Endurance % 3 == 0) ? 4 : 3;
                    Stamina += (Endurance % 3 == 0) ? 4 : 3;
                    HealthPoints++;
                    break;

                // VITALITY
                case 3:
                    Vitality++;
                    HealthPoints += (Vitality % 3 == 0) ? 7 : 5;
                    AttackPower++;
                    DefensePower++;

                    break;

                // DEFENSE
                case 4:
                    Defense++;
                    AttackPower += (Defense % 3 == 0) ? 2 : 1;
                    DefensePower += (Defense % 3 == 0) ? 6 : 5;
                    HealthPoints++;
                    break;

                // LUCK
                case 5:
                    Luck++;
                    CritChance += (Luck % 3 == 0) ? 8 : 6;
                    HealthPoints++;
                    break;
            }
        }

        public void DecreaseAttribute(int attributeToIncrease)
        {
            switch (attributeToIncrease)
            {
                // POWER
                case 0:
                    Power--;
                    AttackPower -= ((Power + 1) % 3 == 0) ? 6 : 5;
                    DefensePower -= ((Power + 1) % 3 == 0) ? 3 : 2;
                    break;

                // DEXTERITY
                case 1:
                    Dexterity--;
                    AttackPower -= 2;
                    MovementSpeed -= ((Dexterity + 1) % 3 == 0) ? 2 : 1;
                    AttackSpeed -= ((Dexterity + 1) % 3 == 0) ? 5 : 4;
                    break;

                // ENDURANCE
                case 2:
                    Endurance--;
                    MovementSpeed -= ((Endurance + 1) % 3 == 0) ? 4 : 3;
                    Stamina -= ((Endurance + 1) % 3 == 0) ? 4 : 3;
                    HealthPoints--;
                    break;

                // VITALITY
                case 3:
                    Vitality--;
                    HealthPoints -= ((Vitality + 1) % 3 == 0) ? 7 : 5;
                    AttackPower--;
                    DefensePower--;

                    break;

                // DEFENSE
                case 4:
                    Defense--;
                    AttackPower -= ((Defense + 1) % 3 == 0) ? 2 : 1;
                    DefensePower -= ((Defense + 1) % 3 == 0) ? 6 : 5;
                    HealthPoints--;
                    break;

                // LUCK
                case 5:
                    Luck--;
                    CritChance -= ((Luck + 1) % 3 == 0) ? 8 : 6;
                    HealthPoints--;
                    break;
            }
        }
    }
}
