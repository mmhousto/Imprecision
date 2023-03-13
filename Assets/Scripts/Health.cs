using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Com.MorganHouston.Imprecision
{
    public class Health : MonoBehaviour
    {
        public Slider healthBar;
        public TextMeshProUGUI label;

        [SerializeField]
        private int scalingfactor = 10;
        [SerializeField]
        private int healthPoints = 100;
        public int HealthPoints { get { return healthPoints; } private set { healthPoints = value; } }

        // Start is called before the first frame update
        void Start()
        {
            if (CompareTag("Player"))
            {
                HealthPoints = Player.Instance.HealthPoints;
            }
            healthBar.maxValue = HealthPoints;
            healthBar.value = HealthPoints;
            label.text = $"{HealthPoints}/{healthBar.maxValue}";
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public void TakeDamage(int damageToTake)
        {
            HealthPoints -= damageToTake;
            if (HealthPoints < 0)
                HealthPoints = 0;

            healthBar.value = HealthPoints;
            label.text = $"{HealthPoints}/{healthBar.maxValue}";

            if (HealthPoints <= 0)
            {
                Destroy(this.gameObject);
            }
        }

        
    }
}
