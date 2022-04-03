using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MorganHouston.Imprecision
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager instance;

        public static GameManager Instance { get { return instance; } }

        [SerializeField]
        private int levelSelected = 0;

        public int LevelSelected { get { return levelSelected; } private set { levelSelected = value; } }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                instance = this;
                DontDestroyOnLoad(Instance.gameObject);
            }
        }

        public void SetLevel(int level)
        {
            levelSelected = level;
        }
    }
}
