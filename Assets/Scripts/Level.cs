using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Com.MorganHouston.Imprecision
{
    public class Level : MonoBehaviour
    {

        public TextMeshProUGUI levelNumberLabel;
        public Image stars;
        public Button button;

        private int level;

        // Start is called before the first frame update
        void Start()
        {
            SetLevelNumber();
            
        }

        private void SetLevelNumber()
        {
            for (int i = 0; i < 50; i++)
            {
                if (gameObject.name == $"Level ({i})")
                {
                    level = i + 1;
                    if(i == 0)
                    {
                        button.interactable = true;
                        continue;
                    }

                    if (Player.Instance.Levels[i - 1] == 0)
                    {
                        button.interactable = false;
                    }
                    else
                    {
                        button.interactable = true;
                    }
                }

                
            }

            levelNumberLabel.text = $"Level {level}";
        }
    }
}
