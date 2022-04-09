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
        public Sprite[] starImages;

        private int level;

        // Start is called before the first frame update
        void Start()
        {
            SetLevelNumberAndStars();
            
        }

        private void SetLevelNumberAndStars()
        {
            for (int i = 0; i < 50; i++)
            {
                // If level matches gameobject add one to level to get level number
                if (gameObject.name == $"Level ({i})")
                {
                    stars.sprite = starImages[Player.Instance.Levels[i]]; // Assigns Stars
                    level = i + 1;

                    // If level is first level set button to true.
                    if(i == 0)
                    {
                        button.interactable = true;
                        continue;
                    }

                    // If level before current level has no stars disable button
                    // Else enable button.
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

            // Sets level number
            levelNumberLabel.text = $"Level {level}";
        }
    }
}
