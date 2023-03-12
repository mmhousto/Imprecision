using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Com.MorganHouston.Imprecision
{
    public class Enemy : MonoBehaviour
    {

        public Transform target;
        private Vector3 spawnLocation;
        private NavMeshAgent agent;
        [SerializeField] private bool following;
        [SerializeField]
        private int scalingfactor = 10;
        [SerializeField]
        private int healthPoints = 100;
        public int HealthPoints { get { return healthPoints; } private set { healthPoints = value; } }

        [SerializeField]
        private int attackPower;
        public int AttackPower { get { return attackPower; } private set { attackPower = value; } }

        [SerializeField]
        private int defensePower;
        public int DefensePower { get { return defensePower; } private set { defensePower = value; } }

        [SerializeField]
        private int attackSpeed;
        public int AttackSpeed { get { return attackSpeed; } private set { attackSpeed = value; } }

        [SerializeField]
        private int movementSpeed;
        public int MovementSpeed { get { return movementSpeed; } private set { movementSpeed = value; } }

        [SerializeField]
        private int stamina;
        public int Stamina { get { return stamina; } private set { stamina = value; } }

        [SerializeField]
        private int critChance;
        public int CritChance { get { return critChance; } private set { critChance = value; } }

        // Start is called before the first frame update
        void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            spawnLocation = transform.position;
            agent.Warp(spawnLocation);
            agent.speed = movementSpeed;
        }

        // Update is called once per frame
        void Update()
        {
            Follow();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("arrow"))
            {
                Collider myCollider = collision.GetContact(0).thisCollider;
                if(myCollider.name == "Head")
                {
                    TakeDamage(HealthPoints);
                }
                else if (myCollider.name == "Body")
                {
                    TakeDamage(DetermineDamage());
                }
                Destroy(collision.gameObject);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Player") && following != true)
            {
                following = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                following = false;
            }
        }

        private void Follow()
        {
            if (following == true)
            {
                FollowTarget(target.position);
            }
            else
            {
                FollowTarget(spawnLocation);
            }
        }

        private void FollowTarget(Vector3 targetPos)
        {
            agent.SetDestination(targetPos);
        }

        private void TakeDamage(int damageToTake)
        {
            HealthPoints -= damageToTake;
            Debug.Log(HealthPoints);
            if(HealthPoints <= 0)
            {
                Destroy(this.gameObject);
            }
        }

        private int DetermineDamage()
        {
            Player player = Player.Instance;

            int damage = 0;
            float critValue = UnityEngine.Random.value;
            float randValue = UnityEngine.Random.Range(1, 22);
            float crit = player.CritChance/100f;

            Debug.Log($"CritChance: {crit}, Crit Rand Value: {critValue}");
            if (critValue < (1f - crit))
            {
                damage = (int)(randValue + player.AttackPower) - (DefensePower * (player.UserLevel / scalingfactor));
            }
            else
            {
                damage = (int)((randValue + player.AttackPower) - (DefensePower * (player.UserLevel / scalingfactor)))*2;
                Debug.Log("CRITICAL HIT!");
            }

            if(damage < 0)
            {
                damage = 0;
            }

            Debug.Log("Damage: " + damage);

            return damage;
        }
    }
}
