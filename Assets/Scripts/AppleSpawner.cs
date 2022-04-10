using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MorganHouston.Imprecision
{
    public class AppleSpawner : MonoBehaviour
    {

        public GameObject apple;

        public Transform[] spawnPoints;

        private int currentLevel;

        private void Awake()
        {
            currentLevel = GameManager.Instance.LevelSelected;
        }

        // Start is called before the first frame update
        void Start()
        {
            Instantiate(apple, spawnPoints[currentLevel].position, Quaternion.identity);
        }
    }
}
