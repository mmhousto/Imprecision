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

        [SerializeField]
        private int userPoints;
        public int UserPoints { get { return userPoints; } private set { userPoints = value; } }

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
        private int[] storyLevels;

        public int[] StoryLevels { get { return storyLevels; } private set { storyLevels = value; } }

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

        [SerializeField]
        private int healthPoints;
        public int HealthPoints { get { return healthPoints; } private set { healthPoints = value; } }
        private int hpMin = 100;
        private int hpMax = 1000;

        [SerializeField]
        private int attackPower;
        public int AttackPower { get { return attackPower; } private set { attackPower = value; } }
        private int apMin = 10;
        private int apMax = 250;

        [SerializeField]
        private int defensePower;
        public int DefensePower { get { return defensePower; } private set { defensePower = value; } }
        private int dpMin = 10;
        private int dpMax = 260;

        [SerializeField]
        private int attackSpeed;
        public int AttackSpeed { get { return attackSpeed; } private set { attackSpeed = value; } }
        private int asMin = 1;
        private int asMax = 100;

        [SerializeField]
        private int movementSpeed;
        public int MovementSpeed { get { return movementSpeed; } private set { movementSpeed = value; } }
        private int msMin = 1;
        private int msMax = 100;

        [SerializeField]
        private int stamina;
        public int Stamina { get { return stamina; } private set { stamina = value; } }
        private int stamMin = 75;
        private int stamMax = 500;

        [SerializeField]
        private int critChance;
        public int CritChance { get { return critChance; } private set { critChance = value; } }
        private int critMin = 1;
        private int critMax = 100;

        protected int maxXP;

        private CloudSaveLogin cloudSaveLogin;
        public bool dataLoaded;

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
            StoryLevels = new int[4];
            AppleShotOnLevels = new int[50];
            BullseyesOnLevels = new int[50];

            Power = 1;
            Dexterity = 1;
            Endurance = 1;
            Vitality = 1;
            Defense = 1;
            Luck = 1;

            SetStats();

            dataLoaded = false;
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

            if (data.storyLevels != null)
                StoryLevels = data.storyLevels;
            else
                StoryLevels = new int[4];

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

            SetStats();

            dataLoaded = true;
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
            StoryLevels = new int[4];
            AppleShotOnLevels = new int[50];
            BullseyesOnLevels = new int[50];

            Power = 1;
            Dexterity = 1;
            Endurance = 1;
            Vitality = 1;
            Defense = 1;
            Luck = 1;

            SetStats();

            dataLoaded = true;
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
            StoryLevels = new int[4];
            AppleShotOnLevels = new int[50];
            BullseyesOnLevels = new int[50];

            Power = 1;
            Dexterity = 1;
            Endurance = 1;
            Vitality = 1;
            Defense = 1;
            Luck = 1;

            SetStats();

            dataLoaded = true;
        }

        public int GetTotalLevelsBeat()
        {
            int levelsBeat = 0;
            foreach (int stars in Levels)
            {
                if (stars > 0)
                    levelsBeat++;
            }
            return levelsBeat;
        }

        public int GetTotalStars()
        {
            int starsCounted = 0;
            foreach (int stars in Levels)
            {
                starsCounted += stars;
            }
            return starsCounted;
        }

        public int GetApplesShotOnLevels()
        {
            int applesShotOnLevelsCounted = 0;
            foreach (int apple in AppleShotOnLevels)
            {
                applesShotOnLevelsCounted += apple;
            }
            return applesShotOnLevelsCounted;
        }

        public int GetPerfects()
        {
            int perfectsCounted = 0;
            foreach (int perfect in BullseyesOnLevels)
            {
                perfectsCounted += perfect;
            }
            return perfectsCounted;
        }

        public int GetThreeStars()
        {
            int threeStarsCounted = 0;
            foreach (int stars in Levels)
            {
                if (stars == 3)
                {
                    threeStarsCounted++;
                }
            }
            return threeStarsCounted;
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

        public void SetStarForStoryLevel(int level, int stars)
        {
            if (stars > StoryLevels[level])
                StoryLevels[level] = stars;

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

        public void SetStats()
        {
            SetHealthPoints();
            SetAttackPower();
            SetDefensePower();
            SetAttackSpeed();
            SetMovementSpeed();
            SetStamina();
            SetCritChance();
        }

        public int SetStatValue(int attributeLevel, int statMin, int statMax)
        {
            int statValue = Mathf.RoundToInt(statMin + (statMax - statMin) * (attributeLevel / 100f));
            return statValue;
        }

        public int SetStatValue(int attributeLevel1, int statMin, int statMax, int attributeLevel2, int attributeWeight1, int attributeWeight2)
        {
            int statValue = Mathf.RoundToInt(statMin + (statMax - statMin) * ((attributeLevel1 * attributeWeight1 + attributeLevel2 * attributeWeight2)
                / (100f * (attributeWeight1 + attributeWeight2))));
            return statValue;
        }

        public int SetStatValue(int attributeLevel1, int statMin, int statMax, int attributeLevel2, int attributeWeight1, int attributeWeight2, int attributeLevel3, int attributeWeight3)
        {
            int statValue = Mathf.RoundToInt(statMin + (statMax - statMin) * ((attributeLevel1 * attributeWeight1 + attributeLevel2 * attributeWeight2 +
                attributeLevel3 * attributeWeight3) / (100f * (attributeWeight1 + attributeWeight2 + attributeWeight3))));
            return statValue;
        }

        public int SetStatValue(int attributeLevel1, int statMin, int statMax, int attributeLevel2, int attributeWeight1, int attributeWeight2, int attributeLevel3, int attributeWeight3, int attributeLevel4, int attributeWeight4)
        {
            int statValue = Mathf.RoundToInt(statMin + (statMax - statMin) * ((attributeLevel1 * attributeWeight1 + attributeLevel2 * attributeWeight2 +
                attributeLevel3 * attributeWeight3 + attributeLevel4 * attributeWeight4) / (100f * (attributeWeight1 + attributeWeight2 + attributeWeight3 + attributeWeight4))));
            return statValue;
        }

        public void SetHealthPoints()
        {
            HealthPoints = SetStatValue(Vitality, hpMin, hpMax, Power, 5, 1, Endurance, 1);
        }

        public void SetAttackPower()
        {
            AttackPower = SetStatValue(Power, apMin, apMax, Defense, 5, 1, Dexterity, 1, Vitality, 1);
        }

        public void SetAttackSpeed()
        {
            AttackSpeed = SetStatValue(Dexterity, asMin, asMax);
        }

        public void SetDefensePower()
        {
            DefensePower = SetStatValue(Defense, dpMin, dpMax, Power, 5, 1, Vitality, 1, Luck, 1);
        }

        public void SetMovementSpeed()
        {
            MovementSpeed = SetStatValue(Dexterity, msMin, msMax, Endurance, 1, 3);
        }

        public void SetStamina()
        {
            Stamina = SetStatValue(Endurance, stamMin, stamMax);
        }

        public void SetCritChance()
        {
            CritChance = SetStatValue(Luck, critMin, critMax, Dexterity, 3, 1);
        }



        public void IncreaseAttribute(int attributeToIncrease)
        {
            switch (attributeToIncrease)
            {
                // POWER
                case 0:
                    Power++;
                    SetAttackPower();
                    SetDefensePower();
                    SetHealthPoints();
                    break;

                // DEXTERITY
                case 1:
                    Dexterity++;
                    SetAttackPower();
                    SetAttackSpeed();
                    SetMovementSpeed();
                    SetCritChance();
                    break;

                // ENDURANCE
                case 2:
                    Endurance++;
                    SetMovementSpeed();
                    SetStamina();
                    SetHealthPoints();
                    break;

                // VITALITY
                case 3:
                    Vitality++;
                    SetHealthPoints();
                    SetAttackPower();
                    SetDefensePower();
                    break;

                // DEFENSE
                case 4:
                    Defense++;
                    SetDefensePower();
                    SetAttackPower();
                    break;

                // LUCK
                case 5:
                    Luck++;
                    SetCritChance();
                    SetDefensePower();
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
                    SetAttackPower();
                    SetDefensePower();
                    SetHealthPoints();
                    break;

                // DEXTERITY
                case 1:
                    Dexterity--;
                    SetAttackPower();
                    SetAttackSpeed();
                    SetMovementSpeed();
                    SetCritChance();
                    break;

                // ENDURANCE
                case 2:
                    Endurance--;
                    SetMovementSpeed();
                    SetStamina();
                    SetHealthPoints();
                    break;

                // VITALITY
                case 3:
                    Vitality--;
                    SetHealthPoints();
                    SetAttackPower();
                    SetDefensePower();

                    break;

                // DEFENSE
                case 4:
                    Defense--;
                    SetDefensePower();
                    SetAttackPower();
                    break;

                // LUCK
                case 5:
                    Luck--;
                    SetCritChance();
                    SetDefensePower();
                    break;
            }
        }
    }
}
