using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

namespace Com.MorganHouston.Imprecision
{
    public class Health : MonoBehaviour
    {
        public Slider healthBar;
        public TextMeshProUGUI label;
        private Animator anim;

        [SerializeField]
        private int healthPoints = 100;
        public int HealthPoints { get { return healthPoints; } private set { healthPoints = value; } }

        // Start is called before the first frame update
        void Start()
        {
            if (CompareTag("Player"))
            {
                HealthPoints = Player.Instance != null ? Player.Instance.HealthPoints : 100;
                anim = GetComponent<Animator>();
            }
            else if (CompareTag("Enemy"))
            {
                healthBar = GetComponentInChildren<Slider>();
                label = GetComponentInChildren<TextMeshProUGUI>();
            }

            if(healthBar != null)
            {
                healthBar.maxValue = HealthPoints;
                healthBar.value = HealthPoints;
                label.text = $"{HealthPoints}/{healthBar.maxValue}";
            }
            
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public void TakeDamage(int damageToTake)
        {
            if (healthPoints <= 0) return;

            HealthPoints -= damageToTake;
            if (HealthPoints < 0)
                HealthPoints = 0;

            healthBar.value = HealthPoints;
            label.text = $"{HealthPoints}/{healthBar.maxValue}";

            if (HealthPoints <= 0 && CompareTag("Player"))
            {
                GetComponent<PlayerInput>().DeactivateInput();

                var rand = Random.Range(0.0f, 2.0f);
                anim.SetFloat("Death", rand);
                anim.SetTrigger("Die");

                GameManager.Instance.Invoke("GameOver", 4.2f);
            }
            else if (CompareTag("Player"))
            {
                anim.SetTrigger("Hurt");
            }
            else if (HealthPoints <= 0)
            {
                GetComponent<Enemy>().Die();
            }
        }

        
    }
}
