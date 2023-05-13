using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MorganHouston.Imprecision
{
    public class Weapon : MonoBehaviour
    {
        private Health playerHealth;
        private Enemy enemy;
        bool playerHit;

        private void Start()
        {
            playerHealth = GameObject.FindWithTag("Player").GetComponent<Health>();
            enemy = transform.root.GetComponentInChildren<Enemy>();
            playerHit = false;
        }

        private void Update()
        {
            ResetPlayerHit();
        }

        private void OnTriggerEnter(Collider other)
        {
            // Checks if weapon collided with player while attacking, then hurts player
            if (other.CompareTag("Player") && enemy.attacking && playerHit == false)
            {
                playerHit = true;
                playerHealth.TakeDamage(enemy.DetermineDamageToDeal());
            }
        }

        private void ResetPlayerHit()
        {
            // Resets player hit after attack
            if (playerHit == true && enemy.attacking == false)
            {
                playerHit = false;
            }
        }

    }
}
