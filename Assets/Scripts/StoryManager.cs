using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

namespace Com.MorganHouston.Imprecision
{
    public class StoryManager : MonoBehaviour
    {
        private static StoryManager instance;

        public static StoryManager Instance { get { return instance; } }

        public UnityEvent miniBossDefeatedEvent;
        public GameObject miniBoss;
        public TextMeshProUGUI currentTimeLabel;
        public float timeToBeat;

        public static List<float> currentRunTimes = new List<float>();

        [SerializeField] GameObject[] enemies;
        [SerializeField] bool miniBossDefeated;
        [SerializeField] bool allEnemiesDefeated;
        [SerializeField] bool beatInTime;
        [SerializeField]public bool finishedLevel;
        public float currentTime;
        int hours, minutes, seconds, milliseconds;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                instance = this;
            }
        }


        // Start is called before the first frame update
        void Start()
        {
            if (miniBossDefeatedEvent == null)
                miniBossDefeatedEvent = new UnityEvent();

            miniBossDefeatedEvent.AddListener(DefeatedMiniBoss);

            enemies = GameObject.FindGameObjectsWithTag("Enemy");
            beatInTime = true;
            allEnemiesDefeated = false;
            miniBossDefeated = false;
            finishedLevel = false;
        }

        // Update is called once per frame
        void Update()
        {
            HandleTime();
            CheckObjectivesStatus();
        }

        public void CutsceneStart()
        {
            AudioManager.Instance.GetComponent<AudioSource>().Pause();
        }

        public void LevelStarted()
        {
            currentTime = 0;
            AudioManager.Instance.GetComponent<AudioSource>().Play();
        }

        private void OnDisable()
        {
            miniBossDefeatedEvent.RemoveAllListeners();
        }

        public void LevelComplete()
        {
            finishedLevel = true;
            currentRunTimes.Add(currentTime);
            Player.Instance.SetLevelTime(currentRunTimes.Count, currentTime);

            if (currentRunTimes.Count == 4) Player.Instance.SetLevelTime(0, GetTotalTime());

            GameManager.Instance?.GameOver();
        }

        public float GetTotalTime()
        {
            float totalTime = 0;
            foreach (float time in currentRunTimes)
            {
                totalTime += time;
            }
            return totalTime;
        }

        void CheckObjectivesStatus()
        {
            if(miniBoss == null && miniBossDefeated == false)
            {
                miniBossDefeatedEvent.Invoke();
            }

            if(enemies.Length <= 0 && allEnemiesDefeated == false)
            {
                allEnemiesDefeated = true;
            }

            if(currentTime > timeToBeat)
            {
                beatInTime = false;
            }
            else
            {
                beatInTime = true;
            }
        }

        void DefeatedMiniBoss()
        {
            miniBossDefeated = true;
        }

        void HandleTime()
        {
            if(GameManager.Instance?.isGameOver == false)
            {
                currentTime += Time.deltaTime;
                
            }

            hours = Mathf.FloorToInt(currentTime / 3600f);
            minutes = Mathf.FloorToInt((currentTime - (hours * 3600f)) / 60f);
            seconds = Mathf.FloorToInt(currentTime - (hours * 3600f) - (minutes * 60f));
            milliseconds = Mathf.FloorToInt((currentTime - Mathf.Floor(currentTime)) * 1000f);
            currentTimeLabel.text = $"TIME: {hours}:{minutes}:{seconds}.{milliseconds}";
        }

        public int GetStarRating()
        {
            int stars = 0;
            if (beatInTime && finishedLevel)
                stars++;
            if (allEnemiesDefeated)
                stars++;
            if (miniBossDefeated)
                stars++;

            return stars;
        }

    }
}
