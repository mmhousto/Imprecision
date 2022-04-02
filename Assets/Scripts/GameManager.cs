using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MorganHouston.Imprecision
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager instance;

        public static GameManager Instance { get { return instance; } }

        public int levelSelected = 0;

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
