using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace Com.MorganHouston.Imprecision
{
    public class Enemy : MonoBehaviour
    {        
        public Transform target;
        public float followDistance = 5f; // the distance at which the spider will start following the player
        public float attackDistance = 2f; // the distance at which the spider will start attacking the player
        public float attackTime = 0;

        private Health health;
        private enum AIState { Idle, Follow, Attack }; // enum for the spider's states
        private AIState currentState = AIState.Idle; // current state of the spider
        private Vector3 spawnLocation;
        private NavMeshAgent agent;
        private bool canAttack;

        [SerializeField]
        private int scalingfactor = 10;

        [SerializeField]
        private int attackPower;
        public int AttackPower { get { return attackPower; } private set { attackPower = value; } }

        [SerializeField]
        private int defensePower;
        public int DefensePower { get { return defensePower; } private set { defensePower = value; } }

        [SerializeField]
        private float attackSpeed;
        public float AttackSpeed { get { return attackSpeed; } private set { attackSpeed = value; } }

        [SerializeField]
        private int movementSpeed;
        public int MovementSpeed { get { return movementSpeed; } private set { movementSpeed = value; } }

        [SerializeField]
        private int stamina;
        public int Stamina { get { return stamina; } private set { stamina = value; } }

        [SerializeField]
        private int critChance;
        public int CritChance { get { return critChance; } private set { critChance = value; } }

        enum HitArea
        {
            Head,
            Body,
            Legs
        }

        // Start is called before the first frame update
        void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            spawnLocation = transform.position;
            agent.Warp(spawnLocation);
            agent.speed = movementSpeed;
            health = GetComponent<Health>();
            attackSpeed = 5 - (attackSpeed / 100)*4;
            attackTime = 0;
        }

        // Update is called once per frame
        void Update()
        {
            CheckCanAttack();
            DetermineState();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("arrow"))
            {
                Collider myCollider = collision.GetContact(0).thisCollider;
                if(myCollider.name == "Head")
                {
                    health.TakeDamage(health.HealthPoints);
                }
                else if (myCollider.name == "Body")
                {
                    health.TakeDamage(DetermineDamageToTake((int)HitArea.Body));
                }
                else
                {
                    health.TakeDamage(DetermineDamageToTake((int)HitArea.Legs));
                }
                Destroy(collision.gameObject);
            }
        }

        private void DetermineState()
        {
            if(target == null)
            {
                FollowTarget(spawnLocation);
            }
            else
            {
                float distanceToPlayer = Vector3.Distance(transform.position, target.position);

                switch (currentState)
                {
                    case AIState.Idle:
                        FollowTarget(spawnLocation);
                        // do idle behavior (e.g. stay in place and wait for player to get close)
                        if (distanceToPlayer < followDistance)
                        {
                            currentState = AIState.Follow;
                        }
                        break;
                    case AIState.Follow:
                        // do follow behavior (e.g. move towards player)
                        FollowTarget(target.position);
                        if (distanceToPlayer < attackDistance)
                        {
                            currentState = AIState.Attack;
                        }
                        break;
                    case AIState.Attack:
                        if (canAttack)
                            JumpAndAttack();
                        else
                        {
                            agent.updatePosition = true;
                            agent.updateRotation = true;
                            agent.isStopped = false;
                            FollowTarget(target.position);
                        }
                        break;
                    default:
                        currentState = AIState.Idle;
                        break;
                }
            }
            
        }

        private void CheckCanAttack()
        {
            if(attackTime <= 0)
            {
                attackTime = 0;
                canAttack = true;
            }
            else
            {
                attackTime -= Time.deltaTime;
                canAttack = false;
            }
        }

        private void FollowTarget(Vector3 targetPos)
        {
            if(targetPos != null)
                agent.SetDestination(targetPos);
        }

        private void JumpAndAttack()
        {
            if (agent.enabled)
            {
                // set the agents target to where you are before the jump
                // this stops her before she jumps. Alternatively, you could
                // cache this value, and set it again once the jump is complete
                // to continue the original move
                agent.SetDestination(transform.position);
                // disable the agent
                agent.updatePosition = false;
                agent.updateRotation = false;
                agent.isStopped = true;
            }

            // make the spider jump towards the player
            Vector3 directionToPlayer = (target.position - transform.position).normalized;
            GetComponent<Rigidbody>().AddForce(directionToPlayer * 5 + Vector3.up * 5, ForceMode.Impulse);

            // check if the player is within attack range
            float distanceToPlayer = Vector3.Distance(transform.position, target.position);
            if (distanceToPlayer <= 1.5f)
            {
                // perform the attack
                target.GetComponent<Health>().TakeDamage(DetermineDamageToDeal());
            }

            attackTime = attackSpeed;
            canAttack = false;

        }

        private int DetermineDamageToDeal()
        {
            Player player = Player.Instance;

            int damage = 0;
            float critValue = UnityEngine.Random.value;
            float randValue = UnityEngine.Random.Range(1, 12);
            float crit = CritChance / 100f;

            if (critValue < (1f - crit))
            {
                damage = (int)(randValue + AttackPower) - (player.DefensePower * (player.UserLevel / scalingfactor));
            }
            else
            {
                damage = ((int)(randValue + AttackPower) - (player.DefensePower * (player.UserLevel / scalingfactor))) * 2;
            }

            if (damage < 0)
            {
                damage = 0;
            }

            return damage;
        }

        private int DetermineDamageToTake(int hitZone)
        {
            Player player = Player.Instance;
            float randValue = 10;

            switch (hitZone)
            {
                case 0:
                    randValue = 22;
                    break;
                case 1:
                    randValue = UnityEngine.Random.Range(12, 22);
                    break;
                case 2:
                    randValue = UnityEngine.Random.Range(1, 12);
                    break;
                default:
                    randValue = UnityEngine.Random.Range(1, 22);
                    break;
            }

            int damage = 0;
            float critValue = UnityEngine.Random.value;
            
            float crit = player.CritChance/100f;

            if (critValue < (1f - crit))
            {
                damage = (int)(randValue + player.AttackPower) - (DefensePower * (player.UserLevel / scalingfactor));
            }
            else
            {
                damage = (int)((randValue + player.AttackPower) - (DefensePower * (player.UserLevel / scalingfactor)))*2;
            }

            if(damage < 0)
            {
                damage = 0;
            }

            return damage;
        }
    }
}
